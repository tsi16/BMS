using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Data;
using NEXT_BMS.Models;
using hu_utils;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    public class BuildingEmployeesController : Controller
    {
        private readonly NEXT_BMSContext _context;
        public BuildingEmployeesController(NEXT_BMSContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetBuildingEmployees ()
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
                var returnData = (from manudata in _context.BuildingEmployees.Where(x=>x.IsDeleted==false) select manudata);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    returnData = returnData.OrderBy(sortColumn + " " + sortColumnDirection);
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

        public IActionResult Index()
        {
            return View();
        }
     
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BuildingEmployees == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var buildingEmployee = await _context.BuildingEmployees
                .Include(b => b.Building)
                .Include(b => b.EmployeeType)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (buildingEmployee == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";return RedirectToAction("Index");
            }

            
            var userExists = _context.Users.Any(u => u.PhoneNumber == buildingEmployee.PhoneNumber); 
                                                      
            ViewData["UserExists"] = userExists; 

            return View(buildingEmployee);
        }

        public IActionResult Create()
        {
            ViewData["BuildingId"] = new SelectList(_context.Buildings  .Where(x=>x.IsDeleted==false), "Id", "ConstractionYear");
            ViewData["EmployeeTypeId"] = new SelectList(_context.EmployeeTypes  .Where(x=>x.IsDeleted==false), "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users  .Where(x=>x.IsDeleted==false), "Id", "FirstName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,PhoneNumber,BuildingId,UserId,EmployeeTypeId,IsActive,IsDeleted")] BuildingEmployee buildingEmployee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(buildingEmployee);
                await _context.SaveChangesAsync();
                TempData["Success"] = "buildingEmployee saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving buildingEmployee. Please review your input.";
            ViewData["BuildingId"] = new SelectList(_context.Buildings .Where(x=>x.IsDeleted==false), "Id", "ConstractionYear", buildingEmployee.BuildingId);
            ViewData["EmployeeTypeId"] = new SelectList(_context.EmployeeTypes .Where(x=>x.IsDeleted==false), "Id", "Name", buildingEmployee.EmployeeTypeId);
            ViewData["UserId"] = new SelectList(_context.Users .Where(x=>x.IsDeleted==false), "Id", "FirstName", buildingEmployee.UserId);
            return View(buildingEmployee);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BuildingEmployees == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var buildingEmployee = await _context.BuildingEmployees.FindAsync(id);
            if (buildingEmployee == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            ViewData["BuildingId"] = new SelectList(_context.Buildings  .Where(x=>x.IsDeleted==false), "Id", "ConstractionYear", buildingEmployee.BuildingId);
            ViewData["EmployeeTypeId"] = new SelectList(_context.EmployeeTypes  .Where(x=>x.IsDeleted==false), "Id", "Name", buildingEmployee.EmployeeTypeId);
            ViewData["UserId"] = new SelectList(_context.Users  .Where(x=>x.IsDeleted==false), "Id", "FirstName", buildingEmployee.UserId);
            return View(buildingEmployee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,PhoneNumber,BuildingId,UserId,EmployeeTypeId,IsActive,IsDeleted")] BuildingEmployee buildingEmployee)
        {
            if (id != buildingEmployee.Id)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(buildingEmployee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BuildingEmployeeExists(buildingEmployee.Id))
                    {
                        TempData["Error"] = "The information you're looking for was not found!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "buildingEmployee saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving buildingEmployee. Please review your input.";
            ViewData["BuildingId"] = new SelectList(_context.Buildings  .Where(x=>x.IsDeleted==false), "Id", "ConstractionYear", buildingEmployee.BuildingId);
            ViewData["EmployeeTypeId"] = new SelectList(_context.EmployeeTypes  .Where(x=>x.IsDeleted==false), "Id", "Name", buildingEmployee.EmployeeTypeId);
            ViewData["UserId"] = new SelectList(_context.Users  .Where(x=>x.IsDeleted==false), "Id", "FirstName", buildingEmployee.UserId);
            return View(buildingEmployee);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BuildingEmployees == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var buildingEmployee = await _context.BuildingEmployees
                .Include(b => b.Building)
                .Include(b => b.EmployeeType)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (buildingEmployee == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }
            return View(buildingEmployee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BuildingEmployees == null)
            {
                return Problem("Entity set 'NEXT_BMSContext.BuildingEmployees'  is null.");
            }
            var buildingEmployee = await _context.BuildingEmployees.FindAsync(id);
            if (buildingEmployee != null)
            {
                 buildingEmployee.IsActive = false;
                 buildingEmployee.IsDeleted = true;
                _context.Update(buildingEmployee);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BuildingEmployeeExists(int id)
        {
          return _context.BuildingEmployees.Any(e => e.Id == id);
        }

        public async Task<IActionResult> LinkDetails(int? id)
        {
            if (id == null || _context.BuildingEmployees == null)
            {
                TempData["Error"] = "The information you're looking for was not found!";
                return RedirectToAction("Index");
            }

            var buildingEmployee = await _context.BuildingEmployees
                .Include(b => b.Building)
                .Include(b => b.EmployeeType)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (buildingEmployee == null)
            {
                TempData["Error"] = "The information you're looking for was not found!"; return RedirectToAction("Index");
            }

            return View(buildingEmployee);
        }


        [HttpPost]

        public IActionResult SignUp(int buildingemployeeId ,string firstName, string middleName, string phoneNumber, string email, string lastName, int genderId, string userName, string password)
        {
            try
            {
                var newUser = new User
                {
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,
                    UserName = userName,
                    Password = password,
                    DefaultLanguageId = 1,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    GenderId = genderId,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,

                };

                _context.Users.Add(newUser);
                _context.SaveChanges();
                encryptPassword(newUser.Id, newUser.Password);
                updatebuildingemployeeId(buildingemployeeId, newUser.Id);
                TempData["Success"] = " you have successfully registered!!";

                return Redirect(Request.GetTypedHeaders().Referer.ToString());

            }
            catch (Exception ex)
            {
                TempData["Error"] = "Not registered.";
                return View();
            }
        }


        private void encryptPassword(int userId, string password)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            user.Password = Password._one_way_encrypt(password, userId);
            _context.Update(user);
            _context.SaveChanges();
        }

        private void updatebuildingemployeeId(int buildingemployeeId,int userId)
        {
            var buildingemployee = _context.BuildingEmployees.FirstOrDefault(x => x.Id == buildingemployeeId);
             buildingemployee.UserId = userId;
             _context.Update(buildingemployee);
             _context.SaveChanges();
        }
        public JsonResult GetUsers(string searchData)
        {
            var users = _context.Users.Where(x => x.PhoneNumber.Contains(searchData) || x.FirstName.Contains(searchData))
                .Select(x => new
                 {
                    x.Id,
                     FirstName = x.FirstName,
                     PhoneNumber = x.PhoneNumber,
                     Email = x.Email
                     
                   
                })
            .ToList();


            return Json(users);
        }

        [HttpPost]
        public JsonResult LinkUser(int buildingEmployeeId, int userId)
        {
            
            var buildingEmployee = _context.BuildingEmployees.FirstOrDefault(x => x.Id == buildingEmployeeId);
            if (buildingEmployee == null)
            {
                
                return Json(new { success = false, message = "Building employee not found." });
            }

            updatebuildingemployeeId(buildingEmployeeId, userId);

            return Json(new { success = true });
        }
    }
}
