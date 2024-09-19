using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using najemci.Data;
using najemci.Models;
using System.Diagnostics;

namespace najemci.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Z�sk�n� po�tu z�znam� v tabulce Nemovitosti
                var count = await _context.Nemovitosti.CountAsync();

                // Zobrazen� v�sledku na str�nce
                return Content($"Po�et z�znam� v tabulce Nemovitost: {count}");
            }
            catch (Exception ex)
            {
                // Zobrazen� chybov� zpr�vy, pokud dojde k chyb� p�ipojen�
                return Content($"Chyba p�i p�ipojen� k datab�zi: {ex.Message}");
            }
        }

            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
