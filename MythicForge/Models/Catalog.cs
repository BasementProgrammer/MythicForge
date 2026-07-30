using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MythicForge.Models
{
    /// <summary>
    /// How the options within a category may be selected on the build page.
    /// </summary>
    public enum SelectionType
    {
        /// <summary>Choose at most one option (rendered as radio buttons).</summary>
        Single = 0,

        /// <summary>Choose any number of options (rendered as checkboxes).</summary>
        Multiple = 1
    }

    /// <summary>
    /// A kind of creature the customer can build (Dragon, Unicorn, Giant, Pixie...).
    /// The whole build experience is driven from this data.
    /// </summary>
    public class CreatureType
    {
        public int Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Display(Name = "Base Price")]
        [DataType(DataType.Currency)]
        public decimal BasePrice { get; set; }

        /// <summary>A simple emoji used as lightweight artwork for the sample.</summary>
        [StringLength(20)]
        public string Emoji { get; set; }

        public int DisplayOrder { get; set; }

        /// <summary>
        /// Comma-separated category tags used for filtering the catalog
        /// (e.g. "Land, Air, Scales"). Habitat (Land/Sea/Air) plus traits
        /// (Furry/Scales/Feathered/Fire/Spirit).
        /// </summary>
        [StringLength(200)]
        public string Tags { get; set; }

        /// <summary>
        /// A descriptive noun phrase used as the subject of the Bedrock image prompt,
        /// e.g. "werewolf, a hulking bipedal wolf-human hybrid covered in shaggy fur".
        /// Gives the image model clear context about the creature.
        /// </summary>
        [StringLength(500)]
        public string PromptTemplate { get; set; }

        public virtual ICollection<OptionCategory> OptionCategories { get; set; }
    }

    /// <summary>
    /// A group of options belonging to a creature type, e.g. "Wings" or "Horns".
    /// </summary>
    public class OptionCategory
    {
        public int Id { get; set; }

        public int CreatureTypeId { get; set; }
        public virtual CreatureType CreatureType { get; set; }

        [Required]
        [StringLength(80)]
        public string Name { get; set; }

        public SelectionType SelectionType { get; set; }

        /// <summary>When true and SelectionType is Single, the customer must pick an option.</summary>
        public bool IsRequired { get; set; }

        public int DisplayOrder { get; set; }

        public virtual ICollection<CreatureOption> Options { get; set; }
    }

    /// <summary>
    /// A single selectable option (e.g. "Feathered Wings") with a price modifier.
    /// </summary>
    public class CreatureOption
    {
        public int Id { get; set; }

        public int OptionCategoryId { get; set; }
        public virtual OptionCategory OptionCategory { get; set; }

        [Required]
        [StringLength(80)]
        public string Name { get; set; }

        [StringLength(250)]
        public string Description { get; set; }

        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal PriceModifier { get; set; }
    }

    /// <summary>
    /// A color the creature can be painted. Shared across all creature types.
    /// </summary>
    public class Color
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(7)]
        public string HexValue { get; set; }

        [DataType(DataType.Currency)]
        public decimal PriceModifier { get; set; }

        public int DisplayOrder { get; set; }
    }
}
