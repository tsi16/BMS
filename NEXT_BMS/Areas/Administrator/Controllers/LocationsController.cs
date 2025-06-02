using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Data;
using NEXT_BMS.Models;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    public class LocationsController : Controller
    {
        private readonly NEXT_BMSContext _context;
        public LocationsController(NEXT_BMSContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetLocations ()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                var returnData = (from manudata in _context.Locations.Where(x=>x.IsDeleted==false) select manudata);
               
                if (!string.IsNullOrEmpty(searchValue))
                {
                    returnData = returnData.Where(m => m.Name.Contains(searchValue));
                }
                recordsTotal = returnData.Count();
                var data = returnData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
                return Ok(jsonData);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // GET: Administrator/Locations

       
        public IActionResult Index()
        {
            return View();
        }
       
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Locations == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var location = await _context.Locations
                .Include(l => l.City)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (location == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";return RedirectToAction("Index");
            }

            return View(location);
        }

        public IActionResult Create()
        {
            ViewData["CityId"] = new SelectList(_context.Cities  .Where(x=>x.IsDeleted==false), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Coordinates,CityId,IsActive,IsDeleted")] Location location)
        {
            if (ModelState.IsValid)
            {
                _context.Add(location);
                await _context.SaveChangesAsync();
                TempData["Success"] = "location saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving location. Please review your input.";
            ViewData["CityId"] = new SelectList(_context.Cities .Where(x=>x.IsDeleted==false), "Id", "Name", location.CityId);
            return View(location);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Locations == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            ViewData["CityId"] = new SelectList(_context.Cities  .Where(x=>x.IsDeleted==false), "Id", "Name", location.CityId);
            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Coordinates,CityId,IsActive,IsDeleted")] Location location)
        {
            if (id != location.Id)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(location);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationExists(location.Id))
                    {
                        TempData["Error"] = "The information you're looking for was not found!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "location saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving location. Please review your input.";
            ViewData["CityId"] = new SelectList(_context.Cities  .Where(x=>x.IsDeleted==false), "Id", "Name", location.CityId);
            return View(location);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Locations == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var location = await _context.Locations
                .Include(l => l.City)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (location == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            return View(location);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Locations == null)
            {
                return Problem("Entity set 'NEXT_BMSContext.Locations'  is null.");
            }
            var location = await _context.Locations.FindAsync(id);
            if (location != null)
            {
                 location.IsActive = false;
                 location.IsDeleted = true;
                _context.Update(location);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LocationExists(int id)
        {
          return _context.Locations.Any(e => e.Id == id);
        }
    }
}
