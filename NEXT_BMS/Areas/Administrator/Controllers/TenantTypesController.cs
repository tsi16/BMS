using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXT_BMS.Models;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    public class TenantTypesController : Controller
    {
        private readonly NEXT_BMSContext _context;

        public TenantTypesController(NEXT_BMSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.TenantTypes.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tenantType = await _context.TenantTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tenantType == null)
            {
                return NotFound();
            }

            return View(tenantType);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,IsActive,IsDeleted")] TenantType tenantType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tenantType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tenantType);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tenantType = await _context.TenantTypes.FindAsync(id);
            if (tenantType == null)
            {
                return NotFound();
            }
            return View(tenantType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsActive,IsDeleted")] TenantType tenantType)
        {
            if (id != tenantType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tenantType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TenantTypeExists(tenantType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tenantType);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tenantType = await _context.TenantTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tenantType == null)
            {
                return NotFound();
            }

            return View(tenantType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tenantType = await _context.TenantTypes.FindAsync(id);
            if (tenantType != null)
            {
                _context.TenantTypes.Remove(tenantType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TenantTypeExists(int id)
        {
            return _context.TenantTypes.Any(e => e.Id == id);
        }
    }
}
