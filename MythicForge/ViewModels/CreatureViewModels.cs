using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MythicForge.Models;

namespace MythicForge.ViewModels
{
    /// <summary>
    /// Everything the data-driven build page needs to render a creature's
    /// options and colors.
    /// </summary>
    public class CustomizeViewModel
    {
        public int CreatureTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Emoji { get; set; }
        public decimal BasePrice { get; set; }

        public IList<OptionCategory> Categories { get; set; }
        public IList<Color> Colors { get; set; }

        /// <summary>When false, the Bedrock AI image preview is hidden from the build page.</summary>
        public bool ImageGenerationEnabled { get; set; }

        public CustomizeViewModel()
        {
            Categories = new List<OptionCategory>();
            Colors = new List<Color>();
            ImageGenerationEnabled = true;
        }
    }

    /// <summary>
    /// The values posted from the build page when adding a creature to the cart.
    /// </summary>
    public class BuildCreatureViewModel
    {
        public int CreatureTypeId { get; set; }

        [Display(Name = "Color")]
        public int ColorId { get; set; }

        [StringLength(80)]
        [Display(Name = "Name your creature")]
        public string CreatureName { get; set; }

        /// <summary>Ids of every selected option (single and multiple selections).</summary>
        public List<int> SelectedOptionIds { get; set; }

        public BuildCreatureViewModel()
        {
            SelectedOptionIds = new List<int>();
        }
    }
}
