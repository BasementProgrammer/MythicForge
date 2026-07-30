using System.Linq;
using System.Web.Mvc;
using MythicForge.Data;
using MythicForge.Models;
using MythicForge.Services;

namespace MythicForge.Controllers
{
    /// <summary>
    /// Shared base controller: owns the EF context, exposes the current user and
    /// a cart service to derived controllers.
    /// </summary>
    public abstract class BaseController : Controller
    {
        protected readonly SampleDbContext Db = new SampleDbContext();

        protected CartService Cart
        {
            get { return new CartService(Session); }
        }

        /// <summary>Returns the signed-in user, or null when anonymous.</summary>
        protected User CurrentUser
        {
            get
            {
                if (User == null || User.Identity == null || !User.Identity.IsAuthenticated)
                {
                    return null;
                }

                var email = User.Identity.Name;
                return Db.Users.FirstOrDefault(u => u.Email == email);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
