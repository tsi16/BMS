using NEXT_BMS.Models;
using NEXT_BMS.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    public class TenantsController : Controller
    {
        private readonly NEXT_BMSContext _context;

        public TenantsController(NEXT_BMSContext context)
        {
            _context = context;
        }

       
        public IActionResult Index(int? buildingId)
        {

            int? userId = HttpContext.Session.GetInt32("UserId");



            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }


            List<Tenant> tenants = _context.Tenants.Where(x => x.BuildingId == buildingId && x.IsDeleted == false).ToList();



            var buildings = _context.Buildings
                .Include(x => x.Owner).ThenInclude(x => x.OwnerUsers)
                .Where(x => x.Owner.OwnerUsers.Any(e => e.UserId == userId));

            ViewData["BuildingId"] = new SelectList(buildings, "Id", "Name");
            ViewData["TenantTypeId"] = new SelectList(_context.TenantTypes.ToList(), "Id", "Name");


            return View(tenants);
        }


        private bool UserIsBuildingOwner(int userId)
        {
           
            var roleId = _context.UserRoles
                .Where(u => u.UserId == userId) 
                .Select(u => u.RoleId)
                .FirstOrDefault();

            return roleId == 3; 

        }


        public IActionResult Details(int? id)
        {

            int? userId = HttpContext.Session.GetInt32("UserId");


            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }


            var tenant = _context.Tenants
                .Include(x => x.TenantUsers)
                .Include(x => x.RoomRentals).ThenInclude(x => x.RentalAgreementTerminations).ThenInclude(x => x.RentalTerminationApprovals).ThenInclude(x => x.TerminationRequestStatus)
                   .Include(x => x.RoomRentals).ThenInclude(rr => rr.BusinessArea)
                .Include(x => x.RoomRentals)
                    .ThenInclude(rr => rr.Room)
                .Include(x => x.Building)
                .Include(x => x.TenantType)
                .FirstOrDefault(m => m.Id == id);


            if (tenant == null)
            {
                return NotFound();
            }

            var RentalAgreementTermination = _context.RentalAgreementTerminations.Where(x => x.RoomRentalId == userId && x.IsActive).ToList();

            var approvedRoomRentalRequestIds = _context.RoomRentalRequests
                .Where(rr => rr.UserId == userId && rr.RequestStatusId == 2)
                .Select(rr => rr.RoomId)
                .ToList();


            var rooms = _context.Rooms
                .Include(x => x.Floor)
                    .ThenInclude(f => f.Building)
                        .ThenInclude(b => b.City)
                .Where(x => x.IsActive && approvedRoomRentalRequestIds.Contains(x.Id))
                .ToList();


            ViewData["RoomId"] = new SelectList(rooms, "Id", "Name");
            ViewData["BusinessAreaId"] = new SelectList(_context.BusinessAreas.ToList(), "Id", "Name");


            return View(tenant);
        }

        [HttpPost]
        public IActionResult AddTenant(int BuildingId, int TenantTypeId, string Name, int Tin, string Description, string Contact)
        {
            if (BuildingId <= 0)
            {
                return BadRequest("Invalid building ID.");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

           
            bool hasAcceptedRequest = _context.RoomRentalRequests
                .Any(r => r.UserId == userId && r.RequestStatusId == 2);

            if (!hasAcceptedRequest)
            {
                return BadRequest("You do not have an accepted room rental request.");
            }

            var existingTenant = _context.TenantUsers
                .Include(x => x.Tenant)
                .Any(x => x.UserId == userId && x.Tenant.BuildingId == BuildingId);

            if (!existingTenant)
            {
                var tenant = new Tenant
                {
                    BuildingId = BuildingId,
                    TenantTypeId = TenantTypeId,
                    Name = Name,
                    Tin = Tin,
                    Description = Description,
                    Contact = Contact,
                    IsActive = true
                };

                _context.Tenants.Add(tenant);
                _context.SaveChanges();

                CreateTenantUser(tenant.Id, userId.Value);
            }
            else
            {
                TempData["Error"] = "You already exist";
            }

            return Ok();
        }



        private void CreateTenantUser(int tenantId, int userId)
        {

            var tenantUser = new TenantUser
            {
                TenantId = tenantId,
                UserId = userId,
                CreatedDate = DateTime.Now,
                CreatedBy = userId,
                IsActive = true
            };

            _context.TenantUsers.Add(tenantUser);
            _context.SaveChanges();


        }





        [HttpPost]

        public async Task<IActionResult> AddRoomRentals(int TenantId, int RoomId, float TotalPrice, int BusinessAreaId)
        {
            if (TenantId <= 0)
            {
                return BadRequest("Invalid Tenant ID.");
            }

            var roomRental = new RoomRental
            {
                TenantId = TenantId,
                RoomId = RoomId,
                TotalPrice = TotalPrice,
                BusinessAreaId = BusinessAreaId,
                StartDate = DateTime.Now,
                IsActive = true
            };

            _context.RoomRentals.Add(roomRental);
            await _context.SaveChangesAsync();

            return Ok();
        }

      
        [HttpGet]
        public IActionResult PaymentList(int roomRentalId)
        {

            var roomrental = _context.RoomRentals
                .Include(x => x.RentalAgreementTerminations)
                .Include(x => x.RoomRentalPayments).ThenInclude(x => x.PaymentType)
                .Include(x => x.RoomRentalPayments).ThenInclude(x => x.PaymentMode)

                .FirstOrDefault(x => x.Id == roomRentalId);
            if (roomrental == null)
            {
                return NotFound();
            }

            return PartialView("_PaymentList", roomrental);
        }
        [HttpPost]
        public IActionResult ApproveRequest(int RentalTerminationId, string Remark)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                var approval = new RentalTerminationApproval
                {
                    RentalTerminationId = RentalTerminationId,
                    TerminationRequestStatusId =(int)terminationRequestStatuses.Approved, 
                    ActionDate = DateTime.Now,
                    ActionBy = userId.Value, 
                    Remark = Remark,
                    IsActive= true

                };

                _context.RentalTerminationApprovals.Add(approval);
                _context.SaveChanges();

                return Json(new { success = true, message = "Request approved successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error approving request", error = ex.Message });
            }
        }


        [HttpPost]
        public IActionResult RejectRequest(int RentalTerminationId, string Remark)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");  
                var rejection = new RentalTerminationApproval
                {
                    RentalTerminationId = RentalTerminationId,
                    TerminationRequestStatusId = (int)terminationRequestStatuses.Rejected,
                    ActionDate = DateTime.Now,
                    ActionBy = userId.Value, 
                    Remark = Remark,
                    IsActive = true
                };

                _context.RentalTerminationApprovals.Add(rejection);
                _context.SaveChanges();

                return Json(new { success = true, message = "Request rejected successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error rejecting request", error = ex.Message });
            }
        }
    }

}

