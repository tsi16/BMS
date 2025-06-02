using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXT_BMS.Models;
using Microsoft.AspNetCore.SignalR;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    public class ChatsController : Controller
    {
        private readonly NEXT_BMSContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatsController(NEXT_BMSContext context, IHubContext<ChatHub> hubContext)
        { 
            _context = context;
            _hubContext = hubContext;
        }
        public JsonResult GetAllUsersWithChatSummary()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("userid");
                if (!userId.HasValue)
                {
                    return Json(new { success = false, error = "Session expired. Please log in again." });
                }

                var users = _context.Users
                    .Where(u => u.Id != userId) 
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        LastMessageDate = _context.Chats
                            .Where(c => (c.SenderId == userId && c.ReceiverId == u.Id) ||
                                        (c.SenderId == u.Id && c.ReceiverId == userId))
                            .OrderByDescending(c => c.Date)
                            .Select(c => c.Date)
                            .FirstOrDefault(),
                        UnreadMessagesCount = _context.Chats
                            .Count(c => c.SenderId == u.Id && c.ReceiverId == userId && c.ChatStatusId == 2) 
                    })
                    .OrderByDescending(u => u.LastMessageDate) 
                    .ToList();

                return Json(new { success = true, users });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user list with chat summary: {ex.Message}");
                return Json(new { success = false, error = "An error occurred while fetching users." });
            }
        }


        [HttpGet]
        public IActionResult GetUserChatHistory(int partnerId)
        {
            int? senderId = HttpContext.Session.GetInt32("userid");

            var partner = _context.Users.FirstOrDefault(x => x.Id == partnerId);

            string partnerName = partner.FirstName;

            try
            {
                
                if (!senderId.HasValue)
                {
                    return Json(new { success = false, error = "Session expired. Please log in again." });
                }
              
                
                var chatHistory = _context.Chats
                    .Include(c => c.Parent)
                    .Where(c =>
                        (c.SenderId == senderId && c.ReceiverId == partnerId) ||
                        (c.SenderId == partnerId && c.ReceiverId == senderId))
                    .OrderBy(c => c.Date)
                    .ToList();

                var messagesToUpdate = chatHistory
                    .Where(c => c.ReceiverId == senderId.Value && c.ChatStatusId == 2)
                    .ToList();

                foreach (var message in messagesToUpdate)
                {
                    message.ChatStatusId = 1; 
                }

                _context.SaveChanges();

                var result = chatHistory.Select(c => new
                {
                    c.Id,
                    c.Message,
                    c.Date,
                    IsSentByMe = c.SenderId == senderId
                }).ToList();
                
                return Json(new {  partnerName, senderId = senderId, success = true, chatHistory = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching chat history: {ex.Message}");
                return Json(new { success = false, error = "An error occurred while fetching chat history." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> UpdateMessage(int messageId, string message)
        {
            int? userId = HttpContext.Session.GetInt32("userid");
            
            try
            {
                Console.WriteLine($"Received messageId: {messageId}, message: {message}");

                var chatMessage = await _context.Chats.FindAsync(messageId);
                if (chatMessage == null)
                {
                    Console.WriteLine("Message not found in database.");
                    return Json(new { success = false, error = "Message not found." });
                }
                saveOldChat(chatMessage.Id,chatMessage.Message);

                chatMessage.Message = message;
                chatMessage.Date = DateOnly.FromDateTime(DateTime.Now);
                _context.Chats.Update(chatMessage);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating message: {ex.Message}");
                return Json(new { success = false, error = "An error occurred while updating the message." });
            }
        }
        private void saveOldChat(int chatId, string message)
        {
            var chatVersion = new ChatVersion
            {
               OldMessage = message,
                ChatId = chatId,
               Date = DateOnly.FromDateTime(DateTime.Now),
               IsActive= true,
               IsDeleted= false
               
            };

            _context.ChatVersions.Add(chatVersion);
            _context.SaveChanges();
        }

        [HttpPost]
        public async Task<JsonResult> SendMessage(int receiverId, string message)
        {
            try
            {
                int? senderId = HttpContext.Session.GetInt32("SenderId");
                if (!senderId.HasValue)
                {
                    return Json(new { success = false, error = "Session expired. Please log in again." });
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    return Json(new { success = false, error = "Message cannot be empty." });
                }

                var newChat = new Chat
                {
                    SenderId = senderId.Value,
                    ReceiverId = receiverId,
                    Message = message,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    IsActive = true,
                    IsDeleted = false,
                    ChatStatusId = 2
                };
                _context.Chats.Add(newChat);
                await _context.SaveChangesAsync();

                var hubContext = HttpContext.RequestServices.GetService<IHubContext<ChatHub>>();
                await hubContext.Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", new
                {
                    SenderId = senderId.Value,
                    Message = message,
                    Date = DateTime.Now
                });

                var unreadMessagesCount = _context.Chats
              .Where(c => c.ReceiverId == senderId.Value && c.ChatStatusId == 2 && c.IsActive && !c.IsDeleted)
               .Count();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                return Json(new { success = false, error = "An error occurred while sending the message." });
            }
        }


        [HttpPost]
        public async Task<JsonResult> DeleteMessage(int messageId)
        {
            try
            {
                var chatMessage = await _context.Chats.FindAsync(messageId);
                if (chatMessage == null)
                {
                    return Json(new { success = false, error = "Message not found." });
                }

                _context.Chats.Remove(chatMessage);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting message: {ex.Message}");
                return Json(new { success = false, error = "An error occurred while deleting the message." });
            }
        }


        [HttpPost]
        public async Task<JsonResult> RealTimeTest(int receiverId,string message)
        {
            try
            {

                await _hubContext.Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", message);
                return Json(new { success = true, message = "Message send successfully!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RealTimeTest: {ex.Message}");
                return Json(new { success = false, error = "An error occurred while broadcasting the message." });
            }
        }
    
        public ActionResult Index(int receiverId, int? parentId)
        {
            var senderId = 1;  
            var chats = _context.Chats
                .Include(c => c.Sender)       
                .Include(c => c.Receiver)    
                .Include(c => c.Parent)       
                .Where(c => (c.SenderId == senderId && c.ReceiverId == receiverId) ||
                            (c.SenderId == receiverId && c.ReceiverId == senderId))
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.Date)
                .ToList();

            ViewBag.ParentId = parentId;
            return View(chats);
        }

        private bool ChatExists(int id)
        {
            return _context.Chats.Any(e => e.Id == id);
        }
    }
}
