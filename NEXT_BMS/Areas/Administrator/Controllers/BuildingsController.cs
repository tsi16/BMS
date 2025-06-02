using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NEXT_BMS.Models;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    public class BuildingsController : Controller
    {
        private readonly NEXT_BMSContext _context;

        public BuildingsController(NEXT_BMSContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult GetBuildings()
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
                var returnData = (from manudata in _context.Buildings.Where(x => x.IsDeleted == false) select manudata);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    //returnData = returnData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    returnData = returnData.Where(m => m.Name.Contains(searchValue));
                }
                recordsTotal = returnData.Count();
                var data = returnData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
                return Ok(jsonData);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // GET: Buildings
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                ViewData["error"] = "Please login first";
            }
            var bIMSContext = _context.Buildings.Include(b => b.BuildingType).Include(b => b.City).Include(b => b.Location).Include(b => b.Owner).Include(b => b.OwnershipType).Include(b => b.User).Include(b => b.UseType);
           // return View(await bIMSContext.ToListAsync());
            var building = new Building();
            ViewData["BuildingTypeId"] = new SelectList(_context.BuildingTypes, "Id", "Name", building.BuildingTypeId);
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", building.CityId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", building.LocationId);
            ViewData["OwnerId"] = new SelectList(_context.Owners.Include(x => x.OwnerUsers).Where(x => x.OwnerUsers.Any(a => a.UserId == userId)), "Id", "FullName", building.OwnerId);
            ViewData["OwnerShipTypeId"] = new SelectList(_context.OwnershipTypes, "Id", "Name", building.OwnershipTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FirstName", building.UserId);
            ViewData["UseTypeId"] = new SelectList(_context.UseTypes, "Id", "Name", building.UseTypeId);
            return View(await bIMSContext.ToListAsync());


        }

        // GET: Buildings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var building = await _context.Buildings
                .Include(b => b.BuildingType)
                .Include(b => b.BuildingImages)
                .Include(b => b.City)
                .Include(b => b.Location)
                .Include(b => b.Owner)
                .Include(b => b.OwnershipType)
                .Include(b => b.Floors).ThenInclude(b => b.Rooms).ThenInclude(b => b.RoomStatus)
                .Include(b => b.Floors).ThenInclude(b => b.Rooms).ThenInclude(b => b.RoomPrices)
                .Include(b => b.Floors).ThenInclude(b => b.Rooms).ThenInclude(b => b.User)
                .Include(b => b.BuildingEmployees).ThenInclude(b => b.EmployeeType)
                .Include(b => b.User)
                .Include(b => b.UseType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (building == null)
            {
                return NotFound();
            }

            return View(building);
        }

        public IActionResult Create()
        {
            ViewData["BuildingTypeId"] = new SelectList(_context.BuildingTypes, "Id", "Id");
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Id");
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Id");
            ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "Id");
            ViewData["OwnershipTypeId"] = new SelectList(_context.OwnershipTypes, "Id", "Id");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["UseTypeId"] = new SelectList(_context.UseTypes, "Id", "Id");
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UseTypeId,UserId,CityId,LocationId,Name,Description,ConstractionYear,NumberOfFloor,BuildingTypeId,OwnershipTypeId,OwnerId,IsActive,IsDeleted")] Building building)
        {
            if (ModelState.IsValid)
            {
                _context.Add(building);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BuildingTypeId"] = new SelectList(_context.BuildingTypes, "Id", "Id", building.BuildingTypeId);
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Id", building.CityId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Id", building.LocationId);
            ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "Id", building.OwnerId);
            ViewData["OwnershipTypeId"] = new SelectList(_context.OwnershipTypes, "Id", "Id", building.OwnershipTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", building.UserId);
            ViewData["UseTypeId"] = new SelectList(_context.UseTypes, "Id", "Id", building.UseTypeId);
            return View(building);
        }

        // GET: Buildings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var building = await _context.Buildings.FindAsync(id);
            if (building == null)
            {
                return NotFound();
            }
            ViewData["BuildingTypeId"] = new SelectList(_context.BuildingTypes, "Id", "Id", building.BuildingTypeId);
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Id", building.CityId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Id", building.LocationId);
            ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "Id", building.OwnerId);
            ViewData["OwnershipTypeId"] = new SelectList(_context.OwnershipTypes, "Id", "Id", building.OwnershipTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", building.UserId);
            ViewData["UseTypeId"] = new SelectList(_context.UseTypes, "Id", "Id", building.UseTypeId);
            return View(building);
        }

        // POST: Buildings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UseTypeId,UserId,CityId,LocationId,Name,Description,ConstractionYear,NumberOfFloor,BuildingTypeId,OwnershipTypeId,OwnerId,IsActive,IsDeleted")] Building building)
        {
            if (id != building.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(building);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BuildingExists(building.Id))
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
            ViewData["BuildingTypeId"] = new SelectList(_context.BuildingTypes, "Id", "Id", building.BuildingTypeId);
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Id", building.CityId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Id", building.LocationId);
            ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "Id", building.OwnerId);
            ViewData["OwnershipTypeId"] = new SelectList(_context.OwnershipTypes, "Id", "Id", building.OwnershipTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", building.UserId);
            ViewData["UseTypeId"] = new SelectList(_context.UseTypes, "Id", "Id", building.UseTypeId);
            return View(building);
        }

        // GET: Buildings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var building = await _context.Buildings
                .Include(b => b.BuildingType)
                .Include(b => b.City)
                .Include(b => b.Location)
                .Include(b => b.Owner)
                .Include(b => b.OwnershipType)
                .Include(b => b.User)
                .Include(b => b.UseType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (building == null)
            {
                return NotFound();
            }

            return View(building);
        }

        // POST: Buildings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building != null)
            {
                _context.Buildings.Remove(building);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BuildingExists(int id)
        {
            return _context.Buildings.Any(e => e.Id == id);
        }
        
        public IActionResult Tenants(int? id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var approvedRoomRentalRequests = _context.RoomRentalRequests
                .Where(rr => rr.UserId == userId && rr.RequestStatusId == 2)
                .Select(rr => rr.RoomId)
                .ToList();

            ViewBag.ApprovedRoomRentalRequests = approvedRoomRentalRequests;


            var buildingId = _context.Rooms
                .Where(r => approvedRoomRentalRequests.Contains(r.Id))
                .Select(r => r.Floor.BuildingId)
                .Distinct()
                .ToList();


            var tenants = _context.Tenants
                .Include(x => x.Building)
                .Include(x => x.TenantType)
                .Where(x => buildingId.Contains(x.BuildingId));

            ViewData["BuildingId"] = new SelectList(_context.Buildings
                .Where(b => buildingId.Contains(b.Id)), "Id", "Name");
            ViewData["TenantTypeId"] = new SelectList(_context.TenantTypes, "Id", "Name");

            return View(tenants);
        }

        public IActionResult Tenant(int? id)
        {
            var tenant = _context.Tenants
                .Include(x => x.Building)
                .Include(x => x.RoomRentals).ThenInclude(x=>x.BusinessArea)
                 .Include(x => x.RoomRentals).ThenInclude(x => x.Room)
                 // .Include(x => x.RoomRentals).ThenInclude(x => x.Tenant)
                //.Include(x => x.Floors).ThenInclude(x => x.Rooms)
                //.Include(x => x.City)
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployee(string Name, int BuildingId, string PhoneNumber, int EmployeeTypeId)
        {

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                TempData["error"] = "User is not authenticated.";
                return RedirectToAction(nameof(Index));
            }

            var existingEmployee = _context.BuildingEmployees
                                            .Any(b => b.FullName.ToLower() == Name.ToLower() && b.BuildingId == BuildingId);

            if (existingEmployee)
            {
                TempData["info"] = "An Employee with this name already exists on this building.";
                return RedirectToAction(nameof(Index));
            }

            var buildingEmployee = new BuildingEmployee
            {
                FullName = Name,
                PhoneNumber = PhoneNumber,
                BuildingId = BuildingId,
                EmployeeTypeId = EmployeeTypeId,
                IsActive = true,
                IsDeleted = false
            };

            _context.BuildingEmployees.Add(buildingEmployee);
            await _context.SaveChangesAsync();

            return RedirectToAction("Employees", new { id = BuildingId });
        }

        public IActionResult Employees(int? id)
        {
            ViewData["EmployeeTypeId"] = new SelectList(_context.EmployeeTypes, "Id", "Name");
            var buildings = _context.Buildings
                .Include(b => b.BuildingEmployees).ThenInclude(b => b.EmployeeType)
                .FirstOrDefault(x => x.Id == id);
            return View(buildings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
       public async Task<IActionResult> AddBuilding(
    string Name,
    int UseTypeId,
    int UserId,
    int NumberOfFloor,
    int BuildingTypeId,
    int OwnerId,
    int OwnerShipTypeId,
    int CityId,
    int LocationId,
    string Description)
{
    int maxBuilding = 7;

    int creatingBuildings = getOwnerBuildingCount(OwnerId);
    if (creatingBuildings >= maxBuilding)
    {
        TempData["info"] = "You have reached the maximum";
        return RedirectToAction(nameof(Index));
    }

    int? userId = HttpContext.Session.GetInt32("UserId");
    if (!userId.HasValue)
    {
        TempData["error"] = "User is not authenticated.";
        return RedirectToAction(nameof(Index));
    }

    if (!ModelState.IsValid)
    {
        TempData["error"] = "Invalid input data.";
        return View();
    }

   
    var buildingRequestStatuses = await _context.BuildingRequestStatuses
        .FirstOrDefaultAsync(br => br.Id == userId.Value);

    if (buildingRequestStatuses == null)
    {
        TempData["error"] = "Please send a request before creating a building.";
        return RedirectToAction(nameof(Index));
    }

    
    if (buildingRequestStatuses.Id == 3)
    {
        TempData["error"] = "Your request has been rejected, and you cannot create buildings.";
        return RedirectToAction(nameof(Index));
    }

    if (buildingRequestStatuses.Id != 2)
    {
        TempData["error"] = "Please send a request before creating a building.";
        return RedirectToAction(nameof(Index));
    }

    var existingBuilding = _context.Buildings
        .Any(b => b.Name.ToLower() == Name.ToLower() && b.OwnerId == OwnerId);

    if (existingBuilding)
    {
        TempData["info"] = "A building with this name already exists at the specified location.";
        return RedirectToAction(nameof(Index));
    }

    var building = new Building
    {
        Name = Name,
        UseTypeId = UseTypeId,
        ConstractionYear = DateTime.Now.ToString("yyyy-MM-dd"),
        NumberOfFloor = NumberOfFloor,
        BuildingTypeId = BuildingTypeId,
        OwnerId = OwnerId,
        OwnershipTypeId = OwnerShipTypeId,
        CityId = CityId,
        LocationId = LocationId,
        Description = Description,
        UserId = userId.Value,
        IsActive = true,
        IsDeleted = false
    };

    try
    {
        _context.Buildings.Add(building);
        await _context.SaveChangesAsync();
        TempData["success"] = "Building added successfully!";
    }
    catch (Exception ex)
    {
        TempData["error"] = $"An error occurred while saving: {ex.Message}";
    }

    return RedirectToAction(nameof(Index));
}

        private int getOwnerBuildingCount(int ownerId)
        {
            return _context.Buildings.Where(a => a.OwnerId == ownerId).Count();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFloor(int FloorId, string Name)
        {
            var existingFloor = _context.Floors
                                              .Any(f => f.Name.ToLower() == Name.ToLower());


            if (existingFloor == true)
            {
                TempData["info"] = "A floor with this name already exists. Please choose a different name.";
                return RedirectToAction(nameof(Index));
            }

            var floor = await _context.Floors.FindAsync(FloorId);

            if (floor == null)
            {
                return NotFound();
            }

            try
            {
                floor.Name = Name;
                _context.Floors.Update(floor);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {

                if (!_context.Floors.Any(r => r.Id == FloorId))
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBuilding(int BuildingId, string Name, int userId, int UseTypeId, int NumberOfFloor, int BuildingTypeId, int OwnerId, int OwnershipTypeId, int CityId, int LocationId)
        {
            int? UserId = HttpContext.Session.GetInt32("UserId");
            if (!UserId.HasValue)
            {
                TempData["error"] = "User is not authenticated.";
                return RedirectToAction(nameof(Index));
            }

            var existingBuilding = _context.Buildings
                                            .Any(b => b.Name.ToLower() == Name.ToLower() && b.CityId == CityId && b.LocationId == LocationId);

            if (existingBuilding)
            {
                TempData["info"] = "A building with this name already exists at the specified location.";
                return RedirectToAction(nameof(Index));
            }

            var building = await _context.Buildings.FindAsync(BuildingId);

            if (building == null)
            {
                return NotFound();
            }

            try
            {
                building.Name = Name;
                building.UseTypeId = UseTypeId;
                building.OwnershipTypeId = OwnershipTypeId;
                building.BuildingTypeId = BuildingTypeId;
                building.OwnerId = OwnerId;
                building.NumberOfFloor = NumberOfFloor;
                building.CityId = CityId;
                building.LocationId = LocationId;

                _context.Update(building);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {

                if (!_context.Buildings.Any(r => r.Id == BuildingId))
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBuilding(int BuildingId)
        {
            var building = await _context.Buildings.FindAsync(BuildingId);

            if (building == null)
            {
                return NotFound();
            }

            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFloor(int FloorId)
        {
            var floor = await _context.Floors.FindAsync(FloorId);

            if (floor == null)
            {
                return NotFound();
            }

            _context.Floors.Remove(floor);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = floor.BuildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteEmployee(int BuildingEmployeeId)
        {
            var buildingEmployee = await _context.BuildingEmployees.FindAsync(BuildingEmployeeId);
            if (buildingEmployee == null)
            {
                return NotFound();
            }
            buildingEmployee.IsActive = false;
            buildingEmployee.IsDeleted = true;
            _context.Update(buildingEmployee);
            await _context.SaveChangesAsync();
            return RedirectToAction("Employees", new { id = buildingEmployee.BuildingId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoom(int RoomId)
        {
            var room = await _context.Rooms.FindAsync(RoomId);

            if (room == null)
            {
                return NotFound();
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(int RoomId, string Name, float Width, float Length, string Description)
        {
            var existingFloor = _context.Rooms
                                              .Any(f => f.Name.ToLower() == Name.ToLower());


            if (existingFloor == true)
            {
                TempData["info"] = "A Room with this name already exists. Please choose a different name.";
                return RedirectToAction(nameof(Index));
            }
            var room = await _context.Rooms.FindAsync(RoomId);

            if (room == null)
            {
                return NotFound();
            }
            room.Name = Name;

            room.Width = Width;
            room.Length = Length;
            room.Description = Description;

            try
            {
                _context.Rooms.Update(room);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {

                if (!_context.Rooms.Any(r => r.Id == RoomId))
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFloor(string Name, string NumberOfRoom, int BuildingId)
        {

            var existingFloor = _context.Floors
                                              .Any(f => f.BuildingId == BuildingId && f.Name.ToLower() == Name.ToLower());



            if (existingFloor == true)
            {
                TempData["info"] = "A floor with this name already exists. Please choose a different name.";
                return RedirectToAction(nameof(Index));
            }

            var floor = new Floor
            {
                Name = Name,
                NumberOfRoom = NumberOfRoom,
                BuildingId = BuildingId
            };

            _context.Floors.Add(floor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoom(string Name, int UserId, int FloorId, int RoomStatusId, int OwnerId, float Width, float Length, string Description)
        {

            int? userId = HttpContext.Session.GetInt32("UserId");
            var existingFloor = _context.Rooms
                                             .Any(f => f.FloorId == FloorId && f.Name.ToLower() == Name.ToLower());



            if (existingFloor == true)
            {
                TempData["info"] = "A Room with this name already exists. Please choose a different name.";
                return RedirectToAction(nameof(Index));
            }
            var room = new Room
            {
                Name = Name,
                FloorId = FloorId,
                RoomStatusId = 1,
                Width = Width,
                Length = Length,
                Description = Description
            };
            if (userId.HasValue)
            {
                room.UserId = userId.Value;
            }
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoomPrice(int RoomId, double price)
        {
            int? userId = HttpContext.Session.GetInt32("userid");

            var roomPrice = new RoomPrice
            {
                PricePerM2 = price,
                RoomId = RoomId,
                AppliedDate = DateOnly.FromDateTime(DateTime.Now),
                IsActive = true,
                IsDeleted = false

            };
            _context.RoomPrices.Add(roomPrice);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }

}
