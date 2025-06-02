using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NEXT_BMS.Models;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Net;


namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
       
        private readonly NEXT_BMSContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;


        public ProfileController(NEXT_BMSContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
           
            _webHostEnvironment = webHostEnvironment;
        }

        
        public IActionResult Index()
        {
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            var user = _context.Users
                .Include(x => x.Notifications)
                .Include(x => x.UserImages)
                 .Include(x => x.Gender)
                .Include(x => x.Notifications).ThenInclude(x => x.NotificationStatus)
                 .Include(x => x.Notifications).ThenInclude(x => x.NotificationType)
                .Include(u => u.OwnerUsers).ThenInclude(u => u.Owner).ThenInclude(u => u.OwnershipType)
               .Include(u => u.Buildings).ThenInclude(u => u.BuildingType)
               .Include(u => u.Buildings).ThenInclude(u => u.UseType)
               .Include(u => u.Buildings).ThenInclude(u => u.BuildingEmployees)
               .Include(u => u.Buildings).ThenInclude(u => u.Floors)
               .ThenInclude(u => u.Rooms)
               .Include(u => u.OwnerUsers).ThenInclude(u => u.Owner).ThenInclude(u => u.OwnershipType)
                .FirstOrDefault(x => x.Id == userId);
            var building = new Building();
            ViewData["BuildingTypeId"] = new SelectList(_context.BuildingTypes, "Id", "Name", building.BuildingTypeId);
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", building.CityId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", building.LocationId);
            ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "FullName", building.OwnerId);
            ViewData["OwnerShipTypeId"] = new SelectList(_context.OwnershipTypes, "Id", "Name", building.OwnershipTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FirstName", building.UserId);
            ViewData["UseTypeId"] = new SelectList(_context.UseTypes, "Id", "Name", building.UseTypeId);

            ViewData["OwnerShipTypeId"] = new SelectList(_context.OwnershipTypes, "Id", "Name");
            return View(user);
           
        }

      
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users

                   .Include(u => u.OwnerUsers).ThenInclude(u => u.Owner).ThenInclude(u => u.OwnershipType)
                   .Include(u => u.Buildings).ThenInclude(u => u.BuildingType)
                   .Include(u => u.Buildings).ThenInclude(u => u.BuildingEmployees)
                   .Include(u => u.Buildings).ThenInclude(u => u.Floors).ThenInclude(u => u.Rooms)
                   .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


       
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,IsActive,IsDeleted")] UseType useType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(useType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(useType);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var useType = await _context.UseTypes.FindAsync(id);
            if (useType == null)
            {
                return NotFound();
            }
            return View(useType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsActive,IsDeleted")] UseType useType)
        {
            if (id != useType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(useType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UseTypeExists(useType.Id))
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
            return View(useType);
        }

       
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var useType = await _context.UseTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (useType == null)
            {
                return NotFound();
            }

            return View(useType);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var useType = await _context.UseTypes.FindAsync(id);
            if (useType != null)
            {
                _context.UseTypes.Remove(useType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UseTypeExists(int id)
        {
            return _context.UseTypes.Any(e => e.Id == id);
        }

        [HttpPost]



        public IActionResult Upload(string imagedescription, IFormFile image)
        {
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            if (image == null || image.Length == 0)
            {
                return BadRequest(new { message = "No file selected for upload." });
            }


            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            var filePath = Path.Combine(uploadFolder, image.FileName);
            var url = "/images/" + image.FileName;

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }


            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(stream);
            }

            var userImage = new UserImage()
            {
                UserId = userId,
                ImageDescription = imagedescription,
                Url = url,
                IsActive = true
            };


            _context.UserImages.Add(userImage);
            _context.SaveChanges();
            TempData["Success"] = "Image Saved successfully.";


            return Redirect(Request.GetTypedHeaders().Referer.ToString());
        }
      



        [HttpPost]
        public async Task<IActionResult> EditProfile(string imagedescription, IFormFile image)
        {
           
            int userId = (int)HttpContext.Session.GetInt32("UserId");

          
            var userImage = await _context.UserImages
                .FirstOrDefaultAsync(img => img.UserId == userId && img.IsActive);

            if (userImage == null)
            {
                return NotFound(new { message = "User image not found." });
            }

            
            if (image != null && image.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { message = "Invalid file type. Only images are allowed." });
                }

                if (image.Length > 5 * 1024 * 1024) 
                {
                    return BadRequest(new { message = "File size exceeds limit. Maximum allowed size is 5 MB." });
                }

                
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                
                var oldImagePath = Path.Combine(uploadFolder, Path.GetFileName(userImage.Url));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }

               
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                
                userImage.ImageDescription = imagedescription;
                userImage.Url = "/images/" + uniqueFileName; 
            }
            else
            {
                userImage.ImageDescription = imagedescription;
            }

           
            _context.UserImages.Update(userImage);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully.";
            return Redirect(Request.GetTypedHeaders().Referer.ToString());
        }

        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
      
        public async Task<IActionResult> VerifyEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Invalid email. Please try again.");
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email. Please try again.");
                return View();
            }

            ViewBag.Message = "Success! You can now reset your password.";

            var verificationCode = new Random().Next(100000, 999999).ToString();

           
            await SendPasswordResetEmail(email, verificationCode);

           
            var passwordResetCode = new PasswordResetCode
            {
                Code = verificationCode,
                UserId = user.Id,
                Email = email, 
                IsActive = true,
                IsVerifayed = false,
                IsDeleted = false,
                SentDate = DateTime.Now,
            };

            await _context.PasswordResetCodes.AddAsync(passwordResetCode);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("VerificationCode", verificationCode);
            HttpContext.Session.SetString("UserEmail", email);
            TempData["rt"] = "Verfication code sent successfully";

            return RedirectToAction("ResetPassword");
        }

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string email, string newPassword, string confirmPassword, string verificationCode)
        {
            int userId = CheckVerficationCode(email, verificationCode);
            if (userId != 0)
            {
                if (newPassword == confirmPassword)
                {
                    ChangePassword(userId, newPassword);
                    TempData["success"] = "Password changed succssefully.";
                    return RedirectToAction("login", "Account");
                }
                else
                {
                    TempData["error"] = "Passwords do not match.";
                }
            }
            else
            {
                TempData["error"] = "Invalid verification code.";
            }

            return View();
        }

        public int CheckVerficationCode(string email, string verificationCode)
        {
            var passwordResetCode = _context.PasswordResetCodes
                .FirstOrDefault(x => x.Email == email && x.Code == verificationCode);

            if (passwordResetCode != null)
            {
                return passwordResetCode.UserId; 
            }
            return 0; 
        }

        private void ChangePassword(int userId, string newPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user != null)
            {
                user.Password = newPassword;
                _context.Users.Update(user);
                _context.SaveChanges();
            }
        }

        public async Task SendPasswordResetEmail(string email, string verificationCode)
        {
            var resetPasswordLink = "http://localhost:5151/Profile/ResetPassword";
              
            var message = $"<p>Your verification code is: <strong>{verificationCode}</strong>.</p>" +
                  $"<p>Click <a href='{resetPasswordLink}'>here</a> to reset your password.</p>";


            await SendEmailAsync(email, message);
        }

        private async Task SendEmailAsync(string email, string message)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("beayerlay@gmail.com"), 
                Subject = "Password Reset Verification",
                Body = message, 
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);

            using (var smtpClient = new SmtpClient("smtp.gmail.com"))
            {
                smtpClient.Port = 587;
                smtpClient.Credentials = new NetworkCredential("beayerlay@gmail.com", "pxurvzigjdyidfbw");
                smtpClient.EnableSsl = true;
                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (SmtpException smtpEx)
                {
                    throw new InvalidOperationException("SMTP error occurred: " + smtpEx.Message, smtpEx);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Error sending email", ex);
                }
            }
        }



       

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        public IActionResult Notifications(int? id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }


            var notifications = _context.Notifications
                 .Include(x => x.NotificationStatus)
                 .Include(x => x.NotificationType)
                  .Include(x => x.User)
                .Where(n => n.UserId == userId.Value)
                .ToList();

            return View(notifications);
        }

      


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOwner(int OwnershipTypeId, string fullName, int Tin, int? ownerid)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                TempData["error"] = "User is not authenticated.";
                return RedirectToAction(nameof(Index));
            }

            if (_context.Owners.Any(f => f.Tin == Tin || f.FullName.ToLower().Trim() == fullName.ToLower().Trim()))
            {
                TempData["error"] = "This owner already exists with this Tin number please change Tin Number";
                return RedirectToAction(nameof(Index));
            }
            var owner = new Owner
            {
                OwnershipTypeId = OwnershipTypeId,
                
                FullName = fullName,
                DocumentId = 1,
                Tin = Tin,
                Verified = true,
                IsActive = true,
                IsDeleted = false,

            };

            _context.Owners.Add(owner);
            await _context.SaveChangesAsync();
            saveOwnerUser(userId, owner.Id);
            return RedirectToAction("Index");
        }

        private void saveOwnerUser(int? UserId, int ownerid)
        {
            if (!_context.OwnerUsers.Any(f => f.Id == ownerid && f.UserId == UserId))
            {
                var ownerUser = new OwnerUser
                {
                    UserId = UserId.Value,
                    OwnerId = ownerid,
                    IsActive = true,
                    IsDeleted = false

                };
                _context.OwnerUsers.Add(ownerUser);
                _context.SaveChanges();
            }
        }

    }
}








