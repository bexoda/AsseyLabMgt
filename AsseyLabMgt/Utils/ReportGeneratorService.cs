using AsseyLabMgt.Data;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace AsseyLabMgt.Utils
{


    public class ReportGeneratorService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ReportGeneratorService> _logger;
        private readonly ApplicationDbContext _context;

        public ReportGeneratorService(IWebHostEnvironment env, ILogger<ReportGeneratorService> logger, ApplicationDbContext context)
        {
            _env = env;
            _logger = logger;
            _context = context;
        }


        // Method to fetch data and generate the PDF report
        public async Task<byte[]> GenerateGeologyReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Fetch data from the database
                var labResults = await _context.LabResults
                    .Where(lr => lr.CreatedDate >= startDate && lr.CreatedDate <= endDate)
                    .ToListAsync();

                var groupedResults = labResults.GroupBy(lr => lr.CreatedDate.Date)
                 .Select(g => new
                 {
                     Date = g.Key,
                     FeCount = g.Count(lr => lr.Fe > 0),
                     MnCount = g.Count(lr => lr.Mn > 0)
                 }).ToList();


                // Generate PDF
                using var stream = new MemoryStream();
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("Times New Roman", 12, XFontStyle.Regular);

                var titleFont = new XFont("Times New Roman", 18, XFontStyle.Bold);
                var subtitleFont = new XFont("Times New Roman", 12, XFontStyle.Italic);

                gfx.DrawString("GMC Geology Report - Analysis Statistics", titleFont, XBrushes.Black,
                    new XRect(0, 0, page.Width, page.Height), XStringFormats.TopCenter);
                gfx.DrawString($"From {startDate:dd-MMM-yyyy} through {endDate:dd-MMM-yyyy}", subtitleFont, XBrushes.Black,
                    new XRect(0, 40, page.Width, page.Height), XStringFormats.TopCenter);
                gfx.DrawString(DateTime.Now.ToString("dd-MMM-yyyy"), font, XBrushes.Black,
                    new XRect(0, 20, page.Width, page.Height), XStringFormats.TopCenter);


                // Table header
                gfx.DrawString("Production Date", font, XBrushes.Black, new XRect(40, 60, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawString("Fe", font, XBrushes.Black, new XRect(200, 60, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawString("Mn", font, XBrushes.Black, new XRect(300, 60, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawString("Totals", font, XBrushes.Black, new XRect(400, 60, page.Width, page.Height), XStringFormats.TopLeft);

                // Table content
                int yPoint = 80;
                foreach (var result in groupedResults)
                {
                    gfx.DrawString(result.Date.ToString("dd/MMM/yyyy"), font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString(result.FeCount.ToString(), font, XBrushes.Black, new XRect(200, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString(result.MnCount.ToString(), font, XBrushes.Black, new XRect(300, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString((result.FeCount + result.MnCount).ToString(), font, XBrushes.Black, new XRect(400, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    yPoint += 20;
                }

                // Totals
                gfx.DrawString("Totals:", font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawString(groupedResults.Sum(gr => gr.FeCount).ToString(), font, XBrushes.Black, new XRect(200, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawString(groupedResults.Sum(gr => gr.MnCount).ToString(), font, XBrushes.Black, new XRect(300, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawString(groupedResults.Sum(gr => gr.FeCount + gr.MnCount).ToString(), font, XBrushes.Black, new XRect(400, yPoint, page.Width, page.Height), XStringFormats.TopLeft);

                document.Save(stream);
                _logger.LogInformation("Report generated successfully from {StartDate} to {EndDate}", startDate, endDate);

                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the report.");
                throw;
            }
        }

        public async Task<byte[]> GenerateSamplesReceivedReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Fetch data from the database
                var labRequests = await _context.LabRequests
                    .Where(lr => lr.ProductionDate >= startDate && lr.ProductionDate <= endDate)
                    .Include(lr => lr.LabResults)
                    .Include(lr => lr.PlantSource)
                    .ToListAsync();

                var plantSources = await _context.PlantSources.ToListAsync();

                var groupedResults = labRequests.GroupBy(lr => lr.ProductionDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        PlantSourceCounts = plantSources.ToDictionary(ps => ps.PlantSourceName, ps => g.Sum(lr => lr.LabResults.Count(res => lr.PlantSourceId == ps.Id)))
                    }).ToList();

                // Generate PDF
                using var stream = new MemoryStream();
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("Times New Roman", 12, XFontStyle.Regular);
                var pen = new XPen(XColors.Black, 0.5);

                var titleFont = new XFont("Times New Roman", 18, XFontStyle.Bold);
                var subtitleFont = new XFont("Times New Roman", 12, XFontStyle.Italic);

                gfx.DrawString("GMC Daily Report - Samples Received Statistics", titleFont, XBrushes.Black,
                    new XRect(0, 0, page.Width, page.Height), XStringFormats.TopCenter);
                gfx.DrawString($"From {startDate:dd-MMM-yyyy} through {endDate:dd-MMM-yyyy}", subtitleFont, XBrushes.Black,
                    new XRect(0, 40, page.Width, page.Height), XStringFormats.TopCenter);
                gfx.DrawString(DateTime.Now.ToString("dd-MMM-yyyy"), font, XBrushes.Black,
                    new XRect(0, 20, page.Width, page.Height), XStringFormats.TopCenter);


                // Table header
                gfx.DrawString("Production Date", font, XBrushes.Black, new XRect(40, 60, page.Width, page.Height), XStringFormats.TopLeft);
                int xPoint = 200;
                foreach (var plantSource in plantSources)
                {
                    gfx.DrawString(plantSource.PlantSourceName, font, XBrushes.Black, new XRect(xPoint, 60, page.Width, page.Height), XStringFormats.TopLeft);
                    xPoint += 60;
                }
                gfx.DrawString("Totals", font, XBrushes.Black, new XRect(xPoint, 60, page.Width, page.Height), XStringFormats.TopLeft);

                // Draw header borders
                gfx.DrawRectangle(pen, 40, 60, page.Width - 80, 20);

                // Table content
                int yPoint = 80;
                foreach (var result in groupedResults)
                {
                    gfx.DrawString(result.Date.ToString("dd/MMM/yyyy"), font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    xPoint = 200;
                    int rowTotal = 0;
                    foreach (var plantSource in plantSources)
                    {
                        int count = result.PlantSourceCounts[plantSource.PlantSourceName];
                        gfx.DrawString(count.ToString(), font, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                        rowTotal += count;
                        gfx.DrawRectangle(pen, xPoint, yPoint, 60, 20);
                        xPoint += 60;
                    }
                    gfx.DrawString(rowTotal.ToString(), font, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawRectangle(pen, xPoint, yPoint, 60, 20);
                    gfx.DrawRectangle(pen, 40, yPoint, 160, 20);

                    yPoint += 20;
                }

                // Totals
                gfx.DrawString("Totals:", font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                xPoint = 200;
                foreach (var plantSource in plantSources)
                {
                    int total = groupedResults.Sum(gr => gr.PlantSourceCounts[plantSource.PlantSourceName]);
                    gfx.DrawString(total.ToString(), font, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawRectangle(pen, xPoint, yPoint, 60, 20);
                    xPoint += 60;
                }
                int grandTotal = groupedResults.Sum(gr => gr.PlantSourceCounts.Values.Sum());
                gfx.DrawString(grandTotal.ToString(), font, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawRectangle(pen, xPoint, yPoint, 60, 20);
                gfx.DrawRectangle(pen, 40, yPoint, 160, 20);

                document.Save(stream);
                _logger.LogInformation("Report generated successfully from {StartDate} to {EndDate}", startDate, endDate);

                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the report.");
                throw;
            }
        }

        public async Task<byte[]> GenerateYearToDateSamplesReceivedReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Fetch data from the database
                var labRequests = await _context.LabRequests
                    .Where(lr => lr.ProductionDate >= startDate && lr.ProductionDate <= endDate)
                    .Include(lr => lr.LabResults)
                    .Include(lr => lr.PlantSource)
                    .ToListAsync();

                var plantSources = await _context.PlantSources.ToListAsync();

                var groupedResults = labRequests.GroupBy(lr => new { lr.ProductionDate.Year, lr.ProductionDate.Month })
                    .Select(g => new
                    {
                        Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                        PlantSourceCounts = plantSources.ToDictionary(ps => ps.PlantSourceName, ps => g.Sum(lr => lr.LabResults.Count(res => lr.PlantSourceId == ps.Id)))
                    }).ToList();

                // Generate PDF
                using var stream = new MemoryStream();
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("Times New Roman", 12, XFontStyle.Regular);
                var titleFont = new XFont("Times New Roman", 18, XFontStyle.Bold);
                var subtitleFont = new XFont("Times New Roman", 12, XFontStyle.Italic);
                var pen = new XPen(XColors.Black, 0.5);

                gfx.DrawString("GMC Daily Report - Year-To-Date Samples Received Statistics", titleFont, XBrushes.Black,
                    new XRect(0, 0, page.Width, page.Height), XStringFormats.TopCenter);
                gfx.DrawString($"From {startDate:MMM-yyyy} through {endDate:MMM-yyyy}", subtitleFont, XBrushes.Black,
                    new XRect(0, 40, page.Width, page.Height), XStringFormats.TopCenter);
                gfx.DrawString(DateTime.Now.ToString("dd-MMM-yyyy"), font, XBrushes.Black,
                                  new XRect(0, 20, page.Width, page.Height), XStringFormats.TopCenter);


                // Table header
                gfx.DrawString("Month", font, XBrushes.Black, new XRect(40, 60, page.Width, page.Height), XStringFormats.TopLeft);
                int xPoint = 200;
                foreach (var plantSource in plantSources)
                {
                    gfx.DrawString(plantSource.PlantSourceName, font, XBrushes.Black, new XRect(xPoint, 60, page.Width, page.Height), XStringFormats.TopLeft);
                    xPoint += 60;
                }
                gfx.DrawString("Totals", font, XBrushes.Black, new XRect(xPoint, 60, page.Width, page.Height), XStringFormats.TopLeft);

                // Draw header borders
                gfx.DrawRectangle(pen, 40, 60, page.Width - 80, 20);

                // Table content
                int yPoint = 80;
                foreach (var result in groupedResults)
                {
                    gfx.DrawString(result.Month.ToString("MMM-yyyy"), font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    xPoint = 200;
                    int rowTotal = 0;
                    foreach (var plantSource in plantSources)
                    {
                        int count = result.PlantSourceCounts[plantSource.PlantSourceName];
                        gfx.DrawString(count.ToString(), font, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                        rowTotal += count;
                        gfx.DrawRectangle(pen, xPoint, yPoint, 60, 20);
                        xPoint += 60;
                    }
                    gfx.DrawString(rowTotal.ToString(), font, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawRectangle(pen, xPoint, yPoint, 60, 20);
                    gfx.DrawRectangle(pen, 40, yPoint, 160, 20);

                    yPoint += 20;
                }

                // Totals
                gfx.DrawString("Totals:", font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                xPoint = 200;
                foreach (var plantSource in plantSources)
                {
                    int total = groupedResults.Sum(gr => gr.PlantSourceCounts[plantSource.PlantSourceName]);
                    gfx.DrawString(total.ToString(), font, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawRectangle(pen, xPoint, yPoint, 60, 20);
                    xPoint += 60;
                }
                int grandTotal = groupedResults.Sum(gr => gr.PlantSourceCounts.Values.Sum());
                gfx.DrawString(grandTotal.ToString(), font, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawRectangle(pen, xPoint, yPoint, 60, 20);
                gfx.DrawRectangle(pen, 40, yPoint, 160, 20);

                document.Save(stream);
                _logger.LogInformation("Year-to-date report generated successfully from {StartDate} to {EndDate}", startDate, endDate);

                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the year-to-date report.");
                throw;
            }
        }

        public async Task<byte[]> GenerateYearToDateAnalysisStatisticsReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Fetch data from the database
                var labRequests = await _context.LabRequests
                    .Where(lr => lr.ProductionDate >= startDate && lr.ProductionDate <= endDate)
                    .Include(lr => lr.LabResults)
                    .Include(lr => lr.PlantSource)
                    .ToListAsync();

                var plantSources = await _context.PlantSources.ToListAsync();

                var groupedResults = labRequests.GroupBy(lr => new { lr.ProductionDate.Year, lr.ProductionDate.Month })
                    .Select(g => new
                    {
                        Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                        PlantSourceCounts = plantSources.ToDictionary(ps => ps.PlantSourceName, ps => g.Sum(lr => lr.LabResults.Count(res => lr.PlantSourceId == ps.Id)))
                    }).ToList();

                // Generate PDF
                using var stream = new MemoryStream();
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("Times New Roman", 12, XFontStyle.Regular);
                var pen = new XPen(XColors.Black, 0.5);

                var titleFont = new XFont("Times New Roman", 18, XFontStyle.Bold);
                var subtitleFont = new XFont("Times New Roman", 12, XFontStyle.Italic);

                gfx.DrawString("GMC Year-To-Date Report - Analysis Statistics", titleFont, XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.TopCenter);
                gfx.DrawString($"From {startDate:MMM-yyyy} through {endDate:MMM-yyyy}", subtitleFont, XBrushes.Black, new XRect(0, 40, page.Width, page.Height), XStringFormats.TopCenter);
                gfx.DrawString(DateTime.Now.ToString("dd-MMM-yyyy"), font, XBrushes.Black, new XRect(0, 20, page.Width, page.Height), XStringFormats.TopCenter);

                // Table header
                gfx.DrawString("Month", font, XBrushes.Black, new XRect(40, 60, page.Width, page.Height), XStringFormats.TopLeft);
                int xPoint = 200;
                foreach (var plantSource in plantSources)
                {
                    gfx.DrawString(plantSource.PlantSourceName, font, XBrushes.Black, new XRect(xPoint, 60, page.Width, page.Height), XStringFormats.TopLeft);
                    xPoint += 60;
                }
                gfx.DrawString("Totals", font, XBrushes.Black, new XRect(xPoint, 60, page.Width, page.Height), XStringFormats.TopLeft);

                // Draw header borders
                gfx.DrawRectangle(pen, 40, 60, page.Width - 80, 20);

                // Table content
                int yPoint = 80;
                foreach (var result in groupedResults)
                {
                    gfx.DrawString(result.Month.ToString("MMM-yyyy"), font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    xPoint = 200;
                    int rowTotal = 0;
                    foreach (var plantSource in plantSources)
                    {
                        int count = result.PlantSourceCounts[plantSource.PlantSourceName];
                        gfx.DrawString(count.ToString(), font, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                        rowTotal += count;
                        gfx.DrawRectangle(pen, xPoint, yPoint, 60, 20);
                        xPoint += 60;
                    }
                    gfx.DrawString(rowTotal.ToString(), font, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawRectangle(pen, xPoint, yPoint, 60, 20);
                    gfx.DrawRectangle(pen, 40, yPoint, 160, 20);

                    yPoint += 20;
                }

                // Totals
                gfx.DrawString("Totals:", font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                xPoint = 200;
                foreach (var plantSource in plantSources)
                {
                    int total = groupedResults.Sum(gr => gr.PlantSourceCounts[plantSource.PlantSourceName]);
                    gfx.DrawString(total.ToString(), font, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawRectangle(pen, xPoint, yPoint, 60, 20);
                    xPoint += 60;
                }
                int grandTotal = groupedResults.Sum(gr => gr.PlantSourceCounts.Values.Sum());
                gfx.DrawString(grandTotal.ToString(), font, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawRectangle(pen, xPoint, yPoint, 60, 20);
                gfx.DrawRectangle(pen, 40, yPoint, 160, 20);

                document.Save(stream);
                _logger.LogInformation("Year-to-date analysis statistics report generated successfully from {StartDate} to {EndDate}", startDate, endDate);

                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the year-to-date analysis statistics report.");
                throw;
            }
        }

        public async Task<byte[]> GenerateMetReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Fetch data from the database
                var labResults = await _context.LabResults
                    .Where(lr => lr.CreatedDate >= startDate && lr.CreatedDate <= endDate)
                    .Include(lr => lr.LabRequest)
                    .ToListAsync();

                var groupedResults = labResults.GroupBy(lr => lr.LabRequest.ProductionDate.Date)
                  .Select(g => new
                  {
                      Date = g.Key,
                      Al2O3 = g.Count(lr => lr.Al2O3 > 0),
                      CaO = g.Count(lr => lr.CaO > 0),
                      Fe = g.Count(lr => lr.Fe > 0),
                      H2O = g.Count(lr => lr.H2O > 0),
                      Mg = g.Count(lr => lr.Mg > 0),
                      MgO = g.Count(lr => lr.MgO > 0),
                      Mn = g.Count(lr => lr.Mn > 0),
                      P = g.Count(lr => lr.B > 0), // Assuming B represents P in the data
                      SiO2 = g.Count(lr => lr.SiO2 > 0),
                      Total = g.Count(lr => lr.Al2O3 > 0) + g.Count(lr => lr.CaO > 0) + g.Count(lr => lr.Fe > 0) + g.Count(lr => lr.H2O > 0) + g.Count(lr => lr.Mg > 0) + g.Count(lr => lr.MgO > 0) + g.Count(lr => lr.Mn > 0) + g.Count(lr => lr.B > 0) + g.Count(lr => lr.SiO2 > 0)
                  }).ToList();


                // Generate PDF
                using var stream = new MemoryStream();
                var document = new PdfDocument();
                var page = document.AddPage();
                page.Orientation = PageOrientation.Landscape; // Set the page orientation to landscape
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("Times New Roman", 12, XFontStyle.Regular);
                var pen = new XPen(XColors.Black, 0.5);

                var titleFont = new XFont("Times New Roman", 18, XFontStyle.Bold);
                var subtitleFont = new XFont("Times New Roman", 12, XFontStyle.Italic);

                gfx.DrawString("GMC MET Report - Analysis Statistics", titleFont, XBrushes.Black,
                    new XRect(0, 0, page.Width, page.Height), XStringFormats.TopCenter);
                gfx.DrawString($"From {startDate:dd-MMM-yyyy} through {endDate:dd-MMM-yyyy}", subtitleFont, XBrushes.Black,
                    new XRect(0, 40, page.Width, page.Height), XStringFormats.TopCenter);
                gfx.DrawString(DateTime.Now.ToString("dd-MMM-yyyy"), font, XBrushes.Black,
                    new XRect(0, 20, page.Width, page.Height), XStringFormats.TopCenter);


                // Table header
                string[] headers = { "Prod_Date", "Al2O3", "CaO", "Fe", "H2O", "Mg", "MgO", "Mn", "P", "SiO2", "Totals" };
                int xPoint = 40;
                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawString(headers[i], font, XBrushes.Black, new XRect(xPoint, 60, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawRectangle(pen, xPoint, 60, 60, 20);
                    xPoint += 60;
                }

                // Table content
                int yPoint = 80;
                foreach (var result in groupedResults)
                {
                    gfx.DrawString(result.Date.ToString("dd-MM-yy"), font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString(result.Al2O3.ToString(), font, XBrushes.Black, new XRect(100, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString(result.CaO.ToString(), font, XBrushes.Black, new XRect(160, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString(result.Fe.ToString(), font, XBrushes.Black, new XRect(220, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString(result.H2O.ToString(), font, XBrushes.Black, new XRect(280, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString(result.Mg.ToString(), font, XBrushes.Black, new XRect(340, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString(result.MgO.ToString(), font, XBrushes.Black, new XRect(400, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString(result.Mn.ToString(), font, XBrushes.Black, new XRect(460, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString(result.P.ToString(), font, XBrushes.Black, new XRect(520, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString(result.SiO2.ToString(), font, XBrushes.Black, new XRect(580, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                    gfx.DrawString(result.Total.ToString(), font, XBrushes.Black, new XRect(640, yPoint, page.Width, page.Height), XStringFormats.TopLeft);

                    // Draw content borders
                    for (int i = 0; i < headers.Length; i++)
                    {
                        gfx.DrawRectangle(pen, 40 + (i * 60), yPoint, 60, 20);
                    }
                    yPoint += 20;
                }

                // Totals
                gfx.DrawString("Totals:", font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawString(groupedResults.Sum(r => r.Al2O3).ToString(), font, XBrushes.Black, new XRect(100, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawString(groupedResults.Sum(r => r.CaO).ToString(), font, XBrushes.Black, new XRect(160, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawString(groupedResults.Sum(r => r.Fe).ToString(), font, XBrushes.Black, new XRect(220, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawString(groupedResults.Sum(r => r.H2O).ToString(), font, XBrushes.Black, new XRect(280, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawString(groupedResults.Sum(r => r.Mg).ToString(), font, XBrushes.Black, new XRect(340, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawString(groupedResults.Sum(r => r.MgO).ToString(), font, XBrushes.Black, new XRect(400, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawString(groupedResults.Sum(r => r.Mn).ToString(), font, XBrushes.Black, new XRect(460, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawString(groupedResults.Sum(r => r.P).ToString(), font, XBrushes.Black, new XRect(520, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawString(groupedResults.Sum(r => r.SiO2).ToString(), font, XBrushes.Black, new XRect(580, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                gfx.DrawString(groupedResults.Sum(r => r.Total).ToString(), font, XBrushes.Black, new XRect(640, yPoint, page.Width, page.Height), XStringFormats.TopLeft);

                // Draw totals borders
                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawRectangle(pen, 40 + (i * 60), yPoint, 60, 20);
                }

                document.Save(stream);
                _logger.LogInformation("Met report generated successfully from {StartDate} to {EndDate}", startDate, endDate);

                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the met report.");
                throw;
            }
        }
    }
}