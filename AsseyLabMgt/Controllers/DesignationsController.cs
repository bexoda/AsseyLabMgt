using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AsseyLabMgt.Data;
using AsseyLabMgt.Models;
using Microsoft.AspNetCore.Authorization;

namespace AsseyLabMgt.Controllers
{
    [Authorize]
    public class DesignationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DesignationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Designations
        public async Task<IActionResult> Index()
        {
            return View(await _context.Designation.ToListAsync());
        }

        // GET: Designations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Designation ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var designation = await _context.Designation
                .FirstOrDefaultAsync(m => m.Id == id);
            if (designation == null)
            {
                TempData["ErrorMessage"] = "Designation not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(designation);
        }

        // GET: Designations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Designations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Designation designation)
        {
            designation.IsActive = true;
            designation.CreatedDate = DateTime.UtcNow;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(designation);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Designation created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while creating the designation: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid data submitted.";
            }
            return View(designation);
        }

        // GET: Designations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Designation ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var designation = await _context.Designation.FindAsync(id);
            if (designation == null)
            {
                TempData["ErrorMessage"] = "Designation not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(designation);
        }

        // POST: Designations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Designation designation)
        {
            if (id != designation.Id)
            {
                TempData["ErrorMessage"] = "Designation ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            designation.UpdatedDate = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(designation);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Designation updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DesignationExists(designation.Id))
                    {
                        TempData["ErrorMessage"] = "Designation not found.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "An error occurred while updating the designation.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while updating the designation: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid data submitted.";
            }
            return View(designation);
        }

        // GET: Designations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Designation ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var designation = await _context.Designation
                .FirstOrDefaultAsync(m => m.Id == id);
            if (designation == null)
            {
                TempData["ErrorMessage"] = "Designation not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(designation);
        }

        // POST: Designations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var designation = await _context.Designation.FindAsync(id);
                if (designation != null)
                {
                    _context.Designation.Remove(designation);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Designation deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Designation not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the designation: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DesignationExists(int id)
        {
            return _context.Designation.Any(e => e.Id == id);
        }
    }
}
