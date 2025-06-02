using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NEXT_BMS.Models;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]

    public class RoomRentalRequestsController : Controller
    {
        private readonly NEXT_BMSContext _context;

        public RoomRentalRequestsController(NEXT_BMSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()

        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var NEXT_BMSContext = _context.RoomRentalRequests.Include(r => r.RequestStatus).Include(r => r.Room).Include(r => r.User)
                .Where(x=>x.UserId==userId);
            return View(await NEXT_BMSContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomRentalRequest = await _context.RoomRentalRequests
                .Include(r => r.RequestStatus)
                .Include(r => r.Room)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomRentalRequest == null)
            {
                return NotFound();
            }

            return View(roomRentalRequest);
        }

        public IActionResult Create()
        {
            ViewData["RequestStatusId"] = new SelectList(_context.RoomRequestStatuses, "Id", "Name");
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Description");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Description,RoomId,RequestStatusId,RequestedDate,IsActive,IsDeleted")] RoomRentalRequest roomRentalRequest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(roomRentalRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RequestStatusId"] = new SelectList(_context.RoomRequestStatuses, "Id", "Name", roomRentalRequest.RequestStatusId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Description", roomRentalRequest.RoomId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", roomRentalRequest.UserId);
            return View(roomRentalRequest);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomRentalRequest = await _context.RoomRentalRequests.FindAsync(id);
            if (roomRentalRequest == null)
            {
                return NotFound();
            }
            ViewData["RequestStatusId"] = new SelectList(_context.RoomRequestStatuses, "Id", "Name", roomRentalRequest.RequestStatusId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Description", roomRentalRequest.RoomId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", roomRentalRequest.UserId);
            return View(roomRentalRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Description,RoomId,RequestStatusId,RequestedDate,IsActive,IsDeleted")] RoomRentalRequest roomRentalRequest)
        {
            if (id != roomRentalRequest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(roomRentalRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomRentalRequestExists(roomRentalRequest.Id))
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
            ViewData["RequestStatusId"] = new SelectList(_context.RoomRequestStatuses, "Id", "Name", roomRentalRequest.RequestStatusId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Description", roomRentalRequest.RoomId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", roomRentalRequest.UserId);
            return View(roomRentalRequest);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomRentalRequest = await _context.RoomRentalRequests
                .Include(r => r.RequestStatus)
                .Include(r => r.Room)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomRentalRequest == null)
            {
                return NotFound();
            }

            return View(roomRentalRequest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roomRentalRequest = await _context.RoomRentalRequests.FindAsync(id);
            if (roomRentalRequest != null)
            {
                _context.RoomRentalRequests.Remove(roomRentalRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomRentalRequestExists(int id)
        {
            return _context.RoomRentalRequests.Any(e => e.Id == id);
        }
        public async Task<IActionResult> adminL()
        {

            int? userId = HttpContext.Session.GetInt32("UserId");


            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access this page.";
                return RedirectToAction("Login", "Account");
            }


            var user = await _context.Users
                 .Include(x => x.Rooms).ThenInclude(x => x.RoomRentalRequests).ThenInclude(x=>x.RequestStatus)
                   .Include(x => x.Rooms).ThenInclude(x => x.RoomStatus)

                   .Include(x => x.UserRoles)
                .Include(x => x.UserRoles)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user != null && user.UserRoles.Any(x => x.RoleId == 1))
            {
                var roomRentalRequest = await _context.RoomRentalRequests
                                                .Include(b => b.Room)
                                                .Include(b => b.RequestStatus)
                                                .Include(b => b.User)
                                                .ToListAsync();

                return View(roomRentalRequest);
            }

            else
            {
                TempData["ErrorMessage"] = "You don't have sufficient privileges to access this page.";
                return RedirectToAction("Login", "Account");
            }
        }
        public IActionResult adminD(int? id)
        {

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access this page.";
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users
                .Include(x => x.UserRoles)
                .FirstOrDefault(x => x.Id == userId);

            if (user != null && user.UserRoles.Any(x => x.RoleId == 1))
            {
                var roomRentalRequest = _context.RoomRentalRequests
               .Include(b => b.Room)
               .Include(b => b.RequestStatus)
               .Include(b => b.User)
               .FirstOrDefault(m => m.Id == id);
                if (roomRentalRequest == null)
                {
                    return NotFound();
                }
                return View(roomRentalRequest);
            }
            else
            {
                TempData["ErrorMessage"] = "You don't have sufficient privileges to access this page.";
                return RedirectToAction("Login", "Account");
            }

        }
        [HttpPost]
        public async Task<IActionResult> UpdateRentalRequestStatus(int id, int status)
        {

            var roomRentalRequest = await _context.RoomRentalRequests.FindAsync(id);

            if (roomRentalRequest == null)
            {
                return NotFound();
            }

            roomRentalRequest.RequestStatusId = status;
            _context.Update(roomRentalRequest);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Request status updated successfully.";
            return Redirect(Request.GetTypedHeaders().Referer.ToString());
        }
   
    
    }
}
