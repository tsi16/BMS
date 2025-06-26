using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using NEXT_BMS.Models;
using NEXT_BMS.ViewModels;

namespace NEXT_BMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly NEXT_BMSContext _context;
       
        public HomeController(NEXT_BMSContext context)
        {
            _context = context;
          
        }
        public IActionResult Index()
        {
            var popularBuildings = _context.Buildings
                .Include(b => b.Location).ThenInclude(l => l.City)
                .Include(b => b.Floors).ThenInclude(f => f.Rooms)
                .Include(b => b.BuildingImages)
                .OrderByDescending(b => b.Floors.SelectMany(f => f.Rooms).Count()) 
                .Take(3)
                .ToList();

            var popularShops = _context.Shops
                .Include(s => s.ShopLocations)
                .Include(s => s.ShopImages)
                .Include(s => s.BusinessArea)
                .OrderByDescending(s => s.CreatedDate) 
                .Take(3)
                .ToList();

            ViewBag.PopularBuildings = popularBuildings;
            ViewBag.PopularShops = popularShops;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Privacy()
        {
            ViewBag.Message = "Your Privacy.";

            return View();
        }

    }
}