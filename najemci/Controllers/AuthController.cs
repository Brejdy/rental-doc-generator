using Microsoft.AspNetCore.Mvc;

namespace najemci.Controllers
{
    public class AuthController : Controller
    {
        private const string Password = "Pelemija241.";

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string password)
        {
            if (password == Password)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = "Nezná Heslo! Vetřelec!";
            return View();
        }
    }
}
