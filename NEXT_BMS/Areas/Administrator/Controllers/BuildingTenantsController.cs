using NEXT_BMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    public class BuildingTenantsController : Controller
    {
        private readonly NEXT_BMSContext _context;

        public BuildingTenantsController(NEXT_BMSContext context)
        {
            _context = context;
        }
        public IActionResult Index(int? tenantId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var userTenants = _context.Tenants
                .Include(t => t.Building)
                .Include(t => t.TenantType)
                .Include(t => t.TenantUsers)
                .Where(t => t.TenantUsers.Any(tu => tu.UserId == userId))
                .ToList();

            ViewData["TenantId"] = new SelectList(userTenants, "Id", "Name");
            ViewBag.SelectedTenantId = tenantId;

            var filteredTenants = tenantId.HasValue
                ? userTenants.Where(t => t.Id == tenantId.Value).ToList()
                : new List<Tenant>();

            return View(filteredTenants);
        }

        public IActionResult Details(int? id)
        {
            var tenant = _context.Tenants
                .Include(x => x.Building)
                .Include(x => x.RoomRentals).ThenInclude(x => x.BusinessArea)
                .Include(x => x.RoomRentals).ThenInclude(x => x.Room)
                 .Include(x => x.RoomRentals).ThenInclude(x => x.RentalAgreementTerminations)
               
                .Include(x => x.TenantType)
                .FirstOrDefault(m => m.Id == id);
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }


            var approvedRoomRentalRequests = _context.RoomRentalRequests
                .Where(rr => rr.UserId == userId && rr.RequestStatusId == 2)
                .Select(rr => rr.RoomId);
           

            var rooms = _context.Rooms
                .Include(x => x.Floor)
                    .ThenInclude(x => x.Building)
                        .ThenInclude(x => x.City)
                .Where(x => x.IsActive && approvedRoomRentalRequests.Contains(x.Id))
                .Select(s => new
                {
                    s.Id,
                    Name = $"{s.Name}/{s.Floor.Name}/{s.Floor.Building.Name}/{s.Floor.Building.City.Name}"
                });
            ViewData["RoomId"] = new SelectList(rooms, "Id", "Name");
            ViewData["BusinessAreaId"] = new SelectList(_context.BusinessAreas, "Id", "Name");

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

        private void CreateTenantUser(int tenantId , int userId)
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
        [HttpPost]
        public IActionResult AddRequest(int RoomRentalId, string Reason)
        {
           var rentalAgreementTermination = new RentalAgreementTermination
            {
                RoomRentalId = RoomRentalId,
                Reason = Reason,
                DocumentId = 1,

                IsActive = true,
                
            };

            _context.RentalAgreementTerminations.Add(rentalAgreementTermination);
             _context.SaveChanges();

            return Ok();
        }

    }
}
