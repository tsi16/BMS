using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXT_BMS.Models;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    public class NotificationStatusController : Controller
    {
        private readonly NEXT_BMSContext _context;

        public NotificationStatusController(NEXT_BMSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.NotificationStatuses.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificationStatus = await _context.NotificationStatuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notificationStatus == null)
            {
                return NotFound();
            }

            return View(notificationStatus);
        }

        public IActionResult Create()
        {
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,IsActive,IsDeleted")] NotificationStatus notificationStatus)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notificationStatus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(notificationStatus);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificationStatus = await _context.NotificationStatuses.FindAsync(id);
            if (notificationStatus == null)
            {
                return NotFound();
            }
            return View(notificationStatus);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsActive,IsDeleted")] NotificationStatus notificationStatus)
        {
            if (id != notificationStatus.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notificationStatus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationStatusExists(notificationStatus.Id))
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
            return View(notificationStatus);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificationStatus = await _context.NotificationStatuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notificationStatus == null)
            {
                return NotFound();
            }

            return View(notificationStatus);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notificationStatus = await _context.NotificationStatuses.FindAsync(id);
            if (notificationStatus != null)
            {
                _context.NotificationStatuses.Remove(notificationStatus);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotificationStatusExists(int id)
        {
            return _context.NotificationStatuses.Any(e => e.Id == id);
        }
    }
}
