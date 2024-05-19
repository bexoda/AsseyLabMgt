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
    public class PlantSourcesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlantSourcesController(ApplicationDbContext context)
        {
            _context = context;
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
                return NotFound();
            }

            var plantSource = await _context.PlantSources
                .FirstOrDefaultAsync(m => m.Id == id);
            if (plantSource == null)
            {
                return NotFound();
            }

            return View(plantSource);
        }

        // GET: PlantSources/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PlantSources/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( PlantSource plantSource)
        {
            plantSource.IsActive=true;
            plantSource.CreatedDate = DateTime.UtcNow;
            if (ModelState.IsValid)
            {
                _context.Add(plantSource);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(plantSource);
        }

        // GET: PlantSources/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plantSource = await _context.PlantSources.FindAsync(id);
            if (plantSource == null)
            {
                return NotFound();
            }
            return View(plantSource);
        }

        // POST: PlantSources/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,PlantSource plantSource)
        {
            if (id != plantSource.Id)
            {
                return NotFound();
            }
            plantSource.UpdatedDate=DateTime.UtcNow;
            plantSource.CreatedDate=plantSource.CreatedDate.ToUniversalTime();
            plantSource.IsActive = true;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(plantSource);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlantSourceExists(plantSource.Id))
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
            return View(plantSource);
        }

        // GET: PlantSources/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plantSource = await _context.PlantSources
                .FirstOrDefaultAsync(m => m.Id == id);
            if (plantSource == null)
            {
                return NotFound();
            }

            return View(plantSource);
        }

        // POST: PlantSources/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plantSource = await _context.PlantSources.FindAsync(id);
            if (plantSource != null)
            {
                _context.PlantSources.Remove(plantSource);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlantSourceExists(int id)
        {
            return _context.PlantSources.Any(e => e.Id == id);
        }
    }
}
