using System.Collections.Generic;
using MythicForge.Models;

namespace MythicForge.Services
{
    /// <summary>
    /// Central place for computing a creature's price. Always calculated on the
    /// server from catalog data so a client cannot tamper with prices.
    /// </summary>
    public static class PricingService
    {
        public static decimal CalculatePrice(CreatureType creatureType, Color color, IEnumerable<CreatureOption> options)
        {
            decimal price = creatureType != null ? creatureType.BasePrice : 0m;

            if (color != null)
            {
                price += color.PriceModifier;
            }

            if (options != null)
            {
                foreach (var option in options)
                {
                    price += option.PriceModifier;
                }
            }

            return price;
        }
    }
}
