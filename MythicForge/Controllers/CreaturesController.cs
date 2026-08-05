using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MythicForge.Models;
using MythicForge.Services;
using MythicForge.ViewModels;

namespace MythicForge.Controllers
{
    public class CreaturesController : BaseController
    {
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
