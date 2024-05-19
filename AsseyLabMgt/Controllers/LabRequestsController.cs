﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AsseyLabMgt.Data;
using AsseyLabMgt.Models;
using ClosedXML.Excel;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace AsseyLabMgt.Controllers
{
    [Authorize]
    public class LabRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LabRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LabRequests
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.LabRequests
                .Include(l => l.Client)
                .Include(l => l.DeliveredBy)
                .Include(l => l.Department)
                .Include(l => l.DigestedBy)
                .Include(l => l.EnteredBy)
                .Include(l => l.PlantSource)
                .Include(l => l.PreparedBy)
                .Include(l => l.ReceivedBy)
                .Include(l => l.TitratedBy)
                .Include(l => l.WeighedBy);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: LabRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labRequest = await _context.LabRequests
                .Include(l => l.Client)
                .Include(l => l.DeliveredBy)
                .Include(l => l.Department)
                .Include(l => l.DigestedBy)
                .Include(l => l.EnteredBy)
                .Include(l => l.PlantSource)
                .Include(l => l.PreparedBy)
                .Include(l => l.ReceivedBy)
                .Include(l => l.TitratedBy)
                .Include(l => l.WeighedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (labRequest == null)
            {
                return NotFound();
            }

            return View(labRequest);
        }

        // GET: LabRequests/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "ClientCode");
            ViewData["DeliveredById"] = new SelectList(_context.Staff, "Id", "Fullname");
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "DeptCode");
            ViewData["DigestedById"] = new SelectList(_context.Staff, "Id", "Fullname");
            ViewData["EnteredById"] = new SelectList(_context.Staff, "Id", "Fullname");
            ViewData["PlantSourceId"] = new SelectList(_context.PlantSources, "Id", "PlantSourceName");
            ViewData["PreparedById"] = new SelectList(_context.Staff, "Id", "Fullname");
            ViewData["ReceivedById"] = new SelectList(_context.Staff, "Id", "Fullname");
            ViewData["TitratedById"] = new SelectList(_context.Staff, "Id", "Fullname");
            ViewData["WeighedById"] = new SelectList(_context.Staff, "Id", "Fullname");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LabRequest labRequest, IFormFile excelFile)
        {
            labRequest.CreatedDate = DateTime.UtcNow;
            labRequest.ProductionDate = labRequest.ProductionDate.ToUniversalTime();
            labRequest.DateReported = labRequest.DateReported.ToUniversalTime();
            labRequest.ProductionDate = labRequest.ProductionDate.ToUniversalTime();
            labRequest.Date = labRequest.Date.ToUniversalTime();
            labRequest.IsActive = true;

            // Generate JobNumber as a long integer from DateTime
            long jobNumber = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss"));
            labRequest.JobNumber = jobNumber.ToString();  // Take last 9 digits to fit in Int32, if necessary

            if (excelFile != null && excelFile.Length > 0)
            {
                try
                {
                    var labResults = await ProcessExcelFileAsync(excelFile, labRequest);
                    HttpContext.Session.SetString("LabResults", JsonConvert.SerializeObject(labResults)); // Store the results temporarily
                    HttpContext.Session.SetString("LabRequest", JsonConvert.SerializeObject(labRequest)); // Store the LabRequest temporarily
                    return RedirectToAction("Confirm"); // Redirect to a confirmation view
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error processing file: " + ex.Message);
                }
            }

            PopulateViewData(labRequest);
            return View(labRequest);
        }

        private void PopulateViewData(LabRequest labRequest)
        {
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "ClientCode", labRequest.ClientId);
            ViewData["DeliveredById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest.DeliveredById);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "DeptCode", labRequest.DepartmentId);
            ViewData["DigestedById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest.DigestedById);
            ViewData["EnteredById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest.EnteredById);
            ViewData["PlantSourceId"] = new SelectList(_context.PlantSources, "Id", "PlantName", labRequest.PlantSourceId);
            ViewData["PreparedById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest.PreparedById);
            ViewData["ReceivedById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest.ReceivedById);
            ViewData["TitratedById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest.TitratedById);
            ViewData["WeighedById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest.WeighedById);
        }

        private async Task<List<LabResults>> ProcessExcelFileAsync(IFormFile excelFile, LabRequest labRequest)
        {
            var results = new List<LabResults>();
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                        throw new InvalidOperationException("No worksheet found");

                    int row = 2; // Assuming the first row is headers
                    while (!worksheet.Row(row).Cell(1).IsEmpty())
                    {
                        var labResult = new LabResults
                        {
                            LabRequestId = labRequest.Id, // This will be set after saving LabRequest
                            // This will be set after saving LabRequest
                            SampleId = worksheet.Row(row).Cell(1).GetValue<string>(),
                            Mn = worksheet.Row(row).Cell(2).GetValue<decimal>(),
                            Sol_Mn = worksheet.Row(row).Cell(3).GetValue<decimal>(),
                            Fe = worksheet.Row(row).Cell(4).GetValue<decimal>(),
                            B = worksheet.Row(row).Cell(5).GetValue<decimal>(),
                            MnO2 = worksheet.Row(row).Cell(6).GetValue<decimal>(),
                            SiO2 = worksheet.Row(row).Cell(7).GetValue<decimal>(),
                            Al2O3 = worksheet.Row(row).Cell(8).GetValue<decimal>(),
                            P = worksheet.Row(row).Cell(9).GetValue<decimal>(),
                            MgO = worksheet.Row(row).Cell(10).GetValue<decimal>(),
                            CaO = worksheet.Row(row).Cell(11).GetValue<decimal>(),
                            Au = worksheet.Row(row).Cell(12).GetValue<decimal>(),
                            As = worksheet.Row(row).Cell(13).GetValue<decimal>(),
                            H2O = worksheet.Row(row).Cell(14).GetValue<decimal>(),
                            Mg = worksheet.Row(row).Cell(15).GetValue<decimal>(),
                            Time = TimeOnly.FromDateTime(worksheet.Row(row).Cell(16).GetValue<DateTime>()),

                            CreatedDate = DateTime.UtcNow,
                            IsActive = true
                        };

                        results.Add(labResult);
                        row++;
                    }
                }
            }
            return results;
        }

        public IActionResult Confirm()
        {
            var labResults = JsonConvert.DeserializeObject<List<LabResults>>(HttpContext.Session.GetString("LabResults"));
            var labRequest = JsonConvert.DeserializeObject<LabRequest>(HttpContext.Session.GetString("LabRequest"));

            // Load related data
            _context.Entry(labRequest).Reference(l => l.Client).Load();
            _context.Entry(labRequest).Reference(l => l.Department).Load();
            _context.Entry(labRequest).Reference(l => l.PlantSource).Load();
            _context.Entry(labRequest).Reference(l => l.DeliveredBy).Load();
            _context.Entry(labRequest).Reference(l => l.ReceivedBy).Load();
            _context.Entry(labRequest).Reference(l => l.DigestedBy).Load();
            _context.Entry(labRequest).Reference(l => l.EnteredBy).Load();
            _context.Entry(labRequest).Reference(l => l.PreparedBy).Load();
            _context.Entry(labRequest).Reference(l => l.TitratedBy).Load();
            _context.Entry(labRequest).Reference(l => l.WeighedBy).Load();

            var viewModel = new ConfirmationViewModel
            {
                LabRequest = labRequest,
                LabResults = labResults
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeConfirmation()
        {
            var labResults = JsonConvert.DeserializeObject<List<LabResults>>(HttpContext.Session.GetString("LabResults"));
            var labRequest = JsonConvert.DeserializeObject<LabRequest>(HttpContext.Session.GetString("LabRequest"));

            _context.Add(labRequest);
            await _context.SaveChangesAsync();

            foreach (var result in labResults)
            {
                result.LabRequestId = labRequest.Id;
                _context.LabResults.Add(result);
            }
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("LabResults");
            HttpContext.Session.Remove("LabRequest");

            return RedirectToAction(nameof(Index));
        }

        // GET: LabRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labRequest = await _context.LabRequests.FindAsync(id);
            if (labRequest == null)
            {
                return NotFound();
            }

            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "ClientCode", labRequest.ClientId);
            ViewData["DeliveredById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest.DeliveredById);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "DeptCode", labRequest.DepartmentId);
            ViewData["DigestedById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest.DigestedById);
            ViewData["EnteredById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest.EnteredById);
            ViewData["PlantSourceId"] = new SelectList(_context.PlantSources, "Id", "PlantName", labRequest.PlantSourceId);
            ViewData["PreparedById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest.PreparedById);
            ViewData["ReceivedById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest.ReceivedById);
            ViewData["TitratedById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest.TitratedById);
            ViewData["WeighedById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest.WeighedById);

            return View(labRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LabRequest labRequest)
        {
            if (id != labRequest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(labRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LabRequestExists(labRequest.Id))
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
            PopulateViewData(labRequest);
            return View(labRequest);
        }

        // GET: LabRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labRequest = await _context.LabRequests
                .Include(l => l.Client)
                .Include(l => l.DeliveredBy)
                .Include(l => l.Department)
                .Include(l => l.DigestedBy)
                .Include(l => l.EnteredBy)
                .Include(l => l.PlantSource)
                .Include(l => l.PreparedBy)
                .Include(l => l.ReceivedBy)
                .Include(l => l.TitratedBy)
                .Include(l => l.WeighedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (labRequest == null)
            {
                return NotFound();
            }

            return View(labRequest);
        }

        // POST: LabRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var labRequest = await _context.LabRequests.FindAsync(id);
            if (labRequest != null)
            {
                _context.LabRequests.Remove(labRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LabRequestExists(int id)
        {
            return _context.LabRequests.Any(e => e.Id == id);
        }
    }
}
