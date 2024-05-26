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
using Microsoft.Extensions.Logging;

namespace AsseyLabMgt.Controllers
{
    [Authorize]
    public class PlantSourcesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PlantSourcesController> _logger;

        public PlantSourcesController(ApplicationDbContext context, ILogger<PlantSourcesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: PlantSources
        public async Task<IActionResult> Index()
        {
            return View(await _context.PlantSources.ToListAsync());
        }

        // GET: PlantSources/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "PlantSource ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var plantSource = await _context.PlantSources
                .FirstOrDefaultAsync(m => m.Id == id);
            if (plantSource == null)
            {
                TempData["ErrorMessage"] = "PlantSource not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(plantSource);
        }

        // GET: PlantSources/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PlantSources/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlantSource plantSource)
        {
            plantSource.IsActive = true;
            plantSource.CreatedDate = DateTime.UtcNow;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(plantSource);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "PlantSource created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating PlantSource: {ErrorMessage}", ex.Message);
                    TempData["ErrorMessage"] = "An error occurred while creating the PlantSource.";
                }
            }
            return View(plantSource);
        }

        // GET: PlantSources/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "PlantSource ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var plantSource = await _context.PlantSources.FindAsync(id);
            if (plantSource == null)
            {
                TempData["ErrorMessage"] = "PlantSource not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(plantSource);
        }

        // POST: PlantSources/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PlantSource plantSource)
        {
            if (id != plantSource.Id)
            {
                TempData["ErrorMessage"] = "PlantSource ID mismatch.";
                return RedirectToAction(nameof(Index));
            }
            plantSource.UpdatedDate = DateTime.UtcNow;
            plantSource.CreatedDate = plantSource.CreatedDate.ToUniversalTime();
            plantSource.IsActive = true;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(plantSource);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "PlantSource updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlantSourceExists(plantSource.Id))
                    {
                        TempData["ErrorMessage"] = "PlantSource not found.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Concurrency error while updating PlantSource with ID {PlantSourceId}.", plantSource.Id);
                        TempData["ErrorMessage"] = "An error occurred while updating the PlantSource.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating PlantSource with ID {PlantSourceId}: {ErrorMessage}", plantSource.Id, ex.Message);
                    TempData["ErrorMessage"] = "An error occurred while updating the PlantSource: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid data submitted.";
            }

            return View(plantSource);
        }

        // GET: PlantSources/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "PlantSource ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var plantSource = await _context.PlantSources
                .FirstOrDefaultAsync(m => m.Id == id);
            if (plantSource == null)
            {
                TempData["ErrorMessage"] = "PlantSource not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(plantSource);
        }

        // POST: PlantSources/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var plantSource = await _context.PlantSources.FindAsync(id);
                if (plantSource != null)
                {
                    _context.PlantSources.Remove(plantSource);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "PlantSource deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "PlantSource not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting PlantSource with ID {PlantSourceId}: {ErrorMessage}", id, ex.Message);
                TempData["ErrorMessage"] = "An error occurred while deleting the PlantSource: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PlantSourceExists(int id)
        {
            return _context.PlantSources.Any(e => e.Id == id);
        }
    }
}
