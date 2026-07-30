using System.Web.Mvc;

namespace MythicForge.Controllers
{
    public class CartController : BaseController
    {
        // GET: Cart
        public ActionResult Index()
        {
            return View(Cart.GetCart());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(string lineId, int quantity)
        {
            Cart.UpdateQuantity(lineId, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(string lineId)
        {
            Cart.RemoveLine(lineId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Clear()
        {
            Cart.Clear();
            return RedirectToAction("Index");
        }

        /// <summary>Small cart badge rendered in the navigation bar.</summary>
        [ChildActionOnly]
        public ActionResult CartSummary()
        {
            ViewBag.ItemCount = Cart.ItemCount();
            return PartialView("_CartSummary");
        }
    }
}
