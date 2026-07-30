using System.Linq;
using System.Web.Mvc;

namespace MythicForge.Controllers
{
    [Authorize]
    public class OrdersController : BaseController
    {
        // GET: Orders  (order history for the signed-in user)
        public ActionResult Index()
        {
            var user = CurrentUser;
            var orders = Db.Orders
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // GET: Orders/Details/5
        public ActionResult Details(int id)
        {
            var user = CurrentUser;
            var order = Db.Orders.FirstOrDefault(o => o.Id == id && o.UserId == user.Id);
            if (order == null)
            {
                return HttpNotFound();
            }

            var unused = order.Items.Count; // ensure items are loaded
            return View(order);
        }
    }
}
