using NEXT_BMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Wordprocessing;

namespace NEXT_BMS/*.Areas.Administrator*/.Controllers
{
   public class BuildingListController : Controller
    {
        private readonly NEXT_BMSContext _context;

       
        public BuildingListController(NEXT_BMSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? cityId, int? locationId, int? useTypeId, int page = 1)
        {
            int pageSize = 6;

            var buildingsQuery = _context.Buildings
                .Include(b => b.BuildingImages)
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Rooms)
                .Include(b => b.Location)
                    .ThenInclude(l => l.City)
                .Include(b => b.UseType)
                .AsQueryable();

           
            if (cityId.HasValue)
            {
                buildingsQuery = buildingsQuery.Where(b => b.Location.CityId == cityId);
            }

            
            if (locationId.HasValue)
            {
                buildingsQuery = buildingsQuery.Where(b => b.LocationId == locationId);
            }

            
            if (useTypeId.HasValue)
            {
                buildingsQuery = buildingsQuery.Where(b => b.UseTypeId == useTypeId);
            }

            var totalItems = await buildingsQuery.CountAsync();

            
            var buildings = await buildingsQuery
                .OrderBy(b => b.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new Building
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    Location = b.Location,
                    UseType = b.UseType,
                    BuildingImages = b.BuildingImages.ToList(),
                    Floors = b.Floors.Select(f => new Floor
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Rooms = f.Rooms
                            .Where(r => r.RoomStatusId == 1) 
                            .Select(r => new Room
                            {
                                Id = r.Id,
                                RoomStatusId = r.RoomStatusId
                            }).ToList()
                    }).ToList()
                })
                .ToListAsync();

           
            ViewBag.Cities = await _context.Cities
                .Where(c => c.Locations.Any(l => l.Buildings.Any()))
                .ToListAsync();

            ViewBag.Locations = cityId.HasValue
                ? await _context.Locations
                    .Where(l => l.CityId == cityId && l.Buildings.Any())
                    .ToListAsync()
                : new List<Location>();

            ViewBag.UseTypes = await _context.UseTypes.ToListAsync();

            
            ViewBag.SelectedCityId = cityId;
            ViewBag.SelectedLocationId = locationId;
            ViewBag.SelectedUseTypeId = useTypeId;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(buildings);
        }


        [HttpGet]
        public async Task<IActionResult> GetLocationsByCity(int cityId)
        {
            var locationIds = await _context.Buildings
                .Where(b => b.CityId == cityId && b.LocationId != null)
                .Select(b => b.LocationId)
                .Distinct()
                .ToListAsync();

            var locations = await _context.Locations
                .Where(l => locationIds.Contains(l.Id))
                .Select(l => new { l.Id, l.Name })
                .ToListAsync();

            return Json(locations);
        }



        //var cityIds = await _context.Buildings
        //    .Where(b => b.LocationId == locationId)
        //    .Select(b => b.CityId)
        //    .Distinct()
        //    .ToListAsync();

  public async Task<IActionResult> Details(
     int? id,
     int? floorId,
     int? cityId,
     int? locationId,
     int? useTypeId,
     int? page)
        {
            if (id == null)
            {
                return NotFound();
            }

            var building = await _context.Buildings
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Rooms)
                .FirstOrDefaultAsync(b => b.Id == id.Value);

            if (building == null)
            {
                return NotFound();
            }

            // Filter floors if a specific one is selected
            if (floorId.HasValue)
            {
                var selectedFloor = building.Floors.FirstOrDefault(f => f.Id == floorId.Value);
                if (selectedFloor != null)
                {
                    building.Floors = new List<Floor> { selectedFloor };
                }
            }

            // Preserve filter state for return link
            ViewBag.CityId = cityId;
            ViewBag.LocationId = locationId;
            ViewBag.UseTypeId = useTypeId;
            ViewBag.Page = page;

            return View(building);
        }


        [HttpGet]
        public async Task<IActionResult> GetRooms(int floorId)
        
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return Json(new { redirectUrl = Url.Action("GuestLogIn", "Account") });
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            var rooms = await _context.Rooms
                .Where(r => r.FloorId == floorId && !r.IsDeleted)
                .Include(x => x.RoomStatus)
                .Select(r => new
                {
                    roomId = r.Id,
                    r.Name,
                    RoomStatus = r.RoomStatus.Name,
                    r.Width,
                    r.Length,
                    RentalRequestId = r.RoomRentalRequests
                        .Where(x => x.IsActive && x.UserId == userId)
                        .Select(x => x.Id)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Json(rooms);
        }
        public IActionResult RoomList(int id) // floorId
        {
            var building = _context.Buildings
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Rooms)
                        .ThenInclude(r => r.RoomStatus)
                .FirstOrDefault(b => b.Floors.Any(f => f.Id == id));

            if (building == null)
            {
                return NotFound();
            }

            // Filter to just the selected floor
            building.Floors = building.Floors
                .Where(f => f.Id == id)
                .ToList();

            return View(building); // This works with your existing view
        }


        public IActionResult saverentalRequest(int RoomId, string Description)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int? userId = HttpContext.Session.GetInt32("UserId");


            var existingRequest = _context.RoomRentalRequests
                .FirstOrDefault(r => r.RoomId == RoomId && r.UserId == userId && !r.IsDeleted);

            if (existingRequest == null)
            {

                var roomRentalRequest = new RoomRentalRequest
                {
                    RoomId = RoomId,
                    Description = Description,
                    UserId = userId.Value,
                    RequestedDate = DateTime.Now,
                    RequestStatusId = 1,
                    IsActive = true,
                };

                _context.RoomRentalRequests.Add(roomRentalRequest);
                _context.SaveChanges();
                TempData["success"] = "Request saved!";
            }
            else
            {
                Console.WriteLine("Existing request found:");
                Console.WriteLine($"Request ID: {existingRequest.Id}, Room ID: {existingRequest.RoomId}, User ID: {existingRequest.UserId}");

                TempData["Info"] = "Request for this room already exists.";
            }

            return Redirect(Request.GetTypedHeaders().Referer.ToString());
        }
        public async Task<IActionResult> Deleterentalrequest(int id)
        {
            var roomRentalRequest = await _context.RoomRentalRequests.FirstOrDefaultAsync(x => x.Id == id);
            if (roomRentalRequest == null) return NotFound();
            roomRentalRequest.IsActive = false;
            roomRentalRequest.IsDeleted = true;
            _context.RoomRentalRequests.Update(roomRentalRequest);
            await _context.SaveChangesAsync();
            return Ok();

        }
    }
}




