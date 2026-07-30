using System.Collections.Generic;
using System.Linq;
using System.Web;
using MythicForge.Models;

namespace MythicForge.Services
{
    /// <summary>
    /// Wraps the session-backed shopping cart. Using the session lets shoppers
    /// build a cart while anonymous; login is only required at checkout.
    /// </summary>
    public class CartService
    {
        private const string CartKey = "MysticalCart";
        private readonly HttpSessionStateBase _session;

        public CartService(HttpSessionStateBase session)
        {
            _session = session;
        }

        public List<CartLine> GetCart()
        {
            var cart = _session[CartKey] as List<CartLine>;
            if (cart == null)
            {
                cart = new List<CartLine>();
                _session[CartKey] = cart;
            }

            return cart;
        }

        public void AddLine(CartLine line)
        {
            var cart = GetCart();
            cart.Add(line);
            Save(cart);
        }

        public void UpdateQuantity(string lineId, int quantity)
        {
            var cart = GetCart();
            var line = cart.FirstOrDefault(l => l.LineId == lineId);
            if (line == null)
            {
                return;
            }

            if (quantity <= 0)
            {
                cart.Remove(line);
            }
            else
            {
                line.Quantity = quantity;
            }

            Save(cart);
        }

        public void RemoveLine(string lineId)
        {
            var cart = GetCart();
            var line = cart.FirstOrDefault(l => l.LineId == lineId);
            if (line != null)
            {
                cart.Remove(line);
                Save(cart);
            }
        }

        public void Clear()
        {
            _session[CartKey] = new List<CartLine>();
        }

        public int ItemCount()
        {
            return GetCart().Sum(l => l.Quantity);
        }

        public decimal Total()
        {
            return GetCart().Sum(l => l.LineTotal);
        }

        private void Save(List<CartLine> cart)
        {
            _session[CartKey] = cart;
        }
    }
}
