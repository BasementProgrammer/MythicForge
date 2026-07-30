using System;
using System.Collections.Generic;

namespace MythicForge.Models
{
    /// <summary>
    /// One configured creature held in the session-based shopping cart. This is a
    /// lightweight, self-contained snapshot (not an EF entity) so anonymous users
    /// can build a cart before logging in at checkout.
    /// </summary>
    [Serializable]
    public class CartLine
    {
        public CartLine()
        {
            LineId = Guid.NewGuid().ToString("N");
            OptionIds = new List<int>();
            Quantity = 1;
        }

        /// <summary>Unique id for this cart line, used to update/remove it.</summary>
        public string LineId { get; set; }

        public int CreatureTypeId { get; set; }
        public string CreatureTypeName { get; set; }
        public string Emoji { get; set; }

        public int ColorId { get; set; }
        public string ColorName { get; set; }
        public string ColorHex { get; set; }

        /// <summary>Customer-provided name for their creature (optional).</summary>
        public string CreatureName { get; set; }

        public List<int> OptionIds { get; set; }
        public string OptionsSummary { get; set; }

        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal LineTotal
        {
            get { return UnitPrice * Quantity; }
        }
    }
}
