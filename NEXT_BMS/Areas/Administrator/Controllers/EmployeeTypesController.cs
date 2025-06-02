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
    public class EmployeeTypesController : Controller
    {
        private readonly NEXT_BMSContext _context;
        public EmployeeTypesController(NEXT_BMSContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetEployeeTypes ()
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
                var returnData = (from manudata in _context.EmployeeTypes.Where(x=>x.IsDeleted==false) select manudata);
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

        // GET: Administrator/EmployeeTypes

       
        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
              //return View(await _context.EmployeeTypes.Where(x=>!x.IsDeleted).ToListAsync());
        //}

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.EmployeeTypes == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var employeeType = await _context.EmployeeTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employeeType == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";return RedirectToAction("Index");
            }

            return View(employeeType);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,IsActive,IsDeleted")] EmployeeType employeeType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employeeType);
                await _context.SaveChangesAsync();
                TempData["Success"] = "employeeType saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving employeeType. Please review your input.";
            return View(employeeType);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.EmployeeTypes == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var employeeType = await _context.EmployeeTypes.FindAsync(id);
            if (employeeType == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            return View(employeeType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsActive,IsDeleted")] EmployeeType employeeType)
        {
            if (id != employeeType.Id)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employeeType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeTypeExists(employeeType.Id))
                    {
                        TempData["Error"] = "The information you're looking for was not found!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "employeeType saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving employeeType. Please review your input.";
            return View(employeeType);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.EmployeeTypes == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var employeeType = await _context.EmployeeTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employeeType == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            return View(employeeType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.EmployeeTypes == null)
            {
                return Problem("Entity set 'NEXT_BMSContext.EmployeeTypes'  is null.");
            }
            var employeeType = await _context.EmployeeTypes.FindAsync(id);
            if (employeeType != null)
            {
                 employeeType.IsActive = false;
                 employeeType.IsDeleted = true;
                _context.Update(employeeType);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeTypeExists(int id)
        {
          return _context.EmployeeTypes.Any(e => e.Id == id);
        }
    }
}
