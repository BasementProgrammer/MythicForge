using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MythicForge.Models
{
    /// <summary>
    /// A placed order. Line items snapshot the creature configuration as text so
    /// the order remains accurate even if catalog data changes.
    /// </summary>
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public DateTime OrderDate { get; set; }

        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; }

        [Required]
        [StringLength(100)]
        public string ShippingName { get; set; }

        [Required]
        [StringLength(250)]
        public string ShippingAddress { get; set; }

        [Required]
        [StringLength(100)]
        public string ShippingCity { get; set; }

        [Required]
        [StringLength(20)]
        public string ShippingPostalCode { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; }
    }

    /// <summary>
    /// A single built creature within an order.
    /// </summary>
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        [StringLength(80)]
        public string CreatureTypeName { get; set; }

        [StringLength(80)]
        public string CreatureName { get; set; }

        [StringLength(50)]
        public string ColorName { get; set; }

        /// <summary>Comma-separated snapshot of the chosen options.</summary>
        [StringLength(500)]
        public string OptionsSummary { get; set; }

        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal LineTotal
        {
            get { return UnitPrice * Quantity; }
        }
    }
}
