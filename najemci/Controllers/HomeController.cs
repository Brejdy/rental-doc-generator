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
                // Získání poètu záznamù v tabulce Nemovitosti
                var count = await _context.Nemovitosti.CountAsync();

                // Zobrazení výsledku na stránce
                return Content($"Poèet záznamù v tabulce Nemovitost: {count}");
            }
            catch (Exception ex)
            {
                // Zobrazení chybové zprávy, pokud dojde k chybì pøipojení
                return Content($"Chyba pøi pøipojení k databázi: {ex.Message}");
            }
        }

            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
