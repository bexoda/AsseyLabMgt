using AsseyLabMgt.Data;
using AsseyLabMgt.Models;
using AsseyLabMgt.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AsseyLabMgt.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StatisticsService _statisticsService;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(ApplicationDbContext context, ILogger<StatisticsController> logger, StatisticsService statisticsService)
        {
            _context = context;
            _logger = logger;
            _statisticsService = statisticsService;
        }

        // GET: Report
        public async Task<IActionResult> Index()
        {
            var plants = await _context.PlantSources.Select(ps => new SelectListItem
            {
                Value = ps.Id.ToString(),
                Text = ps.PlantSourceName
            }).ToListAsync();

            var model = new StatisticViewModel
            {
                Plants = plants
            };

            return View(model);
        }

        // POST: Report/Generate
        [HttpPost]
        public async Task<IActionResult> Generate(StatisticViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.StartDate = model.StartDate.ToUniversalTime();
                    model.EndDate = model.EndDate.ToUniversalTime();
                    var reportBytes = await _statisticsService.GenerateGeologyReportAsync(model.StartDate, model.EndDate);
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
        public async Task<IActionResult> GenerateSamplesReceived(StatisticViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.StartDate = model.StartDate.ToUniversalTime();
                    model.EndDate = model.EndDate.ToUniversalTime();
                    var reportBytes = await _statisticsService.GenerateSamplesReceivedReportAsync(model.StartDate, model.EndDate);
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
        public async Task<IActionResult> GenerateYearToDateSamplesReceived(StatisticViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.StartDate = model.StartDate.ToUniversalTime();
                    model.EndDate = model.EndDate.ToUniversalTime();
                    var reportBytes = await _statisticsService.GenerateYearToDateSamplesReceivedReportAsync(model.StartDate, model.EndDate);
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
        public async Task<IActionResult> GenerateYearToDateAnalysisStatistics(StatisticViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.StartDate = model.StartDate.ToUniversalTime();
                    model.EndDate = model.EndDate.ToUniversalTime();
                    var reportBytes = await _statisticsService.GenerateYearToDateAnalysisStatisticsReportAsync(model.StartDate, model.EndDate);
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
        public async Task<IActionResult> GenerateMetReport(StatisticViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.StartDate = model.StartDate.ToUniversalTime();
                    model.EndDate = model.EndDate.ToUniversalTime();
                    var reportBytes = await _statisticsService.GenerateMetReportAsync(model.StartDate, model.EndDate);
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
        public async Task<IActionResult> GeneratePlantReport(StatisticViewModel model)
        {

            try
            {
                model.StartDate = model.StartDate.ToUniversalTime();
                model.EndDate = model.EndDate.ToUniversalTime();
                var selectedPlantIds = model.SelectedPlantIds ?? new List<int>();
                var pdfData = await _statisticsService.GeneratePlantReportAsync(model.StartDate, model.EndDate, selectedPlantIds);
                return File(pdfData, "application/pdf", $"PlantDailyReport-{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the plant report.");
                ModelState.AddModelError("", "An error occurred while generating the plant report. Please try again later.");
                return View("Index", model);
            }

        }
    }
}
