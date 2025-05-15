using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NEXT_BMS.Models;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    public class RoomsController : Controller
    {
        private readonly NEXT_BMSContext _context;

        public RoomsController(NEXT_BMSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var bIMSContext = _context.Rooms
                .Include(r => r.Floor).ThenInclude(x=>x.Building).ThenInclude(x => x.UseType)
                .Include(r => r.RoomStatus).Include(r => r.User);
            var model = new Room
            {
                RoomProperties = _context.RoomProperties.Include(r => r.RoomPropertyType).ToList(),
               
        };
            return View(await bIMSContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Floor)
                .Include(r => r.RoomStatus)
                .Include(r => r.User)
                 .Include(r => r.RoomProperties).ThenInclude(r=> r.RoomPropertyType)
                  
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return NotFound();
            }
            ViewData["RoomPropertyTypeId"] = new SelectList(_context.RoomPropertyTypes, "Id", "Name");
            return View(room);
        }

        public IActionResult Create()
        {
            ViewData["FloorId"] = new SelectList(_context.Floors, "Id", "Name");
            ViewData["RoomStatusId"] = new SelectList(_context.RoomStatues, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,FloorId,UserId,RoomStatusId,SizeInm2,Width,Length,Description,IsActive,IsDeleted")] Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FloorId"] = new SelectList(_context.Floors, "Id", "Name", room.FloorId);
            ViewData["RoomStatusId"] = new SelectList(_context.RoomStatues, "Id", "Name", room.RoomStatusId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", room.UserId);
            return View(room);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            ViewData["FloorId"] = new SelectList(_context.Floors, "Id", "Name", room.FloorId);
            ViewData["RoomStatusId"] = new SelectList(_context.RoomStatues, "Id", "Name", room.RoomStatusId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", room.UserId);
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,FloorId,UserId,RoomStatusId,SizeInm2,Width,Length,Description,IsActive,IsDeleted")] Room room)
        {
            if (id != room.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
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
            ViewData["FloorId"] = new SelectList(_context.Floors, "Id", "Name", room.FloorId);
            ViewData["RoomStatusId"] = new SelectList(_context.RoomStatues, "Id", "Name", room.RoomStatusId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", room.UserId);
            return View(room);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Floor)
                .Include(r => r.RoomStatus)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int PropertyTypeId, string Value, int roomId)
        {
            
            if (PropertyTypeId <= 0)
            {
                return BadRequest("Invalid Property Type ID.");
            }

            if (string.IsNullOrWhiteSpace(Value))
            {
                return BadRequest("Property Value cannot be empty.");
            }

            var property = new RoomProperty
            {
                RoomId = roomId,
                RoomPropertyTypeId = PropertyTypeId,
                Value = Value
            };
            _context.RoomProperties.Add(property);
            await _context.SaveChangesAsync();
            return Ok();
          
        }
        [HttpPost]
        public async Task<IActionResult> EditRoomProperty(int Id, string Value)
        {
            var property = await _context.RoomProperties.FindAsync(Id);
            if (property == null) return NotFound();

            property.Value = Value;
            _context.RoomProperties.Update(property);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRoomProperty(int Id)
        {
            var property = await _context.RoomProperties.FindAsync(Id);
            if (property == null) return NotFound();
            property.IsActive = false;
            property.IsDeleted = true;
            _context.RoomProperties.Update(property);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }



}






