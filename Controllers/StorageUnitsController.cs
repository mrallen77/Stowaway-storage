using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StowawayStorage.Data;
using StowawayStorage.Models;

namespace StowawayStorage.Controllers
{
    public class StorageUnitsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public StorageUnitsController(ApplicationDbContext db) => _db = db;

        // Browse units (public)
        public async Task<IActionResult> Index()
        {
            var units = await _db.StorageUnits
                .OrderBy(u => u.MonthlyPrice)
                .ToListAsync();
            return View(units);
        }

        // ----- Create (Admin) -----
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View(new StorageUnit());

        [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StorageUnit unit)
        {
            if (!ModelState.IsValid) return View(unit);
            _db.StorageUnits.Add(unit);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ----- Delete (Admin) -----
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var unit = await _db.StorageUnits.FindAsync(id);
            if (unit == null) return NotFound();
            return View(unit);
        }

        [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unit = await _db.StorageUnits.FindAsync(id);
            if (unit == null) return NotFound();

            _db.StorageUnits.Remove(unit);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // ----- Edit (Admin) -----
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var unit = await _db.StorageUnits.FindAsync(id);
            if (unit == null) return NotFound();
            return View(unit);
        }

        [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StorageUnit unit)
        {
            if (id != unit.Id) return BadRequest();
            if (!ModelState.IsValid) return View(unit);

            _db.Entry(unit).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Optional details (Admin or public—adjust as you like)
        public async Task<IActionResult> Details(int id)
        {
            var unit = await _db.StorageUnits
                .Include(u => u.Reservations)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (unit == null) return NotFound();
            return View(unit);
        }
    }
}
