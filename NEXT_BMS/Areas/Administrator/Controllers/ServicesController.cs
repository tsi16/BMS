using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NEXT_BMS.Models;
namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    public class ServicesController : Controller
    {
        private readonly NEXT_BMSContext _context;

        public ServicesController(NEXT_BMSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> BuildingRequests()
        {

            int? userId = HttpContext.Session.GetInt32("UserId");


            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access this page.";
                return RedirectToAction("Login", "Account");
            }


            var user = await _context.Users
                 .Include(x => x.BuildingRequests).ThenInclude(x => x.RequestStatus)
                   .Include(x => x.BuildingRequests).ThenInclude(x => x.BuildingType)

                   .Include(x => x.UserRoles)
                .Include(x => x.UserRoles)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user != null && user.UserRoles.Any(x => x.RoleId == 1))
            {
                var buildingRequests = await _context.BuildingRequests

                                                .Include(b => b.BuildingType)
                                                .Include(b => b.Location)
                                                .Include(b => b.RequestStatus)
                                                .Include(b => b.User)
                                                .ToListAsync();

                return View(buildingRequests);
            }

            else
            {
                TempData["ErrorMessage"] = "You don't have sufficient privileges to access this page.";
                return RedirectToAction("Login", "Account");
            }

        }
        public IActionResult BuildingRequest(int? id)
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
                var buildingRequest = _context.BuildingRequests
               .Include(b => b.BuildingType)
               .Include(b => b.Location)
               .Include(b => b.RequestStatus)
               .Include(b => b.User)
               .FirstOrDefault(m => m.Id == id);
                if (buildingRequest == null)
                {
                    return NotFound();
                }

                return View(buildingRequest);
            }

            else
            {
                TempData["ErrorMessage"] = "You don't have sufficient privileges to access this page.";
                return RedirectToAction("Login", "Account");
            }

        }

        public IActionResult MaintenanceRequests(int? id)
        {
            try
            {

                var maintenanceRequests = _context.MaintenanceRequests
                    .Where(x => !x.IsDeleted && x.IsActive)
                    .Include(x => x.User)
                    .Include(x => x.Room)
                    .Include(x => x.MaintenanceStatus)
                    .Include(x => x.MaintenanceType)
                    .ToList();

                return View(maintenanceRequests);
            }
            catch (Exception ex)
            {

                return View(new List<MaintenanceRequest>());
            }
        }

        public async Task<IActionResult> MaintenanceRequest(int? id)
        {
            if (id == null || _context.MaintenanceRequests == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var maintenanceRequest = await _context.MaintenanceRequests
                .Include(m => m.MaintenanceStatus)
                .Include(m => m.MaintenanceRequestAllocations)
                    .ThenInclude(m => m.AllocationStatus)
                .Include(m => m.MaintenanceRequestAllocations)
                    .ThenInclude(m => m.BuildingEmployee)
                .Include(m => m.MaintenanceRequestAllocations)
                    .ThenInclude(m => m.MaintenanceStatusReports)
                .Include(m => m.MaintenanceType)
                .Include(m => m.Room)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive == true);
            if (maintenanceRequest == null)
            {
                TempData["Error"] = "The information you're looking for was not found!"; return RedirectToAction("Index");
            }
            var buildingEmployees = _context.BuildingEmployees.Select(s => new
            {
                s.Id,
                Name = s.FullName
            });
            ViewBag.BuildingEmployees = new SelectList(buildingEmployees, "Id", "Name");
            return View(maintenanceRequest);
        }

        private bool RoomRentalRequestExists(int id)
        {
            return _context.RoomRentalRequests.Any(e => e.Id == id);
        }
        public async Task<IActionResult> RentalRequests()
        {

            int? userId = HttpContext.Session.GetInt32("UserId");


            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access this page.";
                return RedirectToAction("Login", "Account");
            }


            var user = await _context.Users
                 .Include(x => x.Rooms).ThenInclude(x => x.RoomRentalRequests).ThenInclude(x => x.RequestStatus)
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
        public IActionResult RentalRequest(int? id)
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

        public async Task<IActionResult> ShopRequests()
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
        public IActionResult ShopRequest(int? id)
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

       
        [HttpPost]
        public IActionResult ChangeStatus(int reportId, int statusId)
        {
            var report = _context.MaintenanceStatusReports.Find(reportId);
            if (report == null)
            {
                return NotFound();
            }

            var allocation = _context.MaintenanceRequestAllocations
                .FirstOrDefault(a => a.Id == report.MaintenanceRequestAllocationId);

            if (allocation == null)
            {
                return NotFound("Allocation not found.");
            }

            allocation.AllocationStatusId = statusId;
            _context.SaveChanges();

            TempData["Success"] = "Status updated.";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Ok();

            return Redirect(Request.Headers["Referer"].ToString()); 
        }

    }
}
