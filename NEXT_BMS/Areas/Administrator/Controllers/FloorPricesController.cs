using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NEXT_BMS.Models;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    public class FloorPricesController : Controller
    {
        private readonly NEXT_BMSContext _context;
        public FloorPricesController(NEXT_BMSContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetFloorPrices ()
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
                var returnData = (from manudata in _context.FloorPrices.Where(x=>x.IsDeleted==false) select manudata);
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

        // GET: Administrator/FloorPrices

       
        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
            //var nEXT_BMSContext = _context.FloorPrices.Include(f => f.Floor);
            //return View(await nEXT_BMSContext.ToListAsync());
        //}

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.FloorPrices == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var floorPrice = await _context.FloorPrices
                .Include(f => f.Floor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (floorPrice == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";return RedirectToAction("Index");
            }

            return View(floorPrice);
        }

        public IActionResult Create()
        {
            ViewData["FloorId"] = new SelectList(_context.Floors  .Where(x=>x.IsDeleted==false), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FloorId,Name,Price,AppliedDate,IsActive,IsDeleted")] FloorPrice floorPrice)
        {
            if (ModelState.IsValid)
            {
                _context.Add(floorPrice);
                await _context.SaveChangesAsync();
                TempData["Success"] = "floorPrice saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving floorPrice. Please review your input.";
            ViewData["FloorId"] = new SelectList(_context.Floors .Where(x=>x.IsDeleted==false), "Id", "Name", floorPrice.FloorId);
            return View(floorPrice);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.FloorPrices == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var floorPrice = await _context.FloorPrices.FindAsync(id);
            if (floorPrice == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            ViewData["FloorId"] = new SelectList(_context.Floors  .Where(x=>x.IsDeleted==false), "Id", "Name", floorPrice.FloorId);
            return View(floorPrice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FloorId,Name,Price,AppliedDate,IsActive,IsDeleted")] FloorPrice floorPrice)
        {
            if (id != floorPrice.Id)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(floorPrice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FloorPriceExists(floorPrice.Id))
                    {
                        TempData["Error"] = "The information you're looking for was not found!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "floorPrice saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving floorPrice. Please review your input.";
            ViewData["FloorId"] = new SelectList(_context.Floors  .Where(x=>x.IsDeleted==false), "Id", "Name", floorPrice.FloorId);
            return View(floorPrice);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.FloorPrices == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var floorPrice = await _context.FloorPrices
                .Include(f => f.Floor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (floorPrice == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            return View(floorPrice);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.FloorPrices == null)
            {
                return Problem("Entity set 'NEXT_BMSContext.FloorPrices'  is null.");
            }
            var floorPrice = await _context.FloorPrices.FindAsync(id);
            if (floorPrice != null)
            {
                 floorPrice.IsActive = false;
                 floorPrice.IsDeleted = true;
                _context.Update(floorPrice);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FloorPriceExists(int id)
        {
          return _context.FloorPrices.Any(e => e.Id == id);
        }
    }
}
