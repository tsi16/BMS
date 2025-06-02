using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NEXT_BMS.Models;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    public class ShopRequestsController : Controller
    {
        private readonly NEXT_BMSContext _context;

        public ShopRequestsController(NEXT_BMSContext context)
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
            var shopRequests = await _context.ShopRequests.Include(s => s.RequestStatus).Include(s => s.User)
                .Where(br => br.UserId == userId)
                .ToListAsync(); ;
            return View(shopRequests);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shopRequest = await _context.ShopRequests
                .Include(s => s.RequestStatus)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shopRequest == null)
            {
                return NotFound();
            }

            return View(shopRequest);
        }

        public IActionResult Create()
        {
            ViewData["RequestStatusId"] = new SelectList(_context.ShopRequestStatuses, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Description,NumberOfShops,RequestStatusId,RequestDate,IsActive,IsDeleted")] ShopRequest shopRequest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(shopRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RequestStatusId"] = new SelectList(_context.ShopRequestStatuses, "Id", "Name", shopRequest.RequestStatusId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", shopRequest.UserId);
            return View(shopRequest);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shopRequest = await _context.ShopRequests.FindAsync(id);
            if (shopRequest == null)
            {
                return NotFound();
            }
            ViewData["RequestStatusId"] = new SelectList(_context.ShopRequestStatuses, "Id", "Name", shopRequest.RequestStatusId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", shopRequest.UserId);
            return View(shopRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Description,NumberOfShops,RequestStatusId,RequestDate,IsActive,IsDeleted")] ShopRequest shopRequest)
        {
            if (id != shopRequest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shopRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShopRequestExists(shopRequest.Id))
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
            ViewData["RequestStatusId"] = new SelectList(_context.ShopRequestStatuses, "Id", "Name", shopRequest.RequestStatusId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", shopRequest.UserId);
            return View(shopRequest);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shopRequest = await _context.ShopRequests
                .Include(s => s.RequestStatus)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shopRequest == null)
            {
                return NotFound();
            }

            return View(shopRequest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shopRequest = await _context.ShopRequests.FindAsync(id);
            if (shopRequest != null)
            {
                _context.ShopRequests.Remove(shopRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShopRequestExists(int id)
        {
            return _context.ShopRequests.Any(e => e.Id == id);
        }

        public async Task<IActionResult> RequestList()
        {

            int? userId = HttpContext.Session.GetInt32("UserId");


            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access this page.";
                return RedirectToAction("Login", "Account");
            }


            var user = await _context.Users
                 .Include(x => x.ShopRequests).ThenInclude(x => x.RequestStatus)
                   .Include(x => x.UserRoles)
                .Include(x => x.UserRoles)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user != null && user.UserRoles.Any(x => x.RoleId == 1))
            {
                var shopRequests = await _context.ShopRequests
                                                .Include(b => b.RequestStatus)
                                                .Include(b => b.RequestStatus)
                                                .Include(b => b.User)
                                                .ToListAsync();

                return View(shopRequests);
            }

            else
            {
                TempData["ErrorMessage"] = "You don't have sufficient privileges to access this page.";
                return RedirectToAction("Login", "Account");
            }

        }

        public IActionResult RequestDetail(int? id)
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
                var shopRequests = _context.ShopRequests
               .Include(b => b.RequestStatus)
               .Include(b => b.User)
               .FirstOrDefault(m => m.Id == id);
                if (shopRequests == null)
                {
                    return NotFound();
                }

                return View(shopRequests);
            }

            else
            {
                TempData["ErrorMessage"] = "You don't have sufficient privileges to access this page.";
                return RedirectToAction("Login", "Account");
            }

        }
        [HttpPost]
        public async Task<IActionResult> ChangeRequestStatus(int id, int status)
        {

            var shopRequests = await _context.ShopRequests.FindAsync(id);

            if (shopRequests == null)
            {
                return NotFound();
            }

            shopRequests.RequestStatusId = status;


            _context.Update(shopRequests);
            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] = "Request status updated successfully.";
            return Redirect(Request.GetTypedHeaders().Referer.ToString());
        }

    }
}
