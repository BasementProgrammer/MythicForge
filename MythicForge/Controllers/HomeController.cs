using System.Linq;
using System.Web.Mvc;

namespace MythicForge.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            // Show the creature catalog on the landing page.
            var creatures = Db.CreatureTypes
                .OrderBy(c => c.DisplayOrder)
                .ToList();

            return View(creatures);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Mythic Forge - build and buy your own mystical creature.";
            return View();
        }
    }
}
