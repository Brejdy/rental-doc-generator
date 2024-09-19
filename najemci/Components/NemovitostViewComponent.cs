using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using najemci.Data;
using System.Threading.Tasks;
using najemci.Models;

public class NemovitostViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public NemovitostViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            var nemovitost = await _context.Nemovitosti.ToListAsync();

            if (nemovitost == null || !nemovitost.Any())
            {
                return View(new List<Nemovitost>());
            }
            return View(nemovitost);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}

