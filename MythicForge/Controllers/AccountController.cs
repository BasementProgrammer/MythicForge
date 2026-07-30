using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MythicForge.Models;
using MythicForge.Services;
using MythicForge.ViewModels;

namespace MythicForge.Controllers
{
    public class AccountController : BaseController
    {
        // GET: Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var user = Db.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null || !PasswordHasher.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            FormsAuthentication.SetAuthCookie(user.Email, model.RememberMe);
            return RedirectToLocal(returnUrl);
        }

        // GET: Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (Db.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "An account with that email already exists.");
                return View(model);
            }

            var user = new User
            {
                Email = model.Email,
                DisplayName = model.DisplayName,
                PasswordHash = PasswordHasher.Hash(model.Password),
                CreatedOn = DateTime.UtcNow
            };

            Db.Users.Add(user);
            Db.SaveChanges();

            FormsAuthentication.SetAuthCookie(user.Email, false);
            return RedirectToAction("Index", "Home");
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
