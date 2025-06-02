using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NEXT_BMS.Models;


namespace NEXT_BMS.Controllers
{

    public class BuildingRequestsController : Controller
    {
        private readonly NEXT_BMSContext _context;

        public BuildingRequestsController(NEXT_BMSContext context)
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

            return RedirectToAction("Index", "BuildingRequests", new { area = "Administrator" });
           
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buildingRequest = await _context.BuildingRequests
                .Include(b => b.BuildingType)
                .Include(b => b.Location)
                .Include(b => b.RequestStatus)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (buildingRequest == null)
            {
                return NotFound();
            }

            return View(buildingRequest);
        }

        public IActionResult Create()
        {
            ViewData["BuildingTypeId"] = new SelectList(_context.BuildingTypes, "Id", "Name");
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Description,Amount,LocationId,BuildingTypeId,RequestStatusId,RequestedDate,IsActive,IsDeleted")] BuildingRequest buildingRequest)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            buildingRequest.UserId = userId.Value;

            if (ModelState.IsValid)
            {
                var Rs = new BuildingRequest
                {
                    UserId = userId.Value,
                    RequestStatusId = 1,
                    BuildingTypeId = buildingRequest.BuildingTypeId,
                    LocationId = buildingRequest.LocationId,
                    RequestedDate = buildingRequest.RequestedDate = DateTime.Now,
                    Description = buildingRequest.Description,
                    Amount = buildingRequest.Amount,
                    IsActive= true
                };


                _context.BuildingRequests.Add(Rs);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["BuildingTypeId"] = new SelectList(_context.BuildingTypes, "Id", "Name", buildingRequest.BuildingTypeId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", buildingRequest.LocationId);
           
            return View(buildingRequest);
        }

       
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buildingRequest = await _context.BuildingRequests.FindAsync(id);
            if (buildingRequest == null)
            {
                return NotFound();
            }
            ViewData["BuildingTypeId"] = new SelectList(_context.BuildingTypes, "Id", "Name", buildingRequest.BuildingTypeId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", buildingRequest.LocationId);
            ViewData["RequestStatusId"] = new SelectList(_context.BuildingRequestStatuses, "Id", "Name", buildingRequest.RequestStatusId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", buildingRequest.UserId);
            return View(buildingRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Description,Amount,LocationId,BuildingTypeId,RequestStatusId,RequestedDate,IsActive,IsDeleted")] BuildingRequest buildingRequest)
        {
            if (id != buildingRequest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(buildingRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BuildingRequestExists(buildingRequest.Id))
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
            ViewData["BuildingTypeId"] = new SelectList(_context.BuildingTypes, "Id", "Name", buildingRequest.BuildingTypeId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", buildingRequest.LocationId);
            ViewData["RequestStatusId"] = new SelectList(_context.BuildingRequestStatuses, "Id", "Name", buildingRequest.RequestStatusId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", buildingRequest.UserId);
            return View(buildingRequest);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buildingRequest = await _context.BuildingRequests
                .Include(b => b.BuildingType)
                .Include(b => b.Location)
                .Include(b => b.RequestStatus)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (buildingRequest == null)
            {
                return NotFound();
            }

            return View(buildingRequest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var buildingRequest = await _context.BuildingRequests.FindAsync(id);
            if (buildingRequest != null)
            {
                _context.BuildingRequests.Remove(buildingRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BuildingRequestExists(int id)
        {
            return _context.BuildingRequests.Any(e => e.Id == id);
        }
        public async Task<IActionResult> AdminReq()
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
       
     public IActionResult AdminReqD( int? id)
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
        
        [HttpPost]
        public async Task<IActionResult> UpdateRequestStatus(int id, int status)
        {
            
            var buildingRequest = await _context.BuildingRequests.FindAsync(id);

            if (buildingRequest == null)
            {
                return NotFound();
            }
 
            buildingRequest.RequestStatusId = status;


            _context.Update(buildingRequest);
            await _context.SaveChangesAsync();

            
            TempData["SuccessMessage"] = "Request status updated successfully.";
            return Redirect(Request.GetTypedHeaders().Referer.ToString());
        }
    }
  }


