using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using najemci.Data;
using najemci.Migrations;
using najemci.Models;
using System;
using System.Globalization;

namespace najemci.Controllers
{
    public class NemovitostController : Controller
    {
        private readonly ApplicationDbContext _context;
        public NemovitostController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var nemovitosti = await _context.Nemovitosti.ToListAsync();
            return View(nemovitosti);
        }

        public async Task<IActionResult> Details(int id)
        {
            var nemovitosti = await _context.Nemovitosti.Include(n=>n.Byty.OrderBy(b=>b.Cislo)).ThenInclude(n=>n.Najemnici).FirstOrDefaultAsync(n=>n.Id == id);

            if (nemovitosti == null) 
            { 
                return NotFound(); 
            }

            return View(nemovitosti);
        }

        public IActionResult Nova()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Nova(Nemovitost nemovitost)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nemovitost);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(nemovitost);
            }

            return View(nemovitost);
        }

        public IActionResult NovyByt(int id)
        {
            var nemovitost = _context.Nemovitosti.FirstOrDefault(n => n.Id == id);

            if (nemovitost == null)
            {
                return NotFound();
            }

            var model = new Byt
            {
                NemovitostId = nemovitost.Id
            };

            ViewBag.NemovitostJmeno = nemovitost.Jmeno;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NovyByt([Bind("NemovitostId, Cislo, Najem, Kauce, Sluzby, NajemSluzby, Rozloha, Patro, Mistnosti")] Byt byt)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (byt.NemovitostId == 0)
                    {
                        ModelState.AddModelError("", "Nemovitost musí být vybrána.");
                        return View(byt);
                    }

                    byt.SetDefault();

                    _context.Byty.Add(byt);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = byt.NemovitostId });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                    return View(byt);
                }
                return View(byt);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, "Chyba při ukládání do databáze: " + ex.Message);
            }
        }

        [HttpPost]
        public IActionResult OdstranitByt(int id)
        {
            var byt = _context.Byty.Find(id);
            if (byt == null)
            {
                return NotFound();
            }

            _context.Byty.Remove(byt);
            _context.SaveChanges();

            return RedirectToAction("Detail", new {id = byt.NemovitostId});
        }

        public async Task<IActionResult> DetailBytu(int id)
        {
            var byty = await _context.Byty.Include(n => n.Nemovitost).Include(n => n.Najemnici).FirstOrDefaultAsync(n => n.Id == id);

            if (byty == null)
            {
                return NotFound();
            }

            return View(byty);
        }

        [HttpGet]
        public async Task<IActionResult> UpravByt(int id)
        {
            var byt = _context.Byty.Include(n => n.Nemovitost).FirstOrDefault(n => n.Id == id);

            if (byt == null)
            {
                return NotFound();
            }

            return View(byt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpravByt(int id, [Bind("Id,NemovitostId,Najem,Kauce,Sluzby,Patro,Rozloha,Mistnosti,Cislo,NajemSluzby")] Byt byt)
        {
            if (id != byt.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Byty.Update(byt);
                await _context.SaveChangesAsync();
                return RedirectToAction("DetailBytu", new { id = byt.Id });
            }
            return View(byt);
        }
        
        public IActionResult NovyNajemnik(int id)
        {
            var byt = _context.Byty.FirstOrDefault(n => n.Id == id);

            var model = new Najemnik
            {
                BytId = byt.Id
            };

            ViewBag.BytCislo = byt.Cislo;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NovyNajemnik(int id, [Bind("BytId,Jmeno,Email,Telefon,DatumNarozeni,NajemOd,RodneCislo,CisloUctu,RoleNajemnika,CisloOP")] Najemnik najemnik)
        {
            //try
            //{
                if (ModelState.IsValid)
                {
                    if (najemnik.BytId == 0)
                    {
                        ModelState.AddModelError("", "Byt musí být vybrán.");
                        return View(najemnik);
                    }

                    _context.Add(najemnik);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("DetailBytu", new { id = najemnik.BytId });
                }
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                    return View(najemnik);
                }
                return View(najemnik);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    Console.WriteLine(ex.StackTrace);
            //    return StatusCode(500, "Chyba při ukládání do databáze: " + ex.Message);
            //}
        }
        [HttpGet]
        public async Task<IActionResult> UpravNajemnika(int id)
        {
            var najemnik = _context.Najemnici.Include(n => n.Byt).FirstOrDefault(n => n.Id == id);

            if (najemnik == null)
            {
                return NotFound();
            }

            return View(najemnik);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpravNajemnika(int id, [Bind("Id, BytId,Jmeno,Email,Telefon,DatumNarozeni,RodneCislo,CisloUctu,NajemOd,RoleNajemnika,CisloOP")] Najemnik updatedNajemnik)
        {
            if (id != updatedNajemnik.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Najemnici.Update(updatedNajemnik);
                await _context.SaveChangesAsync();

                return RedirectToAction("DetailBytu", new { id = updatedNajemnik.BytId });
            }
            return View(updatedNajemnik);
        }
        [HttpPost]
        public IActionResult OdstranitNajemnika(int id)
        {
            var najemnik = _context.Najemnici.Find(id);
            if (najemnik == null)
            {
                return NotFound();
            }

            _context.Najemnici.Remove(najemnik);
            _context.SaveChanges();

            return RedirectToAction("DetailBytu", new { id = najemnik.BytId });
        }
    }
}