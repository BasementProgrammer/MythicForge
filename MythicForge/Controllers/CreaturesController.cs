using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using MythicForge.Models;
using MythicForge.Services;
using MythicForge.ViewModels;

namespace MythicForge.Controllers
{
    public class CreaturesController : BaseController
    {
        private readonly BedrockImageService _imageService = new BedrockImageService();

        // GET: Creatures
        public ActionResult Index()
        {
            var creatures = Db.CreatureTypes
                .OrderBy(c => c.DisplayOrder)
                .ToList();

            return View(creatures);
        }

        // GET: Creatures/Customize/5
        public ActionResult Customize(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = BuildCustomizeViewModel(id.Value);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        // POST: Creatures/Customize/5  (Add to cart)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Customize(BuildCreatureViewModel form)
        {
            var creature = Db.CreatureTypes
                .Include(c => c.OptionCategories.Select(oc => oc.Options))
                .FirstOrDefault(c => c.Id == form.CreatureTypeId);

            if (creature == null)
            {
                return HttpNotFound();
            }

            var color = Db.Colors.FirstOrDefault(c => c.Id == form.ColorId);
            if (color == null)
            {
                ModelState.AddModelError("ColorId", "Please choose a color.");
            }

            var selectedIds = form.SelectedOptionIds ?? new List<int>();

            // Build the final option list per category, enforcing selection rules:
            // - Single categories keep at most one option (and must have one if required)
            // - Multiple categories keep every selected option
            var validOptions = new List<CreatureOption>();
            foreach (var category in creature.OptionCategories.OrderBy(c => c.DisplayOrder))
            {
                var chosen = category.Options.Where(o => selectedIds.Contains(o.Id)).ToList();

                if (category.SelectionType == SelectionType.Single)
                {
                    var one = chosen.FirstOrDefault();
                    if (one != null)
                    {
                        validOptions.Add(one);
                    }
                    else if (category.IsRequired)
                    {
                        ModelState.AddModelError("", "Please choose a " + category.Name + ".");
                    }
                }
                else
                {
                    validOptions.AddRange(chosen);
                }
            }

            if (!ModelState.IsValid)
            {
                var vm = BuildCustomizeViewModel(form.CreatureTypeId);
                return View(vm);
            }

            var unitPrice = PricingService.CalculatePrice(creature, color, validOptions);
            var summary = string.Join(", ", validOptions.Select(o => o.OptionCategory.Name + ": " + o.Name));

            var line = new CartLine
            {
                CreatureTypeId = creature.Id,
                CreatureTypeName = creature.Name,
                Emoji = creature.Emoji,
                ColorId = color.Id,
                ColorName = color.Name,
                ColorHex = color.HexValue,
                CreatureName = string.IsNullOrWhiteSpace(form.CreatureName) ? creature.Name : form.CreatureName.Trim(),
                OptionIds = validOptions.Select(o => o.Id).ToList(),
                OptionsSummary = summary,
                UnitPrice = unitPrice,
                Quantity = 1
            };

            Cart.AddLine(line);
            TempData["Message"] = line.CreatureName + " was added to your cart.";
            return RedirectToAction("Index", "Cart");
        }

        // POST: Creatures/Preview  (AJAX) -> generates a live image with Amazon Bedrock.
        [HttpPost]
        public async Task<ActionResult> Preview(int creatureTypeId, int colorId, int[] selectedOptionIds)
        {
            try
            {
                var creature = Db.CreatureTypes
                    .Include(c => c.OptionCategories.Select(oc => oc.Options))
                    .FirstOrDefault(c => c.Id == creatureTypeId);

                if (creature == null)
                {
                    return JsonPayload(new { ok = false, error = "Unknown creature." });
                }

                var color = Db.Colors.FirstOrDefault(c => c.Id == colorId);
                var ids = selectedOptionIds ?? new int[0];

                // Gather selected options with their category and description so the prompt
                // can phrase each one clearly (e.g. "wielding a great club" for a Weapon,
                // "grey fur" for Fur) rather than a bare word the model may ignore.
                var features = new List<PromptFeature>();
                foreach (var category in creature.OptionCategories.OrderBy(c => c.DisplayOrder))
                {
                    foreach (var option in category.Options.Where(o => ids.Contains(o.Id)))
                    {
                        features.Add(new PromptFeature
                        {
                            Category = category.Name,
                            Name = option.Name,
                            Description = option.Description
                        });
                    }
                }

                var prompt = BuildPrompt(creature, color?.Name, features);
                var negativePrompt = BuildNegativePrompt(features);
                var dataUri = await _imageService.GenerateImageDataUriAsync(prompt, negativePrompt);

                return JsonPayload(new { ok = true, image = dataUri, prompt });
            }
            catch (Exception ex)
            {
                // Return a friendly payload so the page can show a message instead of failing.
                return JsonPayload(new { ok = false, error = ex.Message });
            }
        }

        /// <summary>A selected option with the context needed to phrase it in the prompt.</summary>
        private class PromptFeature
        {
            public string Category { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        /// <summary>
        /// Builds a Bedrock image prompt from the current selections. Uses the creature's
        /// PromptTemplate for a clear subject, then lists each selected option with
        /// category-aware phrasing so the model actually renders it (e.g. a Weapon becomes
        /// "wielding a great club", a Form becomes "in shapeshifter form, a fox-human
        /// hybrid"), and finishes with a strong instruction to show every feature.
        /// </summary>
        private static string BuildPrompt(CreatureType creature, string colorName, IList<PromptFeature> features)
        {
            var creatureName = (creature?.Name ?? "creature").ToLowerInvariant();

            // A chosen Form redefines the creature's body. If it carries a visual description,
            // lead the prompt with that body (otherwise the base template, e.g. "fox spirit",
            // overrides the requested form and the model just draws a fox).
            PromptFeature promotedForm = null;
            foreach (var feature in features ?? new List<PromptFeature>())
            {
                if (feature != null
                    && string.Equals(feature.Category, "Form", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(feature.Description)
                    && !IsAbsence(feature.Name))
                {
                    promotedForm = feature;
                    break;
                }
            }

            string subject;
            if (promotedForm != null)
            {
                var body = StripLeadingArticle(promotedForm.Description.Trim().TrimEnd('.')).ToLowerInvariant();
                subject = body + ", a " + creatureName;
            }
            else
            {
                subject = !string.IsNullOrWhiteSpace(creature?.PromptTemplate)
                    ? creature.PromptTemplate.Trim()
                    : creatureName;
            }

            // A "Heads" option that names a count (e.g. "Seven Heads") defines a key trait for
            // creatures like the Hydra. Lead the subject with it ("seven-headed hydra, ...") so
            // the model renders the right number of heads instead of a single-headed serpent.
            PromptFeature headsFeature = null;
            var headCountWord = (string)null;
            foreach (var feature in features ?? new List<PromptFeature>())
            {
                if (feature != null
                    && string.Equals(feature.Category, "Heads", StringComparison.OrdinalIgnoreCase)
                    && !IsAbsence(feature.Name))
                {
                    var word = CountWord(feature.Name);
                    if (word != null)
                    {
                        headsFeature = feature;
                        headCountWord = word;
                    }
                    break;
                }
            }

            if (headCountWord != null)
            {
                subject = headCountWord + "-headed " + subject;
            }

            // "a single" forces one subject so ability/word options (e.g. "Howl of the Pack")
            // don't cause the model to render multiple creatures.
            var sb = new StringBuilder();
            sb.Append("Create an image of a single ");
            if (!string.IsNullOrWhiteSpace(colorName))
            {
                sb.Append(colorName.ToLowerInvariant()).Append(' ');
            }
            sb.Append(subject).Append('.');

            var descriptors = new List<string>();
            foreach (var feature in features ?? new List<PromptFeature>())
            {
                // The promoted form / head count are already in the subject; don't repeat them.
                if (promotedForm != null && ReferenceEquals(feature, promotedForm))
                {
                    continue;
                }
                if (headsFeature != null && ReferenceEquals(feature, headsFeature))
                {
                    continue;
                }

                var descriptor = DescribeFeature(feature);
                if (!string.IsNullOrEmpty(descriptor))
                {
                    descriptors.Add(descriptor);
                }
            }

            if (descriptors.Count > 0)
            {
                sb.Append(" Features: ").Append(string.Join(", ", descriptors)).Append('.');
            }

            // Reinforce a single, isolated subject and push the model to render every feature,
            // in a photorealistic fantasy style.
            sb.Append(" A solo full-body portrait of one single creature, alone, no other creatures in the frame. ")
              .Append("The creature clearly and prominently displays every listed feature. ")
              .Append("Photorealistic fantasy creature, cinematic lighting, ultra-detailed, realistic ")
              .Append("textures, sharp focus, high resolution, centered composition, plain dark background.");

            return sb.ToString();
        }

        /// <summary>
        /// Phrases one selected option for the prompt. Skips "absence" options; uses verbs
        /// for held/worn categories (Weapon -> "wielding a ..."); expands Form options with
        /// their description for a clear body shape; otherwise qualifies the option with its
        /// category ("Grey" in "Fur" -> "grey fur") unless the option already says it.
        /// </summary>
        private static string DescribeFeature(PromptFeature feature)
        {
            if (feature == null) return null;

            var name = (feature.Name ?? string.Empty).Trim();
            if (name.Length == 0 || IsAbsence(name)) return null;

            var option = name.ToLowerInvariant();
            var category = (feature.Category ?? string.Empty).Trim().ToLowerInvariant();
            var description = (feature.Description ?? string.Empty).Trim().TrimEnd('.').ToLowerInvariant();

            // Held/carried items: make it obvious the creature is holding them.
            if (category == "weapon" || category == "weapons" || category == "craft")
            {
                return "wielding a " + option;
            }

            // Form/body shape: state it as the body and add the (visual) description if present.
            if (category == "form")
            {
                var formText = "in " + option + " form";
                if (description.Length > 0)
                {
                    formText += ", " + description;
                }
                return formText;
            }

            if (category.Length == 0 || category == "extras" || category.Contains(" "))
            {
                return option;
            }

            var categoryNoun = category.EndsWith("s") ? category.Substring(0, category.Length - 1) : category;
            if (categoryNoun.Length > 0 && option.Contains(categoryNoun))
            {
                return option;
            }

            return option + " " + category;
        }

        /// <summary>True for "no selection" options that shouldn't be drawn.</summary>
        private static bool IsAbsence(string name)
        {
            name = (name ?? string.Empty).Trim();
            return name.Equals("None", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("No ", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Bare Hands", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Standard", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Removes a leading "a " / "an " so it can follow "a single {color} ...".</summary>
        private static string StripLeadingArticle(string text)
        {
            text = (text ?? string.Empty).Trim();
            if (text.StartsWith("an ", StringComparison.OrdinalIgnoreCase)) return text.Substring(3);
            if (text.StartsWith("a ", StringComparison.OrdinalIgnoreCase)) return text.Substring(2);
            return text;
        }

        private static readonly string[] NumberWords =
        {
            "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve"
        };

        /// <summary>Returns the leading count word of an option name (e.g. "Seven Heads" -&gt; "seven"), else null.</summary>
        private static string CountWord(string optionName)
        {
            var first = (optionName ?? string.Empty).Trim().Split(' ')[0].ToLowerInvariant();
            return Array.IndexOf(NumberWords, first) >= 0 ? first : null;
        }

        /// <summary>True when the selection includes a "Heads" option (multi-headed creatures like the Hydra).</summary>
        private static bool IsMultiHeaded(IList<PromptFeature> features)
        {
            if (features == null) return false;
            foreach (var f in features)
            {
                if (f != null
                    && string.Equals(f.Category, "Heads", StringComparison.OrdinalIgnoreCase)
                    && !IsAbsence(f.Name))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Builds the negative prompt. For multi-headed creatures the usual "extra heads /
        /// extra limbs / duplicated subject / deformed" negatives are dropped (they would
        /// suppress the very thing we want), while still discouraging separate creatures.
        /// </summary>
        private static string BuildNegativePrompt(IList<PromptFeature> features)
        {
            if (IsMultiHeaded(features))
            {
                return "separate creatures, a group of creatures, herd, pack, crowd, " +
                       "text, watermark, signature, logo, frame, border, blurry, low quality";
            }
            return BedrockImageService.DefaultNegativePrompt;
        }

        /// <summary>
        /// Serializes with Newtonsoft and returns it as JSON. Unlike the default MVC
        /// Json()/JavaScriptSerializer (which caps output at ~2 MB via MaxJsonLength and
        /// throws during result execution), this handles the large base64 preview images.
        /// </summary>
        private ActionResult JsonPayload(object data)
        {
            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(data), "application/json");
        }

        private CustomizeViewModel BuildCustomizeViewModel(int creatureTypeId)
        {
            var creature = Db.CreatureTypes
                .Include(c => c.OptionCategories.Select(oc => oc.Options))
                .FirstOrDefault(c => c.Id == creatureTypeId);

            if (creature == null)
            {
                return null;
            }

            return new CustomizeViewModel
            {
                CreatureTypeId = creature.Id,
                Name = creature.Name,
                Description = creature.Description,
                Emoji = creature.Emoji,
                BasePrice = creature.BasePrice,
                Categories = creature.OptionCategories
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c =>
                    {
                        c.Options = c.Options.OrderBy(o => o.PriceModifier).ToList();
                        return c;
                    })
                    .ToList(),
                Colors = Db.Colors.OrderBy(c => c.DisplayOrder).ToList()
            };
        }
    }
}
