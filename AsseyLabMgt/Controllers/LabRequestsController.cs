using AsseyLabMgt.Data;
using AsseyLabMgt.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AsseyLabMgt.Controllers
{
    [Authorize]
    public class LabRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LabRequestsController> _logger;

        public LabRequestsController(ApplicationDbContext context, ILogger<LabRequestsController> logger)
        {
            _context = context;
            _logger = logger;
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
                TempData["ErrorMessage"] = "LabRequest ID is required.";
                return RedirectToAction(nameof(Index));
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
                TempData["ErrorMessage"] = "LabRequest not found.";
                return RedirectToAction(nameof(Index));
            }

            var labResults = await _context.LabResults
                .Where(lr => lr.LabRequestId == id)
                .ToListAsync();

            ViewBag.LabResults = labResults;

            return View(labRequest);
        }

        // GET: LabRequests/Create
        [HttpGet]
        public IActionResult Create()
        {
            PopulateViewData();
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
            labRequest.JobNumber = jobNumber.ToString();

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
                    _logger.LogError(ex, "Error processing file: " + ex.Message);
                    TempData["ErrorMessage"] = "Error processing file: " + ex.Message;
                }
            }
            else if (labRequest.LabResults != null && labRequest.LabResults.Any())
            {
                // If lab results are manually entered, process them
                
                HttpContext.Session.SetString("LabResults", JsonConvert.SerializeObject(labRequest.LabResults)); // Store the results temporarily
                HttpContext.Session.SetString("LabRequest", JsonConvert.SerializeObject(labRequest)); // Store the LabRequest temporarily
                return RedirectToAction("Confirm"); // Redirect to a confirmation view
            }
            else
            {
                TempData["ErrorMessage"] = "Please upload an Excel file or enter lab results manually.";
            }

            PopulateViewData(labRequest);
            return View(labRequest);
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
                    {
                        _logger.LogError("No worksheet found in the uploaded file.");
                        throw new InvalidOperationException("No worksheet found");
                    }

                    // Map column headers to their respective index
                    var headerMap = new Dictionary<string, int>();
                    var headerRow = worksheet.Row(1);
                    for (int col = 1; col <= headerRow.LastCellUsed().Address.ColumnNumber; col++)
                    {
                        var header = headerRow.Cell(col).GetValue<string>().Trim();
                        headerMap[header] = col;
                    }

                    int row = 2; // Assuming the first row is headers
                    while (!worksheet.Row(row).Cell(1).IsEmpty())
                    {
                        var labResult = new LabResults
                        {
                            LabRequestId = labRequest.Id,
                            SampleId = GetCellValue<string>(worksheet, row, headerMap, "SampleId"),
                            Mn = GetCellValue<decimal>(worksheet, row, headerMap, "Mn"),
                            Sol_Mn = GetCellValue<decimal>(worksheet, row, headerMap, "Sol_Mn"),
                            Fe = GetCellValue<decimal>(worksheet, row, headerMap, "Fe"),
                            B = GetCellValue<decimal>(worksheet, row, headerMap, "B"),
                            MnO2 = GetCellValue<decimal>(worksheet, row, headerMap, "MnO2"),
                            SiO2 = GetCellValue<decimal>(worksheet, row, headerMap, "SiO2"),
                            Al2O3 = GetCellValue<decimal>(worksheet, row, headerMap, "Al2O3"),
                            P = GetCellValue<decimal>(worksheet, row, headerMap, "P"),
                            MgO = GetCellValue<decimal>(worksheet, row, headerMap, "MgO"),
                            CaO = GetCellValue<decimal>(worksheet, row, headerMap, "CaO"),
                            Au = GetCellValue<decimal>(worksheet, row, headerMap, "Au"),
                            As = GetCellValue<decimal>(worksheet, row, headerMap, "As"),
                            H2O = GetCellValue<decimal>(worksheet, row, headerMap, "H2O"),
                            Mg = GetCellValue<decimal>(worksheet, row, headerMap, "Mg"),
                            Time = GetCellValue<DateTime>(worksheet, row, headerMap, "Time") != DateTime.MinValue
                                ? (TimeOnly?)TimeOnly.FromDateTime(GetCellValue<DateTime>(worksheet, row, headerMap, "Time"))
                                : null,
                            CreatedDate = DateTime.UtcNow,
                            IsActive = true
                        };

                        results.Add(labResult);
                        row++;
                    }
                }
            }
            _logger.LogInformation("Processed {0} rows from the uploaded file.", results.Count);
            return results;
        }

        private T GetCellValue<T>(IXLWorksheet worksheet, int row, Dictionary<string, int> headerMap, string columnName)
        {
            if (headerMap.ContainsKey(columnName))
            {
                try
                {
                    return worksheet.Row(row).Cell(headerMap[columnName]).GetValue<T>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Error reading column {0} at row {1}: {2}", columnName, row, ex.Message);
                    return default(T);
                }
            }
            else
            {
                _logger.LogWarning("Column {0} not found in the uploaded file.", columnName);
                return default(T);
            }
        }

        private void PopulateViewData(LabRequest labRequest = null)
        {
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "ClientCode", labRequest?.ClientId);
            ViewData["DeliveredById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest?.DeliveredById);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "DeptCode", labRequest?.DepartmentId);
            ViewData["DigestedById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest?.DigestedById);
            ViewData["EnteredById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest?.EnteredById);
            ViewData["PlantSourceId"] = new SelectList(_context.PlantSources, "Id", "PlantSourceName", labRequest?.PlantSourceId);
            ViewData["PreparedById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest?.PreparedById);
            ViewData["ReceivedById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest?.ReceivedById);
            ViewData["TitratedById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest?.TitratedById);
            ViewData["WeighedById"] = new SelectList(_context.Staff, "Id", "Fullname", labRequest?.WeighedById);
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

            TempData["SuccessMessage"] = "Lab request and results saved successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: LabRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "LabRequest ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var labRequest = await _context.LabRequests.FindAsync(id);
            if (labRequest == null)
            {
                TempData["ErrorMessage"] = "LabRequest not found.";
                return RedirectToAction(nameof(Index));
            }

            PopulateViewData(labRequest);
            return View(labRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LabRequest labRequest)
        {
            if (id != labRequest.Id)
            {
                TempData["ErrorMessage"] = "LabRequest ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            labRequest.UpdatedDate = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(labRequest);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "LabRequest updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LabRequestExists(labRequest.Id))
                    {
                        TempData["ErrorMessage"] = "LabRequest not found.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Concurrency error while updating LabRequest with ID {LabRequestId}.", labRequest.Id);
                        TempData["ErrorMessage"] = "An error occurred while updating the LabRequest.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating LabRequest with ID {LabRequestId}: {ErrorMessage}", labRequest.Id, ex.Message);
                    TempData["ErrorMessage"] = "An error occurred while updating the LabRequest: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid data submitted.";
            }

            PopulateViewData(labRequest);
            return View(labRequest);
        }

        // GET: LabRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "LabRequest ID is required.";
                return RedirectToAction(nameof(Index));
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
                TempData["ErrorMessage"] = "LabRequest not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(labRequest);
        }

        // POST: LabRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var labRequest = await _context.LabRequests.FindAsync(id);
                if (labRequest != null)
                {
                    _context.LabRequests.Remove(labRequest);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "LabRequest deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "LabRequest not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting LabRequest with ID {LabRequestId}: {ErrorMessage}", id, ex.Message);
                TempData["ErrorMessage"] = "An error occurred while deleting the LabRequest: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LabRequestExists(int id)
        {
            return _context.LabRequests.Any(e => e.Id == id);
        }
    }
}
