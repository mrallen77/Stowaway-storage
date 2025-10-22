using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StowawayStorage.Data;
using StowawayStorage.Models;
using System.ComponentModel.DataAnnotations;

namespace StowawayStorage.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userMgr;

        public ReservationsController(ApplicationDbContext db, UserManager<IdentityUser> userMgr)
        {
            _db = db;
            _userMgr = userMgr;
        }

        // Alias to support /Reservations/My (matches your view name)
        public async Task<IActionResult> My() => await Mine();

        // Show user's own reservations
        public async Task<IActionResult> Mine()
        {
            var userId = _userMgr.GetUserId(User)!;
            var mine = await _db.Reservations
                .Include(r => r.Unit)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.StartDateUtc)
                .ToListAsync();
            return View("My", mine); // render the My.cshtml view
        }

        // GET: Create
        public async Task<IActionResult> Create(int? unitId)
        {
            await PopulateUnitsDropdown(unitId);
            return View(new ReservationCreateVm
            {
                UnitId = unitId,
                StartLocal = DateTime.Today.AddDays(1),
                EndLocal = DateTime.Today.AddDays(2)
            });
        }

        // POST: Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationCreateVm vm)
        {
            await PopulateUnitsDropdown(vm.UnitId);

            if (vm.StartLocal.Date < DateTime.Today)
                ModelState.AddModelError(nameof(vm.StartLocal), "Start date cannot be in the past.");

            if (vm.EndLocal <= vm.StartLocal)
                ModelState.AddModelError(nameof(vm.EndLocal), "End date must be after start date.");

            if (!ModelState.IsValid) return View(vm);

            var startUtc = DateTime.SpecifyKind(vm.StartLocal.Date, DateTimeKind.Local).ToUniversalTime();
            var endUtc = DateTime.SpecifyKind(vm.EndLocal.Date, DateTimeKind.Local).ToUniversalTime();

            var conflict = await _db.Reservations.AnyAsync(r =>
                r.UnitId == vm.UnitId &&
                r.StartDateUtc < endUtc &&
                r.EndDateUtc > startUtc
            );

            if (conflict)
            {
                ModelState.AddModelError(string.Empty, "That unit is already reserved for part of your selected dates. Try different dates or another unit.");
                return View(vm);
            }

            var userId = _userMgr.GetUserId(User)!;
            var reservation = new Reservation
            {
                UnitId = vm.UnitId!.Value,
                StartDateUtc = startUtc,
                EndDateUtc = endUtc,
                UserId = userId,
                Notes = vm.Notes
            };

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();

            TempData["Flash"] = "Reservation confirmed!";
            return RedirectToAction(nameof(My));
        }

        // Admin list of all reservations
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var all = await _db.Reservations
                .Include(r => r.Unit)
                .OrderByDescending(r => r.StartDateUtc)
                .ToListAsync();
            return View(all);
        }


        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userMgr.GetUserId(User)!;

            var res = await _db.Reservations
                .Include(r => r.Unit)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (res == null) return NotFound();

            var isOwner = res.UserId == userId;
            var isAdmin = User.IsInRole("Admin");
            if (!isOwner && !isAdmin) return Forbid();

            return View(res);
        }


        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userMgr.GetUserId(User)!;

            var res = await _db.Reservations
                .Include(r => r.Unit)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (res == null) return NotFound();

            var isOwner = res.UserId == userId;
            var isAdmin = User.IsInRole("Admin");
            if (!isOwner && !isAdmin) return Forbid();

            return View(res);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userMgr.GetUserId(User)!;

            var res = await _db.Reservations
                .FirstOrDefaultAsync(r => r.Id == id);

            if (res == null) return NotFound();

            var isOwner = res.UserId == userId;
            var isAdmin = User.IsInRole("Admin");
            if (!isOwner && !isAdmin) return Forbid();

            _db.Reservations.Remove(res);
            await _db.SaveChangesAsync();

            TempData["Flash"] = "Reservation canceled.";
            return RedirectToAction(nameof(My));
        }


        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userMgr.GetUserId(User)!;

            var res = await _db.Reservations
                .Include(r => r.Unit)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (res == null) return NotFound();

            var isOwner = res.UserId == userId;
            var isAdmin = User.IsInRole("Admin");
            if (!isOwner && !isAdmin) return Forbid();

            // Pre-fill VM using local dates for the date inputs
            var vm = new ReservationEditVm
            {
                Id = res.Id,
                UnitId = res.UnitId,
                StartLocal = res.StartDateUtc.ToLocalTime().Date,
                EndLocal = res.EndDateUtc.ToLocalTime().Date,
                Notes = res.Notes
            };

            await PopulateUnitsDropdown(vm.UnitId);
            return View(vm);
        }

        // POST: Reservations/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ReservationEditVm vm)
        {
            var userId = _userMgr.GetUserId(User)!;

            var res = await _db.Reservations.FirstOrDefaultAsync(r => r.Id == vm.Id);
            if (res == null) return NotFound();

            var isOwner = res.UserId == userId;
            var isAdmin = User.IsInRole("Admin");
            if (!isOwner && !isAdmin) return Forbid();

            await PopulateUnitsDropdown(vm.UnitId);

            if (vm.StartLocal.Date < DateTime.Today)
                ModelState.AddModelError(nameof(vm.StartLocal), "Start date cannot be in the past.");

            if (vm.EndLocal <= vm.StartLocal)
                ModelState.AddModelError(nameof(vm.EndLocal), "End date must be after start date.");

            if (!ModelState.IsValid) return View(vm);

            var startUtc = DateTime.SpecifyKind(vm.StartLocal.Date, DateTimeKind.Local).ToUniversalTime();
            var endUtc = DateTime.SpecifyKind(vm.EndLocal.Date, DateTimeKind.Local).ToUniversalTime();

            // Overlap check excluding current reservation
            var conflict = await _db.Reservations.AnyAsync(r =>
                r.UnitId == vm.UnitId &&
                r.Id != vm.Id &&
                r.StartDateUtc < endUtc &&
                r.EndDateUtc > startUtc
            );

            if (conflict)
            {
                ModelState.AddModelError(string.Empty, "That unit is already reserved for part of your selected dates.");
                return View(vm);
            }

            // Update and save
            res.UnitId = vm.UnitId!.Value;
            res.StartDateUtc = startUtc;
            res.EndDateUtc = endUtc;
            res.Notes = vm.Notes;

            await _db.SaveChangesAsync();

            TempData["Flash"] = "Reservation updated.";
            return RedirectToAction(nameof(My));
        }

        // ViewModel for edit form
        public class ReservationEditVm
        {
            public int Id { get; set; }
            [Required]
            public int? UnitId { get; set; }

            [Display(Name = "Start date"), DataType(DataType.Date)]
            public DateTime StartLocal { get; set; }

            [Display(Name = "End date"), DataType(DataType.Date)]
            public DateTime EndLocal { get; set; }

            [StringLength(240)]
            public string? Notes { get; set; }
        }


        private async Task PopulateUnitsDropdown(int? selected)
        {
            var units = await _db.StorageUnits
                .Where(u => u.IsActive)
                .OrderBy(u => u.MonthlyPrice)
                .Select(u => new { u.Id, Label = $"{u.Name} · {u.Size} · ${u.MonthlyPrice:n2}/mo" })
                .ToListAsync();

            ViewBag.UnitId = new SelectList(units, "Id", "Label", selected);
        }

        // ViewModel for create form (local date pickers)
        public class ReservationCreateVm
        {
            [Required]
            public int? UnitId { get; set; }

            [Display(Name = "Start date"), DataType(DataType.Date)]
            public DateTime StartLocal { get; set; }

            [Display(Name = "End date"), DataType(DataType.Date)]
            public DateTime EndLocal { get; set; }

            [StringLength(240)]
            public string? Notes { get; set; }
        }
    }
}
