using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AsseyLabMgt.Data;
using AsseyLabMgt.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using AsseyLabMgt.Utils;

namespace AsseyLabMgt.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(ApplicationDbContext context, ILogger<ReportController> logger, ReportService reportService)
        {
            _context = context;
            _logger = logger;
            _reportService = reportService;
        }

        // GET: Report
        public async Task<IActionResult> Index()
        {
            var plants = await _context.PlantSources.Select(ps => new SelectListItem
            {
                Value = ps.Id.ToString(),
                Text = ps.PlantSourceName
            }).ToListAsync();

            var model = new ReportViewModel
            {
                Plants = plants
            };

            return View(model);
        }

        // POST: Report/Generate
        [HttpPost]
        public async Task<IActionResult> Generate(ReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.StartDate = model.StartDate.ToUniversalTime();
                    model.EndDate = model.EndDate.ToUniversalTime();
                    var reportBytes = await _reportService.GenerateGeologyReportAsync(model.StartDate, model.EndDate);
                    return File(reportBytes, "application/pdf", $"GeoDaily-{DateTime.Now:yyyyMMddHHmmss}.pdf");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while generating the geology report.");
                    ModelState.AddModelError("", "An error occurred while generating the geology report. Please try again later.");
                }
            }
            return View(model);
        }

        // POST: Report/GenerateSamplesReceived
        [HttpPost]
        public async Task<IActionResult> GenerateSamplesReceived(ReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.StartDate = model.StartDate.ToUniversalTime();
                    model.EndDate = model.EndDate.ToUniversalTime();
                    var reportBytes = await _reportService.GenerateSamplesReceivedReportAsync(model.StartDate, model.EndDate);
                    return File(reportBytes, "application/pdf", $"DailySamples-{DateTime.Now:yyyyMMddHHmmss}.pdf");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while generating the samples received report.");
                    ModelState.AddModelError("", "An error occurred while generating the samples received report. Please try again later.");
                }
            }
            return View(model);
        }

        // POST: Report/GenerateYearToDateSamplesReceived
        [HttpPost]
        public async Task<IActionResult> GenerateYearToDateSamplesReceived(ReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.StartDate = model.StartDate.ToUniversalTime();
                    model.EndDate = model.EndDate.ToUniversalTime();
                    var reportBytes = await _reportService.GenerateYearToDateSamplesReceivedReportAsync(model.StartDate, model.EndDate);
                    return File(reportBytes, "application/pdf", $"MonthSamples-{DateTime.Now:yyyyMMddHHmmss}.pdf");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while generating the year-to-date samples received report.");
                    ModelState.AddModelError("", "An error occurred while generating the year-to-date samples received report. Please try again later.");
                }
            }
            return View("Index", model);
        }

        // POST: Report/GenerateYearToDateAnalysisStatistics
        [HttpPost]
        public async Task<IActionResult> GenerateYearToDateAnalysisStatistics(ReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.StartDate = model.StartDate.ToUniversalTime();
                    model.EndDate = model.EndDate.ToUniversalTime();
                    var reportBytes = await _reportService.GenerateYearToDateAnalysisStatisticsReportAsync(model.StartDate, model.EndDate);
                    return File(reportBytes, "application/pdf", $"MonthsTotals-{DateTime.Now:yyyyMMddHHmmss}.pdf");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while generating the year-to-date analysis statistics report.");
                    ModelState.AddModelError("", "An error occurred while generating the year-to-date analysis statistics report. Please try again later.");
                }
            }
            return View("Index", model);
        }

        // POST: Report/GenerateMetReport
        [HttpPost]
        public async Task<IActionResult> GenerateMetReport(ReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.StartDate = model.StartDate.ToUniversalTime();
                    model.EndDate = model.EndDate.ToUniversalTime();
                    var reportBytes = await _reportService.GenerateMetReportAsync(model.StartDate, model.EndDate);
                    return File(reportBytes, "application/pdf", $"MetDaily-{DateTime.Now:yyyyMMddHHmmss}.pdf");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while generating the metallurgy report.");
                    ModelState.AddModelError("", "An error occurred while generating the metallurgy report. Please try again later.");
                }
            }
            return View("Index", model);
        }

        // POST: Report/GeneratePlantReport
        [HttpPost]
        public async Task<IActionResult> GeneratePlantReport(ReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.StartDate = model.StartDate.ToUniversalTime();
                    model.EndDate = model.EndDate.ToUniversalTime();
                    var selectedPlantIds = model.SelectedPlantIds ?? new List<int>();
                    var pdfData = await _reportService.GeneratePlantReportAsync(model.StartDate, model.EndDate, selectedPlantIds);
                    return File(pdfData, "application/pdf", $"PlantDailyReport-{DateTime.Now:yyyyMMddHHmmss}.pdf");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while generating the plant report.");
                    ModelState.AddModelError("", "An error occurred while generating the plant report. Please try again later.");
                }
            }
            return View("Index", model);
        }
    }
}
