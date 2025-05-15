using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NEXT_BMS.Models;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace NEXT_BMS.Areas.Administrator.Controllers
{
    [Area("Administrator")]

    public class RoomPropertyTypesController : Controller
    {
        private readonly NEXT_BMSContext _context;

        public RoomPropertyTypesController(NEXT_BMSContext context)
        {
            _context = context;
        }

       
        public async Task<IActionResult> Index()
        {
            return View(await _context.RoomPropertyTypes.ToListAsync());
        }

       
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomPropertyType = await _context.RoomPropertyTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomPropertyType == null)
            {
                return NotFound();
            }

            return View(roomPropertyType);
        }

        
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,IsActive,IsDeleted")] RoomPropertyType roomPropertyType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(roomPropertyType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(roomPropertyType);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomPropertyType = await _context.RoomPropertyTypes.FindAsync(id);
            if (roomPropertyType == null)
            {
                return NotFound();
            }
            return View(roomPropertyType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsActive,IsDeleted")] RoomPropertyType roomPropertyType)
        {
            if (id != roomPropertyType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(roomPropertyType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomPropertyTypeExists(roomPropertyType.Id))
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
            return View(roomPropertyType);
        }

       
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomPropertyType = await _context.RoomPropertyTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomPropertyType == null)
            {
                return NotFound();
            }

            return View(roomPropertyType);
        }

       
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roomPropertyType = await _context.RoomPropertyTypes.FindAsync(id);
            if (roomPropertyType != null)
            {
                _context.RoomPropertyTypes.Remove(roomPropertyType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomPropertyTypeExists(int id)
        {
            return _context.RoomPropertyTypes.Any(e => e.Id == id);
        }
    }
}
