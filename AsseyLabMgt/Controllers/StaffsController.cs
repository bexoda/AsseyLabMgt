using AsseyLabMgt.Data;
using AsseyLabMgt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AsseyLabMgt.Controllers
{
    [Authorize]
    public class StaffsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StaffsController> _logger;

        public StaffsController(ApplicationDbContext context, ILogger<StaffsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Staffs
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Staff.Include(s => s.Department).Include(s => s.Designation);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Staffs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Staff ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var staff = await _context.Staff
                .Include(s => s.Department)
                .Include(s => s.Designation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (staff == null)
            {
                TempData["ErrorMessage"] = "Staff not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(staff);
        }

        // GET: Staffs/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "DeptCode");
            ViewData["DesignationId"] = new SelectList(_context.Designation, "Id", "DesignationName");
            return View();
        }

        // POST: Staffs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Staff staff)
        {
            if (!string.IsNullOrEmpty(staff.Firstname) && !string.IsNullOrEmpty(staff.Surname))
            {
                staff.Fullname = $"{staff.Firstname} {staff.Surname}";
            }

            if (string.IsNullOrEmpty(staff.Fullname))
            {
                ModelState.AddModelError("FullName", "Full name cannot be empty.");
            }

            staff.CreatedDate = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(staff);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Staff created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating staff: {ErrorMessage}", ex.Message);
                    TempData["ErrorMessage"] = "An error occurred while creating the staff. Please try again.";
                }
            }

            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "DeptCode", staff.DepartmentId);
            ViewData["DesignationId"] = new SelectList(_context.Designation, "Id", "DesignationName", staff.DesignationId);
            return View(staff);
        }

        // GET: Staffs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Staff ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var staff = await _context.Staff.FindAsync(id);
            if (staff == null)
            {
                TempData["ErrorMessage"] = "Staff not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "DeptCode", staff.DepartmentId);
            ViewData["DesignationId"] = new SelectList(_context.Designation, "Id", "DesignationName", staff.DesignationId);
            return View(staff);
        }

        // POST: Staffs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Staff staff)
        {
            if (id != staff.Id)
            {
                TempData["ErrorMessage"] = "Staff ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            staff.UpdatedDate = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(staff);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Staff updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StaffExists(staff.Id))
                    {
                        TempData["ErrorMessage"] = "Staff not found.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Concurrency error while updating staff with ID {StaffId}.", staff.Id);
                        TempData["ErrorMessage"] = "An error occurred while updating the staff.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating staff with ID {StaffId}: {ErrorMessage}", staff.Id, ex.Message);
                    TempData["ErrorMessage"] = "An error occurred while updating the staff. Please try again.";
                }
            }

            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "DeptCode", staff.DepartmentId);
            ViewData["DesignationId"] = new SelectList(_context.Designation, "Id", "DesignationName", staff.DesignationId);
            return View(staff);
        }

        // GET: Staffs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Staff ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var staff = await _context.Staff
                .Include(s => s.Department)
                .Include(s => s.Designation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (staff == null)
            {
                TempData["ErrorMessage"] = "Staff not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(staff);
        }

        // POST: Staffs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(id);
                if (staff != null)
                {
                    _context.Staff.Remove(staff);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Staff deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Staff not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff with ID {StaffId}: {ErrorMessage}", id, ex.Message);
                TempData["ErrorMessage"] = "An error occurred while deleting the staff. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StaffExists(int id)
        {
            return _context.Staff.Any(e => e.Id == id);
        }
    }
}
