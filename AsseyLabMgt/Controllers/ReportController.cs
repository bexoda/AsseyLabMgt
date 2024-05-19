using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AsseyLabMgt.Data;
using AsseyLabMgt.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using AsseyLabMgt.Utils;
using Microsoft.AspNetCore.Authorization;

namespace AsseyLabMgt.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ReportGeneratorService _reportGeneratorService;
        private readonly ReportService _reportService;
        private readonly ILogger<ReportController> _logger;
       
        public ReportController(ApplicationDbContext context, ReportGeneratorService reportGeneratorService, ILogger<ReportController> logger, ReportService reportService)
        {
            _context = context;
            _reportGeneratorService = reportGeneratorService;
            _logger = logger;
            _reportService = reportService;
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
                    //var reportBytes = await _reportGeneratorService.GenerateGeologyReportAsync(model.StartDate, model.EndDate);
                    var reportBytes = await _reportService.GenerateGeologyReportAsync(model.StartDate, model.EndDate);
                    return File(reportBytes, "application/pdf", $"GeoDaily-{DateTime.Now:yyyyMMddHHmmss}.pdf");

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
                    //var reportBytes = await _reportGeneratorService.GenerateSamplesReceivedReportAsync(model.StartDate, model.EndDate);
                    var reportBytes = await _reportService.GenerateSamplesReceivedReportAsync(model.StartDate, model.EndDate);
                    return File(reportBytes, "application/pdf", $"DailySamples-{DateTime.Now:yyyyMMddHHmmss}.pdf");
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
                    //var reportBytes = await _reportGeneratorService.GenerateYearToDateSamplesReceivedReportAsync(model.StartDate, model.EndDate);
                    var reportBytes = await _reportService.GenerateYearToDateSamplesReceivedReportAsync(model.StartDate, model.EndDate);
                    return File(reportBytes, "application/pdf", $"MonthSamples-{DateTime.Now:yyyyMMddHHmmss}.pdf");
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
                    //var reportBytes = await _reportGeneratorService.GenerateYearToDateAnalysisStatisticsReportAsync(model.StartDate, model.EndDate);
                    var reportBytes = await _reportService.GenerateYearToDateAnalysisStatisticsReportAsync(model.StartDate, model.EndDate);
                    return File(reportBytes, "application/pdf", $"MonthsTotals-{DateTime.Now:yyyyMMddHHmmss}.pdf");
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

                    //var reportBytes = await _reportGeneratorService.GenerateMetReportAsync(model.StartDate, model.EndDate);
                    var reportBytes = await _reportService.GenerateMetReportAsync(model.StartDate, model.EndDate);
                    return File(reportBytes, "application/pdf", $"MetDaily-{DateTime.Now:yyyyMMddHHmmss}.pdf");
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
