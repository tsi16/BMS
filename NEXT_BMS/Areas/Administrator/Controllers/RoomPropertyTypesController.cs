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
    public class RoomPropertyTypesController : Controller
    {
        private readonly NEXT_BMSContext _context;
        public RoomPropertyTypesController(NEXT_BMSContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetRoomPropertyTypes ()
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
                var returnData = (from manudata in _context.RoomPropertyTypes.Where(x=>x.IsDeleted==false) select manudata);
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

        // GET: Administrator/RoomPropertyTypes

       
        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
              //return View(await _context.RoomPropertyTypes.Where(x=>!x.IsDeleted).ToListAsync());
        //}

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.RoomPropertyTypes == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var roomPropertyType = await _context.RoomPropertyTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomPropertyType == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";return RedirectToAction("Index");
            }

            return View(roomPropertyType);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,IsActive,IsDeleted")] RoomPropertyType roomPropertyType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(roomPropertyType);
                await _context.SaveChangesAsync();
                TempData["Success"] = "roomPropertyType saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving roomPropertyType. Please review your input.";
            return View(roomPropertyType);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.RoomPropertyTypes == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var roomPropertyType = await _context.RoomPropertyTypes.FindAsync(id);
            if (roomPropertyType == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            return View(roomPropertyType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsActive,IsDeleted")] RoomPropertyType roomPropertyType)
        {
            if (id != roomPropertyType.Id)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(roomPropertyType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomPropertyTypeExists(roomPropertyType.Id))
                    {
                        TempData["Error"] = "The information you're looking for was not found!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "roomPropertyType saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving roomPropertyType. Please review your input.";
            return View(roomPropertyType);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.RoomPropertyTypes == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var roomPropertyType = await _context.RoomPropertyTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomPropertyType == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            return View(roomPropertyType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.RoomPropertyTypes == null)
            {
                return Problem("Entity set 'NEXT_BMSContext.RoomPropertyTypes'  is null.");
            }
            var roomPropertyType = await _context.RoomPropertyTypes.FindAsync(id);
            if (roomPropertyType != null)
            {
                 roomPropertyType.IsActive = false;
                 roomPropertyType.IsDeleted = true;
                _context.Update(roomPropertyType);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomPropertyTypeExists(int id)
        {
          return _context.RoomPropertyTypes.Any(e => e.Id == id);
        }
    }
}
