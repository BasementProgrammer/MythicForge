using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MythicForge.Models;

namespace MythicForge.ViewModels
{
    public class CheckoutViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full name")]
        public string ShippingName { get; set; }

        [Required]
        [StringLength(250)]
        [Display(Name = "Address")]
        public string ShippingAddress { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "City")]
        public string ShippingCity { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Postal code")]
        public string ShippingPostalCode { get; set; }

        public IList<CartLine> Items { get; set; }
        public decimal Total { get; set; }

        public CheckoutViewModel()
        {
            Items = new List<CartLine>();
        }
    }
}
