using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NEXT_BMS.Models;
using NuGet.Protocol;

namespace NEXT_BMS.Controllers
{
    public class ShopsController : Controller
    {
        private readonly NEXT_BMSContext _context;

        private int Id;
        public ShopsController(NEXT_BMSContext context)
        {
            _context = context;
        }

    public async Task<IActionResult> Index(int? cityId, int? locationId, int? businessAreaId, string searchItem, int page = 1, int pageSize = 12)
        {
            var shopsQuery = _context.Shops
                .Include(s => s.BusinessArea)
                .Include(s => s.ShopLocations)
                .ThenInclude(sl => sl.Room)
                .Include(s => s.ShopItems).ThenInclude(s => s.Item).ThenInclude(s => s.ItemCategory)
                .AsQueryable();

            // Filter by city
            if (cityId.HasValue)
            {
                shopsQuery = shopsQuery.Where(s => s.ShopLocations
                    .Any(sl => sl.Room.Floor.Building.CityId == cityId));
            }
            // Filter Locations only if CityId is selected
            var locations = _context.Locations
                                    .Where(l => !cityId.HasValue || l.CityId == cityId)
                                    .ToList();

            // Filter by location
            if (locationId.HasValue)
            {
                shopsQuery = shopsQuery.Where(s => s.ShopLocations.Any(sl => sl.Room.Floor.Building.LocationId == locationId));
            }

            // Filter by business area
            if (businessAreaId.HasValue)
            {
                shopsQuery = shopsQuery.Where(s => s.BusinessAreaId == businessAreaId);
            }

            // Search for an item 
            if (!string.IsNullOrEmpty(searchItem))
            {
                shopsQuery = shopsQuery.Where(s => s.ShopItems.Any(i => i.Item.Name.Contains(searchItem)));
            }
            // Count the total shops (for pagination)
            var totalShops = await shopsQuery.CountAsync();

            // Fetch the current page of shops
            var filteredShops = await shopsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            // Pass the filters back to the view for pre-selection
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", cityId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", locationId);
            ViewData["BusinessAreaId"] = new SelectList(_context.BusinessAreas, "Id", "Name", businessAreaId);
            // Pass pagination data
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalShops / (double)pageSize);

            return View(filteredShops);
        }

       

   public async Task<IActionResult> Details(int? id, int? shopCityId, int? locationId, int? businessAreaId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops
                .Include(s => s.BusinessArea)
                .Include(s => s.ShopLocations)
                .ThenInclude(sl => sl.Room)
                .ThenInclude(r => r.Floor)
                .ThenInclude(f => f.Building)
                .Include(s => s.ShopItems).ThenInclude(s => s.Item).ThenInclude(s => s.ItemCategory)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (shop == null)
            {
                return NotFound();
            }

            // Set cityId if not passed as parameter
            shopCityId ??= shop.ShopLocations?.FirstOrDefault()?.Room?.Floor?.Building?.CityId;

            if (shopCityId == null)
            {
                Console.WriteLine("Shop CityId is null");
            }

            // Fetch related shops based on same BusinessArea and City
            var relatedShopsQuery = _context.Shops
                .Include(s => s.BusinessArea)
                .Include(s => s.ShopLocations)
                .ThenInclude(sl => sl.Room)
                .ThenInclude(r => r.Floor)
                .ThenInclude(f => f.Building)
                .ThenInclude(b => b.City)
                .Where(s => s.Id != id &&  // Exclude current shop
                            s.BusinessArea.Id == shop.BusinessArea.Id &&  // Same Business Area
                            s.ShopLocations.Any(sl => sl.Room.Floor.Building.CityId == shopCityId));// Same City

            var relatedShops = await relatedShopsQuery.ToListAsync();

            Console.WriteLine($"Related shops count: {relatedShops.Count}");

            // Pass related shops to the view
            ViewBag.RelatedShops = relatedShops;
            // Pass the filters back to the view for pre-selection
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", shopCityId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", locationId);
            ViewData["BusinessAreaId"] = new SelectList(_context.BusinessAreas, "Id", "Name", businessAreaId);


            return View(shop);
        }

   public IActionResult Create(int? userId)
        {
            // Check if the user is logged in by verifying the session
            var loggedInUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(loggedInUserId))
            {
                TempData["ErrorMessage"] = "You are not logged in. Please log in to create a shop.";
                return RedirectToAction("Login", "Users");
            }

            if (!int.TryParse(loggedInUserId, out int loggedUserId))
            {
                TempData["ErrorMessage"] = "Invalid session user ID.";
                return RedirectToAction("Login", "Users");
            }

            // Ensure that the provided `userId` matches the logged-in user's ID
            if (userId != null && userId != loggedUserId)
            {
                TempData["ErrorMessage"] = "You are not authorized to create a shop.";
                return RedirectToAction("Index", "Shops");
            }
            // Pass the logged-in user's ID to the View
            ViewData["UserId"] = loggedUserId;

            ViewData["BusinessAreaId"] = new SelectList(_context.BusinessAreas, "Id", "Name");

            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,UserId,BusinessAreaId,Description,IsActive,IsDeleted")] Shop shop, IFormFile image)
        {
            if (!ModelState.IsValid)



            {
                if (image != null)
                {
                    // Upload the image and get its URL
                    var imagUrl = await UploadImage(image);

                    // Save the URL to the shop model
                    shop.ImagUrl = imagUrl;
                }

                _context.Add(shop);
                await _context.SaveChangesAsync();

            }
            ViewData["BusinessAreaId"] = new SelectList(_context.BusinessAreas, "Id", "Name", shop.BusinessAreaId);
            return View(shop);
        }

       
        public async Task<IActionResult> Edit(int? id, int? userId)
        {

            // Check if the user is logged in by verifying the session or authentication
            var loggedInUserId = HttpContext.Session.GetString("UserId");
            if (id == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops.FindAsync(id);
            if (shop == null)
            {
                return NotFound();
            }
            ViewData["BusinessAreaId"] = new SelectList(_context.BusinessAreas, "Id", "Name", shop.BusinessAreaId);
            ViewData["UserId"] = userId;

            return View(shop);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,UserId,BusinessAreaId,Description,IsActive,IsDeleted")] Shop shop)
        {
            if (id != shop.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShopExists(shop.Id))
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
            ViewData["BusinessAreaId"] = new SelectList(_context.BusinessAreas, "Id", "Name", shop.BusinessAreaId);

            return View(shop);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            // Check if the user is logged in by verifying the session or authentication
            var loggedInUserId = HttpContext.Session.GetString("UserId");
            if (id == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops
                .Include(s => s.BusinessArea)
                .Include(s => s.User)
                .Include(s => s.ShopItems).ThenInclude(x=>x.Item).ThenInclude(x => x.ItemCategory)
                
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shop == null)
            {
                return NotFound();
            }

            return View(shop);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shop = await _context.Shops.FindAsync(id);
            if (shop != null)
            {
                _context.Shops.Remove(shop);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShopExists(int id)
        {
            return _context.Shops.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> GetLocationsByCity(int? cityId)
        {
            if (!cityId.HasValue)
            {
                return Json(new List<object>());
            }

            var locations = await _context.Locations
                .Where(l => l.CityId == cityId)
                .Select(l => new { l.Id, l.Name })
                .ToListAsync();

            return Json(locations);
        }

        [HttpPost("UploadImage")]
        private async Task<string> UploadImage(IFormFile image)
        {
            try
            {
                if (image != null && image.Length > 0)
                {
                    // Define the path where the image will be saved
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                    // Ensure the directory exists
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Create a unique file name
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

                    // Combine the path and file name
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    // Return the relative URL to the saved image
                    return $"/images/{fileName}";
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Image upload failed: {ex.Message}");
            }
            return null;

        }

        [HttpGet]
        public async Task<IActionResult> HasShop()
        {
            var loggedInUserId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(loggedInUserId) || !int.TryParse(loggedInUserId, out int userId))
            {
                return Json(new { hasShop = false });
            }

            var hasShop = await _context.Shops.AnyAsync(s => s.UserId == userId && !s.IsDeleted);
            return Json(new { hasShop });
        }

        [HttpGet("Shops/ShopDashboard/{shopId}")]
        public async Task<IActionResult> ShopDashboard(int shopId)
        {
            var loggedInUserId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(loggedInUserId) || !int.TryParse(loggedInUserId, out int userId))
            {
                TempData["ErrorMessage"] = "You are not logged in. Please log in to access this page.";
                return RedirectToAction("Login", "Users");
            }

            // Fetch the shop details and ensure it belongs to the logged-in user
            var shop = await _context.Shops
                .Include(s => s.BusinessArea)
                .Include(s => s.ShopItems).ThenInclude(s => s.Item).ThenInclude(s => s.ItemCategory)
                .FirstOrDefaultAsync(s => s.Id == shopId && s.UserId == userId);

            if (shop == null)
            {
                TempData["ErrorMessage"] = "Unauthorized access or shop not found.";
                return RedirectToAction("Index");
            }


            var itemCount = shop.ShopItems.Count; // Count items
            ViewBag.ItemCount = itemCount;

            // Add any additional data for display in the dashboard
            ViewBag.ShopName = shop.Name;
            ViewBag.BusinessArea = shop.BusinessArea.Name;

            return View(shop);
        }

        [HttpPost]
        public JsonResult LoadItems(int shopId, int page, int pageSize, string itemName)
        {
            var itemsQuery = _context.ShopItems
                .Include(x => x.Item).ThenInclude(x => x.ItemImages)
                .Where(x => x.ShopId == shopId && x.IsActive);

            int totalItems = itemsQuery.Count();

            var items = itemsQuery
                .OrderBy(item => item.Item.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .Select(s => new
                {
                    s.Id,
                    ItemName = s.Item.Name,
                    s.Item.ItemImages.FirstOrDefault()?.Url
                });
            if (!string.IsNullOrEmpty(itemName))
            {
                items = items.Where(x => x.ItemName.ToLower().StartsWith(itemName.ToLower()));
            }

            var result = new
            {
                items,
                totalItems
            };


            return Json(result);


        }

        public IActionResult ItemDetail(int id)
        {
            var shopItem = _context.ShopItems
                .Include(x => x.Item).ThenInclude(x => x.ItemImages)
                .Include(x => x.ItemEntries).ThenInclude(x => x.ItemEntryVarations).ThenInclude(x => x.VarationType).ThenInclude(x => x.Varations)
                 .Include(x => x.ItemEntryVarations)
                .FirstOrDefault(x => x.Id == id);


            ViewData["VarationTypeId"] = new SelectList(_context.VarationTypes, "Id", "Name");
            ViewData["VarationId"] = new SelectList(_context.Varations, "Id", "Name");
            return View(shopItem);
        }

        [HttpPost]
        public async Task<IActionResult> Edititemimage(IFormFile image, int itemImageId)
        {
            var itemImage = await _context.ItemImages
           .FirstOrDefaultAsync(img => img.Id == itemImageId && img.IsActive);


            if (itemImage == null)
            {
                return NotFound(new { message = "item image not found." });
            }
            string url = "";
            if (url != "Invalid file type" && image.Length < 5 * 1024 * 1024)
            {

                var Url = Editupload(image, "images");
                itemImage.Url = Url;
                _context.ItemImages.Update(itemImage);
                await _context.SaveChangesAsync();

                TempData["Success"] = "image updated successfully.";
                return Redirect(Request.GetTypedHeaders().Referer.ToString());
            }
            else
            {
                TempData["Error"] = "image error.";
            }

            return View(Url);
        }

        private string Editupload(IFormFile image, string dir)
        {
            string url = "";

            if (image != null && image.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    url = "Invalid file type";
                    return url;
                }

                if (image.Length > 5 * 1024 * 1024)
                {
                    url = "File size exceeds limi";
                    return url;
                }


                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }


                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }



                url = $"/{dir}/" + uniqueFileName;
            }
            return url;
        }

    }
}
