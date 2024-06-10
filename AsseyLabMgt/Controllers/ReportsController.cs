using AsseyLabMgt.Data;
using AsseyLabMgt.Models;
using AsseyLabMgt.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AsseyLabMgt.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ReportService _reportService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(ApplicationDbContext context, ILogger<ReportsController> logger, ReportService reportService)
        {
            _reportService = reportService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var elementNames = _reportService.GetElementNames();
            var plants = await _context.PlantSources.Select(ps => new SelectListItem
            {
                Value = ps.Id.ToString(),
                Text = ps.PlantSourceName
            }).ToListAsync();

            var model = new ReportViewModel
            {
                ElementList = elementNames,
                Plants = plants
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport(ReportViewModel model, string reportType)
        {
            if (model.StartDate == default || model.EndDate == default || model.SelectedElements == null || !model.SelectedElements.Any())
            {
                ModelState.AddModelError("", "Please provide valid inputs.");
                return View("Index", model);
            }

            try
            {
                byte[] reportData = null; // Initialize reportData to null
                string fileName = "";

                switch (reportType)
                {
                    case "GeologyReport":
                        if (model.SelectedElements == null || !model.SelectedElements.Any())
                        {
                            // If no elements are selected, select all available elements
                            model.SelectedElements = _reportService.GetElementNames().Select(e => e.Value).ToList();
                        }

                        reportData = await _reportService.GenerateGeologyReportAsync(model.StartDate, model.EndDate, model.SelectedElements);
                        fileName = $"GeologyReport-{model.StartDate:yyyy-MM-dd}.pdf";
                        break;

                    case "MetReport":
                        if (string.IsNullOrEmpty(model.Description))
                        {
                            ModelState.AddModelError("", "Please provide a description for the Met Report.");
                            return View("Index", model);
                        }
                        reportData = await _reportService.GenerateMetReportAsync(model.StartDate, model.EndDate, model.SelectedElements, model.Description, model.JobNumber);
                        fileName = $"MetReport-{model.StartDate:yyyy-MM-dd}.pdf";
                        break;

                    case "DailyReport":
                        reportData = await _reportService.GenerateDailyReportAsync(model.StartDate, model.SelectedPlantIds, model.SelectedElements);
                        fileName = $"DailyReport-{model.StartDate:yyyy-MM-dd}.pdf";
                        break;

                    default:
                        ModelState.AddModelError("", "Invalid report type.");
                        return View("Index", model);
                }

                return File(reportData, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while generating the report. " + ex.Message);
                return View("Index", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateGeologyReport(ReportViewModel model, string reportType)
        {
            if (model.StartDate == default || model.SelectedElements == null || !model.SelectedElements.Any())
            {
                ModelState.AddModelError("", "Please provide valid inputs.");
                return View("Geology", model);
            }

            try
            {
                byte[] reportData;
                string fileName;

                if (model.SelectedElements == null || !model.SelectedElements.Any())
                {
                    // If no elements are selected, select all available elements
                    model.SelectedElements = _reportService.GetElementNames().Select(e => e.Value).ToList();
                }

                reportData = await _reportService.GenerateGeologyReportAsync(model.StartDate, model.EndDate, model.SelectedElements);
                fileName = $"GeologyReport-{model.StartDate:yyyy-MM-dd}.pdf";

                return File(reportData, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while generating the report. " + ex.Message);
                return View("Geology", model);
            }
        }

        [HttpGet]
        public JsonResult SearchJobNumbers(string term)
        {
            var jobNumbers = _reportService.SearchJobNumbers(term);
            return Json(jobNumbers);
        }

        [HttpGet]
        public async Task<IActionResult> Geology()
        {
            var elementNames = _reportService.GetElementNames();
            var plants = await _context.PlantSources.Select(ps => new SelectListItem
            {
                Value = ps.Id.ToString(),
                Text = ps.PlantSourceName
            }).ToListAsync();

            var model = new ReportViewModel
            {
                ElementList = elementNames,
                Plants = plants
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Met()
        {
            var elementNames = _reportService.GetElementNames();
            var plants = await _context.PlantSources.Select(ps => new SelectListItem
            {
                Value = ps.Id.ToString(),
                Text = ps.PlantSourceName
            }).ToListAsync();

            var model = new ReportViewModel
            {
                ElementList = elementNames,
                Plants = plants
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Daily()
        {
            var elementNames = _reportService.GetElementNames();
            var plants = await _context.PlantSources.Select(ps => new SelectListItem
            {
                Value = ps.Id.ToString(),
                Text = ps.PlantSourceName
            }).ToListAsync();

            var model = new ReportViewModel
            {
                ElementList = elementNames,
                Plants = plants
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateMetReport(ReportViewModel model, string reportType)
        {
            if (model.StartDate == default || model.EndDate == default || model.SelectedElements == null || !model.SelectedElements.Any())
            {
                ModelState.AddModelError("", "Please provide valid inputs.");
                return View("Met", model);
            }

            if (string.IsNullOrEmpty(model.Description))
            {
                ModelState.AddModelError("", "Please provide a description for the Met Report.");
                return View("Met", model);
            }

            try
            {
                byte[] reportData = null; // Initialize reportData to null
                string fileName = "";

                if (reportType == "MetReport")
                {
                    reportData = await _reportService.GenerateMetReportAsync(model.StartDate, model.EndDate, model.SelectedElements, model.Description, model.JobNumber);
                    fileName = $"MetReport-{model.StartDate:yyyy-MM-dd}.pdf";
                }
                else
                {
                    ModelState.AddModelError("", "Invalid report type.");
                    return View("Met", model);
                }

                return File(reportData, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while generating the report. " + ex.Message);
                return View("Met", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateDailyReport(ReportViewModel model, string reportType)
        {
            if (model.StartDate == default || model.SelectedElements == null || !model.SelectedElements.Any())
            {
                ModelState.AddModelError("", "Please provide valid inputs.");
                return View("Daily", model);
            }

            try
            {
                byte[] reportData = null; // Initialize reportData to null
                string fileName = "";

                if (reportType == "DailyReport")
                {
                    reportData = await _reportService.GenerateDailyReportAsync(model.StartDate, model.SelectedPlantIds, model.SelectedElements);
                    fileName = $"DailyReport-{model.StartDate:yyyy-MM-dd}.pdf";
                }
                else
                {
                    ModelState.AddModelError("", "Invalid report type.");
                    return View("Daily", model);
                }

                return File(reportData, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while generating the report. " + ex.Message);
                return View("Daily", model);
            }
        }


    }
}
