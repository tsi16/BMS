using NEXT_BMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using DocumentFormat.OpenXml.Bibliography;


namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]

    public class TenantPaymentsController : Controller
    {
        private readonly NEXT_BMSContext _context;

        public TenantPaymentsController(NEXT_BMSContext context)
        {
            _context = context;
        }
     
        public IActionResult Index(int? id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var tenants = _context.Tenants
                .Include(x => x.TenantUsers)
                .Include(x => x.Building)
                .Include(x => x.TenantType).Where(x => x.TenantUsers.Any(a => a.UserId == userId));


            ViewData["BuildingId"] = new SelectList(_context.Buildings
                .Where(b => b.IsActive), "Id", "Name");


            ViewBag.SelectedBuildingId = id;

           
            ViewData["TenantTypeId"] = new SelectList(_context.TenantTypes
               .Where(b => b.IsActive), "Id", "Name");

            return View(tenants);
        }

        public IActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                return BadRequest();
            }

            var tenant = _context.Tenants
                .Include(x => x.Building)
                .Include(x => x.RoomRentals).ThenInclude(x => x.BusinessArea)
                .Include(x => x.RoomRentals).ThenInclude(x => x.Room)
                .Include(x => x.TenantType)
               .Include(x => x.RoomRentals).ThenInclude(x => x.RoomRentalPayments).ThenInclude(x => x.PaymentType)
                .Include(x => x.RoomRentals).ThenInclude(x => x.RoomRentalPayments).ThenInclude(x => x.PaymentMode)
                 .Include(x => x.RoomRentals).ThenInclude(x => x.RoomRentalPayments).ThenInclude(x => x.RoomRentalPaymentDetails).ThenInclude(x => x.Month)
                .Include(x => x.RoomRentals).ThenInclude(x => x.RoomRentalPayments).ThenInclude(x => x.RoomRentalPaymentDetails).ThenInclude(x => x.Year)

                 .FirstOrDefault(m => m.Id == id);

            if (tenant == null)
            {
                return NotFound();
            }

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


            ViewBag.PaymentTypeId = new SelectList(_context.PaymentTypes, "Id", "Name");
            ViewBag.PaymentModeId = new SelectList(_context.PaymentModes, "Id", "Name");
            ViewBag.MonthId = new SelectList(_context.Months, "Id", "Name");
            ViewBag.YearId = new SelectList(_context.Years, "Id", "Name");
            return View(tenant);
        }

        public IActionResult Payments()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPayment(int RoomRentalId, int PaymentTypeId, int PaymentModeId, float TotalAmount, string InvoiceNumber)
        {
            if (RoomRentalId <= 0 || PaymentTypeId <= 0 || PaymentModeId <= 0 || TotalAmount <= 0)
            {
                return BadRequest("Invalid data provided.");
            }

            var roomRentalPayment = new RoomRentalPayment
            {
                RoomRentalId = RoomRentalId,
                PaymentTypeId = PaymentTypeId,
                PaymentModeId = PaymentModeId,
                TotalAmount = TotalAmount,
                InvoiceNumber = InvoiceNumber,
                PaidDate = DateTime.Now,
                IsActive = true
            };

            _context.RoomRentalPayments.Add(roomRentalPayment);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddRoomRentalPayment(int RoomRentalPaymentId, int MonthId, int YearId, float TotalAmount)
        {

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return BadRequest("User must be logged in.");
            }


            var roomRentalPaymentDetails = new RoomRentalPaymentDetail
            {
                RoomRentalPaymentId = RoomRentalPaymentId,
                MonthId = MonthId,
                YearId = YearId,
                TotalAmount = TotalAmount,
                AccceptedBy = userId.Value,
                Date = DateTime.Now,
                IsActive = true,
            };

            _context.RoomRentalPaymentDetails.Add(roomRentalPaymentDetails);
            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpPost]

        public async Task<IActionResult> EditPayment(int Id, int PaymentTypeId, int PaymentModeId, float TotalAmount, string InvoiceNumber)
        {
            var roomRentalPayment = await _context.RoomRentalPayments.FindAsync(Id);
            if (roomRentalPayment == null) return NotFound();
            roomRentalPayment.PaymentTypeId = PaymentTypeId;
            roomRentalPayment.PaymentModeId = PaymentModeId;
            roomRentalPayment.TotalAmount = TotalAmount;
            roomRentalPayment.InvoiceNumber = InvoiceNumber;
            _context.RoomRentalPayments.Update(roomRentalPayment);
            await _context.SaveChangesAsync();


            return Ok();
        }
       
        [HttpPost]

        public async Task<IActionResult> EditPaymentdetail(int Id, int MonthId, int YearId, int PaymentTotalAmount)
        {
            var roomRentalPaymentDetail = await _context.RoomRentalPaymentDetails.FindAsync(Id);
            if (roomRentalPaymentDetail == null) return NotFound();
            roomRentalPaymentDetail.MonthId = MonthId;
            roomRentalPaymentDetail.YearId = YearId;
            roomRentalPaymentDetail.TotalAmount = PaymentTotalAmount;
            _context.RoomRentalPaymentDetails.Update(roomRentalPaymentDetail);
            await _context.SaveChangesAsync();


            return Ok();
        }

       
        public async Task<IActionResult> DeletePayments(int Id)
        {
            var rpayment = await _context.RoomRentalPayments.FindAsync(Id);
            if (rpayment == null) return NotFound();
            rpayment.IsActive = false;
            rpayment.IsDeleted = true;
            _context.RoomRentalPayments.Update(rpayment);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public async Task<IActionResult> DeletePaymentdetail(int Id)
        {
            var rpayment = await _context.RoomRentalPaymentDetails.FindAsync(Id);
            if (rpayment == null) return NotFound();
            rpayment.IsActive = false;
            rpayment.IsDeleted = true;
            _context.RoomRentalPaymentDetails.Update(rpayment);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }

}