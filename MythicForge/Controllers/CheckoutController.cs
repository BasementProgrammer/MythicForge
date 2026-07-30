using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MythicForge.Models;
using MythicForge.ViewModels;

namespace MythicForge.Controllers
{
    [Authorize]
    public class CheckoutController : BaseController
    {
        // GET: Checkout
        public ActionResult Index()
        {
            var cart = Cart.GetCart();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var user = CurrentUser;
            var model = new CheckoutViewModel
            {
                ShippingName = user != null ? user.DisplayName : string.Empty,
                Items = cart,
                Total = Cart.Total()
            };

            return View(model);
        }

        // POST: Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(CheckoutViewModel model)
        {
            var cart = Cart.GetCart();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                model.Items = cart;
                model.Total = Cart.Total();
                return View(model);
            }

            var user = CurrentUser;
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                ShippingName = model.ShippingName,
                ShippingAddress = model.ShippingAddress,
                ShippingCity = model.ShippingCity,
                ShippingPostalCode = model.ShippingPostalCode,
                TotalPrice = cart.Sum(l => l.LineTotal),
                Items = cart.Select(l => new OrderItem
                {
                    CreatureTypeName = l.CreatureTypeName,
                    CreatureName = l.CreatureName,
                    ColorName = l.ColorName,
                    OptionsSummary = l.OptionsSummary,
                    UnitPrice = l.UnitPrice,
                    Quantity = l.Quantity
                }).ToList()
            };

            Db.Orders.Add(order);
            Db.SaveChanges();

            Cart.Clear();
            return RedirectToAction("Confirmation", new { id = order.Id });
        }

        // GET: Checkout/Confirmation/5
        public ActionResult Confirmation(int id)
        {
            var user = CurrentUser;
            var order = Db.Orders.FirstOrDefault(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
            {
                return HttpNotFound();
            }

            // Touch the items collection so it is loaded for the view.
            var unused = order.Items.Count;
            return View(order);
        }
    }
}
