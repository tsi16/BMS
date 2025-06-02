using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Data;
using NEXT_BMS.Models;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    public class ItemsController : Controller
    {
        private readonly NEXT_BMSContext _context;
        public ItemsController(NEXT_BMSContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetItems ()
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
                var returnData = (from manudata in _context.Items.Where(x=>x.IsDeleted==false) select manudata);
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

        public IActionResult Index()
        {
            return View();
        }
      
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Items == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var item = await _context.Items
                .Include(i => i.ItemCategory)
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";return RedirectToAction("Index");
            }

            return View(item);
        }

        public IActionResult Create()
        {
            ViewData["ItemCategoryId"] = new SelectList(_context.ItemCategories  .Where(x=>x.IsDeleted==false), "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users  .Where(x=>x.IsDeleted==false), "Id", "Email");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,UserId,Description,ItemCategoryId,IsAvailable,IsActive,IsDeleted")] Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "item saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving item. Please review your input.";
            ViewData["ItemCategoryId"] = new SelectList(_context.ItemCategories .Where(x=>x.IsDeleted==false), "Id", "Name", item.ItemCategoryId);
            ViewData["UserId"] = new SelectList(_context.Users .Where(x=>x.IsDeleted==false), "Id", "Email", item.UserId);
            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Items == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            ViewData["ItemCategoryId"] = new SelectList(_context.ItemCategories  .Where(x=>x.IsDeleted==false), "Id", "Name", item.ItemCategoryId);
            ViewData["UserId"] = new SelectList(_context.Users  .Where(x=>x.IsDeleted==false), "Id", "Email", item.UserId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,UserId,Description,ItemCategoryId,IsAvailable,IsActive,IsDeleted")] Item item)
        {
            if (id != item.Id)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.Id))
                    {
                        TempData["Error"] = "The information you're looking for was not found!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "item saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving item. Please review your input.";
            ViewData["ItemCategoryId"] = new SelectList(_context.ItemCategories  .Where(x=>x.IsDeleted==false), "Id", "Name", item.ItemCategoryId);
            ViewData["UserId"] = new SelectList(_context.Users  .Where(x=>x.IsDeleted==false), "Id", "Email", item.UserId);
            return View(item);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Items == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var item = await _context.Items
                .Include(i => i.ItemCategory)
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Items == null)
            {
                return Problem("Entity set 'NEXT_BMSContext.Items'  is null.");
            }
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                 item.IsActive = false;
                 item.IsDeleted = true;
                _context.Update(item);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemExists(int id)
        {
          return _context.Items.Any(e => e.Id == id);
        }
    }
}
