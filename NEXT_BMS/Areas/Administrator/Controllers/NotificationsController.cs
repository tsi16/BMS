using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NEXT_BMS.Models;
using NEXT_BMS.Utilities;
using Microsoft.AspNetCore.SignalR;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    public class NotificationsController : Controller
    {
        private readonly NEXT_BMSContext _context;
        
        private CancellationToken message;

        public NotificationsController(NEXT_BMSContext context)
        {
            _context = context;
           
        }


        public IActionResult SendToAll()
        {
            ViewData["NotificationTypeId"] = new SelectList(_context.NotificationTypes, "Id", "Name");
            return View(); 
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToAll([Bind("Message,NotificationTypeId")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                
                var users = await _context.Users.Where(u => !u.IsDeleted).ToListAsync();

                foreach (var user in users)
                {
                    SaveNotification(notification.Message, notification.NotificationTypeId, user.Id);
                }

                ViewData["NotificationTypeId"] = new SelectList(_context.NotificationTypes, "Id", "Name");
               
                
                return Json(new { success = true, message = "Notification sent to all users!" });
            }
            return Json(new { success = false, message = "Failed to send notification." });
        

    

           
        }

        private void SaveNotification(string message,int notificationTypeId,  int userId )
        {
         var  notification = new Notification
            {
                UserId = userId,
                NotificationTypeId = notificationTypeId,
                NotificationStatusId = (int)notificationStatus.Pending,
                NotificationDate = DateOnly.FromDateTime(DateTime.Now),
                Message = message,
                IsActive = true,
                IsDeleted = false
            };
             _context.Notifications.Add(notification);
            _context.SaveChanges();
        }
             
        public void MakeAsRead(int id)
        {
            var notification =  _context.Notifications.FirstOrDefault(u => u.Id == id);
           
            notification.NotificationStatusId = (int)notificationStatus.Seen;
            _context.Notifications.Update(notification);
            _context.SaveChanges();
           
        }
        public IActionResult SendToUser()
        {
            ViewData["UserId"] = new SelectList(_context.Users.Where(u => !u.IsDeleted), "Id", "Email");
            ViewData["NotificationTypeId"] = new SelectList(_context.NotificationTypes, "Id", "Name");
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendToUser([Bind("Message,NotificationTypeId,UserId")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                SaveNotification(notification.Message, notification.NotificationTypeId, notification.UserId);

                return RedirectToAction("Index");
            }

            ViewData["UserId"] = new SelectList(_context.Users.Where(u => !u.IsDeleted), "Id", "Email", notification.UserId);
            ViewData["NotificationTypeId"] = new SelectList(_context.NotificationTypes, "Id", "Name", notification.NotificationTypeId);
            return View(notification);
        }



        public async Task<IActionResult> MyNotifications() 
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = HttpContext.Session.GetInt32("UserId");
            var notifications = await _context.Notifications
                .Include(n => n.NotificationStatus)
                .Include(n => n.NotificationType)
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.NotificationDate)
                .ToListAsync();

            return View(notifications);
        }



        public async Task<IActionResult> Index()
        {
            var bIMSContext = _context.Notifications.Include(n => n.NotificationStatus).Include(n => n.NotificationType).Include(n => n.User);
            return View(await bIMSContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.NotificationStatus)
                .Include(n => n.NotificationType)
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        public IActionResult Create()
        {
            ViewData["NotificationStatusId"] = new SelectList(_context.NotificationStatuses, "Id", "Name");
            ViewData["NotificationTypeId"] = new SelectList(_context.NotificationTypes, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,NotificationTypeId,NotificationStatusId,NotificationDate,IsActive,IsDeleted")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notification);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["NotificationStatusId"] = new SelectList(_context.NotificationStatuses, "Id", "Name", notification.NotificationStatusId);
            ViewData["NotificationTypeId"] = new SelectList(_context.NotificationTypes, "Id", "Name", notification.NotificationTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", notification.UserId);
            return View(notification);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            ViewData["NotificationStatusId"] = new SelectList(_context.NotificationStatuses, "Id", "Name", notification.NotificationStatusId);
            ViewData["NotificationTypeId"] = new SelectList(_context.NotificationTypes, "Id", "Name", notification.NotificationTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", notification.UserId);
            return View(notification);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,NotificationTypeId,NotificationStatusId,NotificationDate,IsActive,IsDeleted")] Notification notification)
        {
            if (id != notification.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationExists(notification.Id))
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
            ViewData["NotificationStatusId"] = new SelectList(_context.NotificationStatuses, "Id", "Name", notification.NotificationStatusId);
            ViewData["NotificationTypeId"] = new SelectList(_context.NotificationTypes, "Id", "Name", notification.NotificationTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", notification.UserId);
            return View(notification);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.NotificationStatus)
                .Include(n => n.NotificationType)
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

       
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.Id == id);
        }
    }
}
