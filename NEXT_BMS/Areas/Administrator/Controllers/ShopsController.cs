using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NEXT_BMS.Models;
using NuGet.Protocol;
using static System.Net.Mime.MediaTypeNames;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]

    public class ShopsController : Controller
    {
        private readonly NEXT_BMSContext _context;

        public ShopsController(NEXT_BMSContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetShops()

        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

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
                var returnData = (from manudata in _context.Shops.Where(x => x.UserId == userId)
                .Include(s => s.BusinessArea)
                .Include(s => s.User).Select(s => new
                {
                    s.Id,
                    s.Name,
                    BusinessArea = s.BusinessArea.Name,
                   s.IsActive

                })
                                  select manudata);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                  
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

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }


            ViewData["BusinessAreaId"] = new SelectList(_context.BusinessAreas, "Id", "Name");


            var shopRequests = await _context.ShopRequests
                .Where(r => r.UserId == userId)
                .Include(r => r.RequestStatus)
                .Include(r => r.User)
                .ToListAsync();

            ViewBag.ShopRequests = shopRequests;

            bool canCreateShop = shopRequests.Any(r => r.RequestStatusId == 1);
            ViewBag.CanCreateShop = canCreateShop;

            return View();
        }

       
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops
                .Include(s => s.BusinessArea)
                .Include(s => s.User)
                .Include(s => s.ShopItems).ThenInclude(x => x.Item).ThenInclude(x => x.ItemCategory)
                .Include(s => s.ShopItems).ThenInclude(x => x.Item).ThenInclude(x => x.ItemImages)
                .Include(s => s.ShopLocations).ThenInclude(x => x.Room).ThenInclude(x => x.Floor).ThenInclude(x => x.Building).ThenInclude(x => x.City)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shop == null)
            {
                return NotFound();
            }
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }


            var items = _context.Items.Where(x => x.UserId == userId);
            ViewData["ItemId"] = new SelectList(items, "Id", "Name");


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
            return View(shop);
        }

        public IActionResult Create()
        {
            ViewData["BusinessAreaId"] = new SelectList(_context.BusinessAreas, "Id", "Name");
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Name");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create([Bind("Id,Name,UserId,BusinessAreaId,ItemId,Description,IsActive,IsDeleted,CreatedDate")] Shop shop)
        {
            int? sessionUserId = HttpContext.Session.GetInt32("UserId");

            if (!sessionUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            shop.UserId = sessionUserId.Value;

            
            if (shop.CreatedDate == default(DateTime))
            {
                shop.CreatedDate = DateTime.Now;
            }

            var approvedShopRequest = await _context.ShopRequests
                .FirstOrDefaultAsync(sr => sr.UserId == shop.UserId && sr.RequestStatusId == 1);

            if (approvedShopRequest != null)
            {
                int shopCount = await _context.Shops.CountAsync(s => s.UserId == shop.UserId);

                if (shopCount >= approvedShopRequest.NumberOfShops)
                {
                    TempData["Error"] = "You cannot create more shops than what was approved in your Shop Request.";
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                TempData["Error"] = "You have no approved Shop Requests.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                _context.Add(shop);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["BusinessAreaId"] = new SelectList(_context.BusinessAreas, "Id", "Name", shop.BusinessAreaId);
            return View(shop);
        }
        public async Task<IActionResult> Edit(int? id)
        {

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

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", shop.UserId);
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,UserId,BusinessAreaId,ItemId,Description,IsActive,IsDeleted")] Shop shop)
        {
            if (id != shop.Id)
            {
                return NotFound();
            }
            var userId = HttpContext.Session.GetInt32("UserId");


            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
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

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", shop.UserId);
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops
                .Include(s => s.BusinessArea)
                .Include(s => s.User)
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




        [HttpPost]
        public async Task<IActionResult> AddItem(int ShopId, int ItemId, string Balance)
        {
            if (ShopId <= 0)
            {
                return BadRequest("Invalid Shop ID.");
            }

            if (ItemId <= 0)
            {
                return BadRequest("Invalid Item ID.");
            }

            if (string.IsNullOrWhiteSpace(Balance) || !decimal.TryParse(Balance, out decimal balanceValue))
            {
                return BadRequest("Balance must be a valid non-negative number.");
            }


            if (balanceValue < 0)
            {
                return BadRequest("Balance cannot be negative.");
            }


            var shopItem = new ShopItem
            {
                ShopId = ShopId,
                ItemId = ItemId,
                Balance = Balance,
                IsActive = true
            };

            _context.ShopItems.Add(shopItem);
            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> AddShopLocation(int ShopId, int RoomId)
        {
            if (ShopId <= 0)
            {
                return BadRequest("Invalid Shop ID.");
            }

            if (RoomId <= 0)
            {
                return BadRequest("Invalid Room ID.");
            }

            var shopLocation = new ShopLocation
            {
                ShopId = ShopId,
                RoomId = RoomId,
                CeratedDate = DateTime.Now,
                IsActive = true
            };

            _context.ShopLocations.Add(shopLocation);
            await _context.SaveChangesAsync();

            return Ok();
        }




        [HttpPost]
        public async Task<IActionResult> AddEntry(int ShopItemId, DateTime EntryDate, int Quantity, int? WithdrawQuantity, float Price)
        {


            if (EntryDate == default)
            {
                return BadRequest("Entry Date is required.");
            }


            if (Quantity < 0)
            {
                return BadRequest("Quantity cannot be negative.");
            }


            if (WithdrawQuantity.HasValue && WithdrawQuantity < 0)
            {
                return BadRequest("Withdraw Quantity cannot be negative.");
            }


            if (Price < 0)
            {
                return BadRequest("Price cannot be negative.");
            }

            var itemEntry = new ItemEntry
            {
                ShopItemId = ShopItemId,
                EntryDate = EntryDate,
                Quantity = Quantity,
                WithdrawQuantity = WithdrawQuantity,

                Price = Price,
                IsActive = true
            };

            _context.ItemEntries.Add(itemEntry);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteShopItems(int Id)
        {
            var item = await _context.ShopItems.FindAsync(Id);
            if (item == null) return NotFound();
            item.IsActive = false;
            item.IsDeleted = true;
            _context.ShopItems.Update(item);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteShopLocation(int Id)
        {
            var location = await _context.ShopLocations.FindAsync(Id);
            if (location == null) return NotFound();
            location.IsActive = false;
            location.IsDeleted = true;
            _context.ShopLocations.Update(location);
            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> EditItemEntry(int Id, int Quantity, float Price)
        {
            var itemEntry = await _context.ItemEntries.FindAsync(Id);
            if (itemEntry == null) return NotFound();
            itemEntry.Quantity = Quantity;
            itemEntry.Price = Price;
            _context.ItemEntries.Update(itemEntry);
            await _context.SaveChangesAsync();


            return Ok();
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

        public async Task<IActionResult> DeleteItemEntrys(int Id)
        {
            var item = await _context.ItemEntries.FindAsync(Id);
            if (item == null) return NotFound();
            item.IsActive = false;
            item.IsDeleted = true;
            _context.ItemEntries.Update(item);
            await _context.SaveChangesAsync();

            return Ok();
        }


        public async Task<IActionResult> DeleteItemEntryVaration(int Id)
        {
            var itementryvaration = await _context.ItemEntryVarations.FindAsync(Id);
            if (itementryvaration == null) return NotFound();
            itementryvaration.IsActive = false;
            itementryvaration.IsDeleted = true;
            _context.ItemEntryVarations.Update(itementryvaration);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddItemEntryVariation(int VarationTypeId, int VarationId, int ItemEntryId, float Quantity, float Price, int ShopItemId)
        {

            if (Quantity < 0)
            {
                return BadRequest("Quantity cannot be negative.");
            }

            if (Price < 0)
            {
                return BadRequest("Price cannot be negative.");
            }


            var itemEntryVaration = new ItemEntryVaration
            {
                ShopItemId = ShopItemId,
                ItemEntryId = ItemEntryId,
                VarationTypeId = VarationTypeId,
                VarationId = VarationId,
                Quantity = Quantity,
                Price = Price,
            };

            try
            {

                _context.ItemEntryVarations.Add(itemEntryVaration);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateException ex)
            {

                return BadRequest($"An error occurred while saving: {ex.InnerException?.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditItemEntryVariation(int Id, int VarationTypeId, int VarationId, float Quantity, float Price)
        {
            var itemVariation = await _context.ItemEntryVarations.FindAsync(Id);
            if (itemVariation == null) return NotFound();

            itemVariation.VarationTypeId = VarationTypeId;
            itemVariation.VarationId = VarationId;
            itemVariation.Quantity = Quantity;
            itemVariation.Price = Price;

            _context.ItemEntryVarations.Update(itemVariation);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> UploadbuildingImage(IFormFile image, int buildingId, int? buildingImageId = null)
        {
            string url = "";


            if (image != null)
            {
                url = await UploadImage(image, "images");

                if (url == "Invalid file type" || url == "File size exceeds limit")
                {
                    TempData["Error"] = url;
                    return Redirect(Request.GetTypedHeaders().Referer.ToString());
                }

                else
                {

                    var newBuildingImage = new BuildingImage { Url = url, BuildingId = buildingId, IsActive = true };
                    await _context.BuildingImages.AddAsync(newBuildingImage);
                }

                await _context.SaveChangesAsync();

            }
            else
            {
                TempData["Error"] = "No image provided.";
            }

            return Redirect(Request.GetTypedHeaders().Referer.ToString());
        }


        [HttpPost]
        public async Task<IActionResult> UploadItemImage(IFormFile image, int itemId, int? itemImageId = null)
        {
            string url = "";


            if (image != null)
            {
                url = await UploadImage(image, "images");

                if (url == "Invalid file type" || url == "File size exceeds limit")
                {
                    TempData["Error"] = url;
                    return Redirect(Request.GetTypedHeaders().Referer.ToString());
                }

                else
                {

                    var newItemImage = new ItemImage { Url = url, ItemId = itemId, IsActive = true };
                    await _context.ItemImages.AddAsync(newItemImage);
                }

                await _context.SaveChangesAsync();

            }
            else
            {
                TempData["Error"] = "No image provided.";
            }

            return Redirect(Request.GetTypedHeaders().Referer.ToString());
        }

        private async Task<string> UploadImage(IFormFile image, string dir)
        {
            string url = "";

            if (image.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return "Invalid file type";
                }

                if (image.Length > 5 * 1024 * 1024)
                {
                    return "File size exceeds limit";
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
                    await image.CopyToAsync(stream);
                }

                url = $"/{dir}/" + uniqueFileName;
            }
            return url;
        }


        [HttpPost]
        public async Task<IActionResult> DeleteBuildingImage(int id, int buildingId)
        {
            var image = await _context.BuildingImages.FindAsync(id);

            if (image == null)
            {
                TempData["Error"] = "Image not found.";
                return Redirect(Request.GetTypedHeaders().Referer.ToString());
            }

            
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.Url.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            
            _context.BuildingImages.Remove(image);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Image deleted successfully.";

            return Redirect(Request.GetTypedHeaders().Referer.ToString());
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

        [HttpPost]
        public async Task<IActionResult> CreateShopRequest(int Id, int RequestStatusId, string Description, int NumberOfShop)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var shopRequest = new ShopRequest
            {
                Id = Id,
                UserId = (int)userId,
                RequestStatusId = 2,
                Description = Description,
                NumberOfShops = NumberOfShop,
                RequestDate = DateTime.Now,
                IsActive = true
            };

            _context.ShopRequests.Add(shopRequest);
            await _context.SaveChangesAsync();

            
            TempData["success"] = "Shop Request Created successfully!";

            
            return Ok(new { success = true, message = "Shop Request Created successfully!" });
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


    }

}




