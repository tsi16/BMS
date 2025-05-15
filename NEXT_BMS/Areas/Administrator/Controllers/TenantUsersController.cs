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
    public class TenantUsersController : Controller
    {
        private readonly NEXT_BMSContext _context;
        public TenantUsersController(NEXT_BMSContext context)
        {
            _context = context;
        }
        [HttpPost]
        [HttpPost]
        public IActionResult GetTenantUsers()
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

                var query = _context.TenantUsers
                    .Include(x => x.User)
                    .Include(x => x.Tenant)
                    .Where(x => !x.IsDeleted);

                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m => m.User.FirstName.Contains(searchValue));
                }

                // Apply sorting dynamically
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    query = query.OrderBy($"{sortColumn} {sortColumnDirection}"); // requires System.Linq.Dynamic.Core
                }

                var recordsTotal = query.Count();

                var data = query
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(s => new
                    {
                        s.Id,
                        TenantName = s.Tenant.Name,
                        s.User.FirstName,
                        s.CreatedBy,
                        s.CreatedDate,
                        s.IsActive
                    })
                    .ToList();

                var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Server Error: " + ex.Message });
            }
        }

        // GET: Administrator/TenantUsers


        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
            //var nEXT_BMSContext = _context.TenantUsers.Include(t => t.CreatedByNavigation).Include(t => t.Tenant).Include(t => t.User);
            //return View(await nEXT_BMSContext.ToListAsync());
        //}

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TenantUsers == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var tenantUser = await _context.TenantUsers
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.Tenant)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tenantUser == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";return RedirectToAction("Index");
            }

            return View(tenantUser);
        }

        public IActionResult Create()
        {
            ViewData["CreatedBy"] = new SelectList(_context.Users  .Where(x=>x.IsDeleted==false), "Id", "FirstName");
            ViewData["TenantId"] = new SelectList(_context.Tenants  .Where(x=>x.IsDeleted==false), "Id", "Contact");
            ViewData["UserId"] = new SelectList(_context.Users  .Where(x=>x.IsDeleted==false), "Id", "FirstName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TenantId,UserId,CreatedDate,CreatedBy,IsActive,IsDeleted")] TenantUser tenantUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tenantUser);
                await _context.SaveChangesAsync();
                TempData["Success"] = "tenantUser saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving tenantUser. Please review your input.";
            ViewData["CreatedBy"] = new SelectList(_context.Users .Where(x=>x.IsDeleted==false), "Id", "FirstName", tenantUser.CreatedBy);
            ViewData["TenantId"] = new SelectList(_context.Tenants .Where(x=>x.IsDeleted==false), "Id", "Contact", tenantUser.TenantId);
            ViewData["UserId"] = new SelectList(_context.Users .Where(x=>x.IsDeleted==false), "Id", "FirstName", tenantUser.UserId);
            return View(tenantUser);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TenantUsers == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var tenantUser = await _context.TenantUsers.FindAsync(id);
            if (tenantUser == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            ViewData["CreatedBy"] = new SelectList(_context.Users  .Where(x=>x.IsDeleted==false), "Id", "FirstName", tenantUser.CreatedBy);
            ViewData["TenantId"] = new SelectList(_context.Tenants  .Where(x=>x.IsDeleted==false), "Id", "Contact", tenantUser.TenantId);
            ViewData["UserId"] = new SelectList(_context.Users  .Where(x=>x.IsDeleted==false), "Id", "FirstName", tenantUser.UserId);
            return View(tenantUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenantId,UserId,CreatedDate,CreatedBy,IsActive,IsDeleted")] TenantUser tenantUser)
        {
            if (id != tenantUser.Id)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tenantUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TenantUserExists(tenantUser.Id))
                    {
                        TempData["Error"] = "The information you're looking for was not found!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "tenantUser saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving tenantUser. Please review your input.";
            ViewData["CreatedBy"] = new SelectList(_context.Users  .Where(x=>x.IsDeleted==false), "Id", "FirstName", tenantUser.CreatedBy);
            ViewData["TenantId"] = new SelectList(_context.Tenants  .Where(x=>x.IsDeleted==false), "Id", "Contact", tenantUser.TenantId);
            ViewData["UserId"] = new SelectList(_context.Users  .Where(x=>x.IsDeleted==false), "Id", "FirstName", tenantUser.UserId);
            return View(tenantUser);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TenantUsers == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var tenantUser = await _context.TenantUsers
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.Tenant)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tenantUser == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            return View(tenantUser);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TenantUsers == null)
            {
                return Problem("Entity set 'NEXT_BMSContext.TenantUsers'  is null.");
            }
            var tenantUser = await _context.TenantUsers.FindAsync(id);
            if (tenantUser != null)
            {
                 tenantUser.IsActive = false;
                 tenantUser.IsDeleted = true;
                _context.Update(tenantUser);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TenantUserExists(int id)
        {
          return _context.TenantUsers.Any(e => e.Id == id);
        }
    }
}
