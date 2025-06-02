using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Data;
using NEXT_BMS.Models;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]

    public class MaintenanceRequestsController : Controller
    {
        private readonly NEXT_BMSContext _context;
        public MaintenanceRequestsController(NEXT_BMSContext context)
        {
            _context = context;

        }

        [HttpPost]
        public IActionResult GetMaintenanceRequests()
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
                var userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                var query = _context.MaintenanceRequests
                    .Include(m => m.User)
                    .Include(m => m.Room)
                    .Include(m => m.MaintenanceStatus)
                    .Include(m => m.MaintenanceType)
                    .Where(m => !m.IsDeleted&& m.UserId==userId)
                    .Select(m => new
                    {
                        m.Id,
                        Name = m.User.FirstName + " " + m.User.LastName,
                        Room = m.Room.Name,
                        MaintenanceStatus = m.MaintenanceStatus.Name,
                        MaintenanceType = m.MaintenanceType.Name,
                        DateSubmitted = m.DateSubmitted.ToString("yyyy-MM-dd"),
                        m.IsActive
                    });

               
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m =>
                        m.Name.Contains(searchValue) ||
                        m.Room.Contains(searchValue) ||
                        m.MaintenanceStatus.Contains(searchValue) ||
                        m.MaintenanceType.Contains(searchValue));
                }

                
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                recordsTotal = query.Count();

                var data = query.Skip(skip).Take(pageSize).ToList();

                var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Something went wrong.", detail = ex.Message });
            }
        }

       
        public IActionResult Index()
        {

            return View();
        }
       
        public IActionResult IndexAllocation(int id)
        {
            try
            {
                var tenantUserIds = _context.TenantUsers
                    .Where(tu => tu.TenantId == id)
                    .Select(tu => tu.UserId)
                    .ToList();
                if (!tenantUserIds.Any())
                {
                    return View(new List<MaintenanceRequest>());
                }

               
                var maintenanceRequests = _context.MaintenanceRequests
                    .Where(x => tenantUserIds.Contains(x.UserId) && !x.IsDeleted && x.IsActive)
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

        public async Task<IActionResult> DetailsAllocation(int? id)
        {
            if (id == null || _context.MaintenanceRequests == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var maintenanceRequest = await _context.MaintenanceRequests
                .Include(m => m.MaintenanceStatus)
                .Include(m => m.MaintenanceRequestAllocations).ThenInclude(m => m.AllocationStatus)
                .Include(m => m.MaintenanceRequestAllocations).ThenInclude(m => m.BuildingEmployee)
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

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MaintenanceRequests == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var maintenanceRequest = await _context.MaintenanceRequests
                .Include(m => m.MaintenanceStatus)
                .Include(m => m.MaintenanceType)
                .Include(m => m.Room)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (maintenanceRequest == null)
            {
                TempData["Error"] = "The information you're looking for was not found!"; return RedirectToAction("Index");
            }

            return View(maintenanceRequest);
        }

        public async Task<IActionResult> MyAllocation()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var buildingEmployee = await _context.BuildingEmployees.FirstOrDefaultAsync(x => x.UserId == userId);

            if (buildingEmployee == null)
            {
                TempData["Error"] = "You are not a building employee.";
                return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");
            }

            var buildingEmployeeId = buildingEmployee.Id;

            var allocations = await _context.MaintenanceRequestAllocations
                .Include(a => a.AllocationStatus)
                .Include(a => a.BuildingEmployee)
                .Include(a => a.MaintenanceRequest)
                    .ThenInclude(m => m.MaintenanceType)
                .Include(a => a.MaintenanceRequest)
                    .ThenInclude(m => m.Room)
                .Include(a => a.MaintenanceRequest)
                    .ThenInclude(m => m.User)
                .Where(a => a.BuildingEmployeeId == buildingEmployeeId)
                .ToListAsync();

            if (!allocations.Any())
            {
                TempData["Error"] = "No maintenance request allocations found for you!";
                return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");
            }

            return View(allocations); 
        }

        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var rooms = _context.Rooms
              .Include(x => x.RoomRentals).ThenInclude(x => x.Tenant)
              .ThenInclude(x => x.TenantUsers)
              .Where(x => x.RoomRentals.Any(tu => tu.Tenant.TenantUsers.Any(a => a.UserId == userId)))
               .Select(s => new
               {
                   Id = s.Id,
                   Name = s.Name
               });

            ViewData["MaintenanceStatusId"] = new SelectList(_context.MaintenanceStatuses.Where(x => x.IsDeleted == false), "Id", "Name");
            ViewData["MaintenanceTypeId"] = new SelectList(_context.MaintenanceTypes.Where(x => x.IsDeleted == false), "Id", "Name");
            ViewData["RoomId"] = new SelectList(rooms, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users.Where(x => x.IsDeleted == false), "Id", "Email");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,RoomId,MaintenanceTypeId,BuildingEmployeeId,MaintenanceStatusId,Description,DateSubmitted,IsActive,IsDeleted")] MaintenanceRequest maintenanceRequest)
        {

            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {

                TempData["Error"] = "User not logged in.";
                return RedirectToAction("Login", "Account");
            }

            maintenanceRequest.UserId = userId.Value;
            maintenanceRequest.MaintenanceStatusId = 1;


            var rooms = _context.Rooms
                .Include(x => x.RoomRentals).ThenInclude(x => x.Tenant)
                .ThenInclude(x => x.TenantUsers)
                .Where(x => x.RoomRentals.Any(tu => tu.Tenant.TenantUsers.Any(a => a.UserId == userId)))
                 .Select(s => new
                 {
                     Id = s.Id,
                     Name = s.Name
                 });





            ViewData["RoomId"] = new SelectList(rooms, "Id", "Name");

            maintenanceRequest.DateSubmitted = DateOnly.FromDateTime(DateTime.Now);

            if (ModelState.IsValid)
            {
                _context.Add(maintenanceRequest);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Maintenance request saved successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "An error occurred while saving maintenance request. Please review your input.";


            ViewData["MaintenanceStatusId"] = new SelectList(_context.MaintenanceStatuses.Where(x => x.IsDeleted == false), "Id", "Name", maintenanceRequest.MaintenanceStatusId);
            ViewData["MaintenanceTypeId"] = new SelectList(_context.MaintenanceTypes.Where(x => x.IsDeleted == false), "Id", "Name", maintenanceRequest.MaintenanceTypeId);

            return View(maintenanceRequest);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MaintenanceRequests == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
            if (maintenanceRequest == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            ViewData["MaintenanceStatusId"] = new SelectList(_context.MaintenanceStatuses.Where(x => x.IsDeleted == false), "Id", "Name", maintenanceRequest.MaintenanceStatusId);
            ViewData["MaintenanceTypeId"] = new SelectList(_context.MaintenanceTypes.Where(x => x.IsDeleted == false), "Id", "Name", maintenanceRequest.MaintenanceTypeId);
            ViewData["RoomId"] = new SelectList(_context.Rooms.Where(x => x.IsDeleted == false), "Id", "Description", maintenanceRequest.RoomId);
            ViewData["UserId"] = new SelectList(_context.Users.Where(x => x.IsDeleted == false), "Id", "Email", maintenanceRequest.UserId);
            return View(maintenanceRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,RoomId,MaintenanceTypeId,BuildingEmployeeId,MaintenanceStatusId,Description,DateSubmitted,IsActive,IsDeleted")] MaintenanceRequest maintenanceRequest)
        {
            if (id != maintenanceRequest.Id)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(maintenanceRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaintenanceRequestExists(maintenanceRequest.Id))
                    {
                        TempData["Error"] = "The information you're looking for was not found!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "maintenanceRequest saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving maintenanceRequest. Please review your input.";

            ViewData["MaintenanceStatusId"] = new SelectList(_context.MaintenanceStatuses.Where(x => x.IsDeleted == false), "Id", "Name", maintenanceRequest.MaintenanceStatusId);
            ViewData["MaintenanceTypeId"] = new SelectList(_context.MaintenanceTypes.Where(x => x.IsDeleted == false), "Id", "Name", maintenanceRequest.MaintenanceTypeId);
            ViewData["RoomId"] = new SelectList(_context.Rooms.Where(x => x.IsDeleted == false), "Id", "Description", maintenanceRequest.RoomId);
            ViewData["UserId"] = new SelectList(_context.Users.Where(x => x.IsDeleted == false), "Id", "Email", maintenanceRequest.UserId);
            return View(maintenanceRequest);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MaintenanceRequests == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var maintenanceRequest = await _context.MaintenanceRequests

                .Include(m => m.MaintenanceStatus)
                .Include(m => m.MaintenanceType)
                .Include(m => m.Room)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (maintenanceRequest == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            return View(maintenanceRequest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MaintenanceRequests == null)
            {
                return Problem("Entity set 'NEXT_BMSContext.MaintenanceRequests'  is null.");
            }
            var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
            if (maintenanceRequest != null)
            {
                maintenanceRequest.IsActive = false;
                maintenanceRequest.IsDeleted = true;
                _context.Update(maintenanceRequest);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MaintenanceRequestExists(int id)
        {
            return _context.MaintenanceRequests.Any(e => e.Id == id);
        }
        [HttpPost]
        public IActionResult SubmitRemark(int MaintenanceRequestAllocationId, string Remark)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (MaintenanceRequestAllocationId == 0 || string.IsNullOrWhiteSpace(Remark))
            {
                TempData["Error"] = "Invalid input.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            try
            {
                var report = new MaintenanceStatusReport
                {
                    MaintenanceRequestAllocationId = MaintenanceRequestAllocationId,
                    Remark = Remark,
                    ReportDate = DateTime.Now,
                    IsActive = true
                };
                _context.MaintenanceStatusReports.Add(report);
                _context.SaveChanges();
                TempData["Success"] = "Remark submitted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.InnerException?.Message ?? ex.Message;
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> removerequests(int Id)
        {
            var request = await _context.MaintenanceRequestAllocations.FindAsync(Id);
            
            if (request == null) return NotFound();
            request.IsActive = false;
            request.IsDeleted = true;
            _context.MaintenanceRequestAllocations.Update(request);
            await _context.SaveChangesAsync();

            return Ok();
        }


    }
}
