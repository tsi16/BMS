using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NEXT_BMS.Models;
using Microsoft.EntityFrameworkCore;

namespace NEXT_BMS.Controllers
{
   public class ShopItemsController : Controller
    {
        private readonly NEXT_BMSContext _context;

        public ShopItemsController(NEXT_BMSContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int id)
        {
            var shopItem = _context.ShopItems
                .Include(x => x.Item).ThenInclude(x => x.ItemImages)
                .Include(x => x.ItemEntries).ThenInclude(x => x.ItemEntryVarations).ThenInclude(x => x.VarationType).ThenInclude(x => x.Varations)
                 .Include(x => x.ItemEntryVarations)
                .FirstOrDefault(x => x.Id == id);


            ViewData["VarationTypeId"] = new SelectList(_context.VarationTypes, "Id", "Name");
            ViewData["VarationId"] = new SelectList(_context.Varations, "Id", "Name");
            return View(shopItem);
        }

    }
}
