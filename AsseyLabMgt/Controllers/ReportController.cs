using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AsseyLabMgt.Data;
using AsseyLabMgt.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using AsseyLabMgt.Utils;

namespace AsseyLabMgt.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ReportGeneratorService _reportGeneratorService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(ApplicationDbContext context, ReportGeneratorService reportGeneratorService, ILogger<ReportController> logger)
        {
            _context = context;
            _reportGeneratorService = reportGeneratorService;
            _logger = logger;
        }

        // GET: Report
        public async Task<IActionResult> Index()
        {
            // var departments = await _context.Departments.ToListAsync();
            return View();
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
                    var reportBytes = await _reportGeneratorService.GenerateGeologyReportAsync(model.StartDate, model.EndDate);
                    return File(reportBytes, "application/pdf", $"GeologyReport-{DateTime.Now:yyyyMMddHHmmss}.pdf");

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while generating the report.");
                    ModelState.AddModelError("", "An error occurred while generating the report.");
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
                    var reportBytes = await _reportGeneratorService.GenerateSamplesReceivedReportAsync(model.StartDate, model.EndDate);
                    return File(reportBytes, "application/pdf", $"SamplesReceivedReport-{DateTime.Now:yyyyMMddHHmmss}.pdf");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while generating the report.");
                    ModelState.AddModelError("", "An error occurred while generating the report.");
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
                    var reportBytes = await _reportGeneratorService.GenerateYearToDateSamplesReceivedReportAsync(model.StartDate, model.EndDate);
                    return File(reportBytes, "application/pdf", $"YearToDateSamplesReceivedReport-{DateTime.Now:yyyyMMddHHmmss}.pdf");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while generating the year-to-date report.");
                    ModelState.AddModelError("", "An error occurred while generating the report.");
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
                    var reportBytes = await _reportGeneratorService.GenerateYearToDateAnalysisStatisticsReportAsync(model.StartDate, model.EndDate);
                    return File(reportBytes, "application/pdf", $"YearToDateAnalysisStatisticsReport-{DateTime.Now:yyyyMMddHHmmss}.pdf");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while generating the year-to-date analysis statistics report.");
                    ModelState.AddModelError("", "An error occurred while generating the report.");
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

                    var reportBytes = await _reportGeneratorService.GenerateMetReportAsync(model.StartDate, model.EndDate);
                    return File(reportBytes, "application/pdf", $"MetReport-{DateTime.Now:yyyyMMddHHmmss}.pdf");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while generating the met report.");
                    ModelState.AddModelError("", "An error occurred while generating the report.");
                }
            }
            return View("Index", model);
        }
    }
}
