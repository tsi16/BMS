using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Data;
using NEXT_BMS.Models;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    public class FloorsController : Controller
    {
        private readonly NEXT_BMSContext _context;
        public FloorsController(NEXT_BMSContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetFloors ()
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
                var returnData = (from manudata in _context.Floors.Where(x=>x.IsDeleted==false) select manudata);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    returnData = returnData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
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

        // GET: Administrator/Floors

       
        public IActionResult Index()
        {
            return View();
        }
      
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Floors == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var floor = await _context.Floors
                .Include(f => f.Building)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (floor == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";return RedirectToAction("Index");
            }

            return View(floor);
        }

        public IActionResult Create()
        {
            ViewData["BuildingId"] = new SelectList(_context.Buildings  .Where(x=>x.IsDeleted==false), "Id", "ConstractionYear");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,BuildingId,NumberOfRoom,IsActive,IsDeleted")] Floor floor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(floor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "floor saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving floor. Please review your input.";
            ViewData["BuildingId"] = new SelectList(_context.Buildings .Where(x=>x.IsDeleted==false), "Id", "ConstractionYear", floor.BuildingId);
            return View(floor);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Floors == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var floor = await _context.Floors.FindAsync(id);
            if (floor == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            ViewData["BuildingId"] = new SelectList(_context.Buildings  .Where(x=>x.IsDeleted==false), "Id", "ConstractionYear", floor.BuildingId);
            return View(floor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,BuildingId,NumberOfRoom,IsActive,IsDeleted")] Floor floor)
        {
            if (id != floor.Id)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(floor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FloorExists(floor.Id))
                    {
                        TempData["Error"] = "The information you're looking for was not found!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "floor saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving floor. Please review your input.";
            ViewData["BuildingId"] = new SelectList(_context.Buildings  .Where(x=>x.IsDeleted==false), "Id", "ConstractionYear", floor.BuildingId);
            return View(floor);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Floors == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var floor = await _context.Floors
                .Include(f => f.Building)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (floor == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            return View(floor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Floors == null)
            {
                return Problem("Entity set 'NEXT_BMSContext.Floors'  is null.");
            }
            var floor = await _context.Floors.FindAsync(id);
            if (floor != null)
            {
                 floor.IsActive = false;
                 floor.IsDeleted = true;
                _context.Update(floor);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FloorExists(int id)
        {
          return _context.Floors.Any(e => e.Id == id);
        }
    }
}
