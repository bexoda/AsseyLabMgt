using AsseyLabMgt.Data;
using AsseyLabMgt.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using AsseyLabMgt.Data;
using Microsoft.AspNetCore.Hosting;

namespace AsseyLabMgt.Utils
{
    public class ReportService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ReportGeneratorService> _logger;
        private readonly ApplicationDbContext _context;

        public ReportService(IWebHostEnvironment env, ILogger<ReportGeneratorService> logger, ApplicationDbContext context)
        {
            _env = env;
            _logger = logger;
            _context = context;
        }

        // Method to fetch data and generate the PDF report
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

                // Define PDF document
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);

                        page.Header().Height(100).Background(Colors.White)
                            .AlignLeft()
                            .Row(row =>
                            {
                                row.ConstantItem(100).Image(Path.Combine(_env.WebRootPath, "img", "GMC-LOGO-1.png")); // Add logo
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text("GMC Geology Report").FontFamily("Times")
                                        .FontSize(18)
                                        .Bold()
                                        .AlignRight();
                                    column.Item().Text("Analysis Statistics")
                                     .FontFamily("Times") // Set font family
                                        .FontSize(18)
                                        .Bold()
                                        .AlignRight();
                                    column.Item().Text($"From {startDate:dd-MMM-yyyy} through {endDate:dd-MMM-yyyy}")
                                     .FontFamily("Times") // Set font family
                                        .FontSize(10)
                                        .AlignRight();

                                });

                            });

                        page.Content().Background(Colors.Green.Lighten5)
                            .PaddingVertical(20)

                            .Table(table =>
                            {
                                // Define columns
                                table.ColumnsDefinition(columns =>
                                {
                                    int totalColumns = 4; // Total number of columns
                                    for (int i = 0; i < totalColumns; i++)
                                    {
                                        columns.RelativeColumn(); // Distribute columns evenly
                                    }
                                });

                                // Add table headers
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Prod Date").Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Fe").Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Mn").Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Totals").Italic().Bold();
                                });

                                // Add table rows
                                foreach (var result in groupedResults)
                                {
                                    table.Cell().Element(CellStyle).Text(result.Date.ToString("dd/MMM/yyyy"));
                                    table.Cell().Element(CellStyle).Text(result.FeCount.ToString());
                                    table.Cell().Element(CellStyle).Text(result.MnCount.ToString());
                                    table.Cell().Element(CellStyle).Text((result.FeCount + result.MnCount).ToString());
                                }

                                // Add totals row
                                table.Cell().Element(CellStyle).Text("Totals:");
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.FeCount).ToString());
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.MnCount).ToString());
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.FeCount + gr.MnCount).ToString());
                            });

                        page.Footer().Height(50).Background(Colors.White)
                            .AlignCenter()
                            .Text($"Report generated on {DateTime.Now:dd-MMM-yyyy}");
                    });
                });



                // Generate PDF file
                byte[] pdfData;
                using (var stream = new MemoryStream())
                {
                    document.GeneratePdf(stream);
                    pdfData = stream.ToArray();
                }

                _logger.LogInformation("Report generated successfully from {StartDate} to {EndDate}", startDate, endDate);
                return pdfData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the report.");
                throw;
            }
        }

        // Define a reusable cell style method
        private static IContainer CellStyle(IContainer container) =>
            container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(2);


        // The following methods should be similarly converted to use QuestPDF
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

                // Define PDF document
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);

                        page.Header().Height(100).Background(Colors.White)
                            .AlignLeft()
                            .Row(row =>
                            {
                                row.ConstantItem(100).Image(Path.Combine(_env.WebRootPath, "img", "GMC-LOGO-1.png")); // Add logo
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text("GMC Daily Report").FontFamily("Times")
                                        .FontSize(18)
                                        .Bold()
                                        .AlignRight();
                                    column.Item().Text("Samples Received Statistics")
                                        .FontFamily("Times")
                                        .FontSize(18)
                                        .Bold()
                                        .AlignRight();
                                    column.Item().Text($"From {startDate:dd-MMM-yyyy} through {endDate:dd-MMM-yyyy}")
                                        .FontFamily("Times")
                                        .FontSize(10)
                                        .AlignRight();
                                });
                            });

                        page.Content().Background(Colors.Green.Lighten5)
                            .PaddingVertical(20)
                            .Table(table =>
                            {
                                // Define columns
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(); // First column for Production Date
                                    foreach (var plantSource in plantSources)
                                    {
                                        columns.RelativeColumn(); // Columns for each Plant Source
                                    }
                                    columns.RelativeColumn(); // Last column for Totals
                                });

                                // Add table headers
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Production Date").Italic().Bold();
                                    foreach (var plantSource in plantSources)
                                    {
                                        header.Cell().Element(CellStyle).Text(plantSource.PlantSourceName).Italic().Bold();
                                    }
                                    header.Cell().Element(CellStyle).Text("Totals").Italic().Bold();
                                });

                                // Add table rows
                                foreach (var result in groupedResults)
                                {
                                    table.Cell().Element(CellStyle).Text(result.Date.ToString("dd/MMM/yyyy"));
                                    foreach (var plantSource in plantSources)
                                    {
                                        table.Cell().Element(CellStyle).Text(result.PlantSourceCounts[plantSource.PlantSourceName].ToString());
                                    }
                                    table.Cell().Element(CellStyle).Text(result.PlantSourceCounts.Values.Sum().ToString());
                                }

                                // Add totals row
                                table.Cell().Element(CellStyle).Text("Totals:");
                                foreach (var plantSource in plantSources)
                                {
                                    table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.PlantSourceCounts[plantSource.PlantSourceName]).ToString());
                                }
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.PlantSourceCounts.Values.Sum()).ToString());
                            });

                        page.Footer().Height(50).Background(Colors.White)
                            .AlignCenter()
                            .Text($"Report generated on {DateTime.Now:dd-MMM-yyyy}");
                    });
                });

                // Generate PDF file
                byte[] pdfData;
                using (var stream = new MemoryStream())
                {
                    document.GeneratePdf(stream);
                    pdfData = stream.ToArray();
                }

                _logger.LogInformation("Report generated successfully from {StartDate} to {EndDate}", startDate, endDate);
                return pdfData;
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

                // Define PDF document
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);

                        page.Header().Height(100).Background(Colors.White)
                            .AlignLeft()
                            .Row(row =>
                            {
                                row.ConstantItem(100).Image(Path.Combine(_env.WebRootPath, "img", "GMC-LOGO-1.png")); // Add logo
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text("GMC Year-To-Date Report").FontFamily("Times")
                                        .FontSize(18)
                                        .Bold()
                                        .AlignRight();
                                    column.Item().Text("Samples Received Statistics")
                                        .FontFamily("Times")
                                        .FontSize(18)
                                        .Bold()
                                        .AlignRight();
                                    column.Item().Text($"From {startDate:MMM-yyyy} through {endDate:MMM-yyyy}")
                                        .FontFamily("Times")
                                        .FontSize(10)
                                        .AlignRight();
                                });
                            });

                        page.Content().Background(Colors.Green.Lighten5)
                            .PaddingVertical(20)
                            .Table(table =>
                            {
                                // Define columns
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(); // First column for Production Date
                                    foreach (var plantSource in plantSources)
                                    {
                                        columns.RelativeColumn(); // Columns for each Plant Source
                                    }
                                    columns.RelativeColumn(); // Last column for Totals
                                });

                                // Add table headers
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Month").Italic().Bold();
                                    foreach (var plantSource in plantSources)
                                    {
                                        header.Cell().Element(CellStyle).Text(plantSource.PlantSourceName).Italic().Bold();
                                    }
                                    header.Cell().Element(CellStyle).Text("Totals").Italic().Bold();
                                });

                                // Add table rows
                                foreach (var result in groupedResults)
                                {
                                    table.Cell().Element(CellStyle).Text(result.Month.ToString("MMM-yyyy"));
                                    foreach (var plantSource in plantSources)
                                    {
                                        table.Cell().Element(CellStyle).Text(result.PlantSourceCounts[plantSource.PlantSourceName].ToString());
                                    }
                                    table.Cell().Element(CellStyle).Text(result.PlantSourceCounts.Values.Sum().ToString());
                                }

                                // Add totals row
                                table.Cell().Element(CellStyle).Text("Totals:");
                                foreach (var plantSource in plantSources)
                                {
                                    table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.PlantSourceCounts[plantSource.PlantSourceName]).ToString());
                                }
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.PlantSourceCounts.Values.Sum()).ToString());
                            });

                        page.Footer().Height(50).Background(Colors.White)
                            .AlignCenter()
                            .Text($"Report generated on {DateTime.Now:dd-MMM-yyyy}");
                    });
                });

                // Generate PDF file
                byte[] pdfData;
                using (var stream = new MemoryStream())
                {
                    document.GeneratePdf(stream);
                    pdfData = stream.ToArray();
                }

                _logger.LogInformation("Report generated successfully from {StartDate} to {EndDate}", startDate, endDate);
                return pdfData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the report.");
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

                // Define PDF document
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);

                        page.Header().Height(100).Background(Colors.White)
                            .AlignLeft()
                            .Row(row =>
                            {
                                row.ConstantItem(100).Image(Path.Combine(_env.WebRootPath, "img", "GMC-LOGO-1.png")); // Add logo
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text("GMC Year-To-Date Report").FontFamily("Times")
                                        .FontSize(18)
                                        .Bold()
                                        .AlignRight();
                                    column.Item().Text("Analysis Statistics")
                                        .FontFamily("Times")
                                        .FontSize(18)
                                        .Bold()
                                        .AlignRight();
                                    column.Item().Text($"From {startDate:MMM-yyyy} through {endDate:MMM-yyyy}")
                                        .FontFamily("Times")
                                        .FontSize(10)
                                        .AlignRight();
                                });
                            });

                        page.Content().Background(Colors.Green.Lighten5)
                            .PaddingVertical(20)
                            .Table(table =>
                            {
                                // Define columns
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(); // First column for Production Date
                                    foreach (var plantSource in plantSources)
                                    {
                                        columns.RelativeColumn(); // Columns for each Plant Source
                                    }
                                    columns.RelativeColumn(); // Last column for Totals
                                });

                                // Add table headers
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Month").Italic().Bold();
                                    foreach (var plantSource in plantSources)
                                    {
                                        header.Cell().Element(CellStyle).Text(plantSource.PlantSourceName).Italic().Bold();
                                    }
                                    header.Cell().Element(CellStyle).Text("Totals").Italic().Bold();
                                });

                                // Add table rows
                                foreach (var result in groupedResults)
                                {
                                    table.Cell().Element(CellStyle).Text(result.Month.ToString("MMM-yyyy"));
                                    foreach (var plantSource in plantSources)
                                    {
                                        table.Cell().Element(CellStyle).Text(result.PlantSourceCounts[plantSource.PlantSourceName].ToString());
                                    }
                                    table.Cell().Element(CellStyle).Text(result.PlantSourceCounts.Values.Sum().ToString());
                                }

                                // Add totals row
                                table.Cell().Element(CellStyle).Text("Totals:");
                                foreach (var plantSource in plantSources)
                                {
                                    table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.PlantSourceCounts[plantSource.PlantSourceName]).ToString());
                                }
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.PlantSourceCounts.Values.Sum()).ToString());
                            });

                        page.Footer().Height(50).Background(Colors.White)
                            .AlignCenter()
                            .Text($"Report generated on {DateTime.Now:dd-MMM-yyyy}");
                    });
                });

                // Generate PDF file
                byte[] pdfData;
                using (var stream = new MemoryStream())
                {
                    document.GeneratePdf(stream);
                    pdfData = stream.ToArray();
                }

                _logger.LogInformation("Report generated successfully from {StartDate} to {EndDate}", startDate, endDate);
                return pdfData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the report.");
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

                // Define PDF document
                var document = Document.Create(container =>
                {

                    container.Page(page =>
                    {

                        page.Margin(30);

                        page.Header().Height(100).Background(Colors.White)
                            .AlignLeft()

                            .Row(row =>
                            {
                                row.ConstantItem(100).Image(Path.Combine(_env.WebRootPath, "img", "GMC-LOGO-1.png")); // Add logo
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text("GMC MET Report").FontFamily("Times")
                                        .FontSize(18)
                                        .Bold()
                                        .AlignRight();
                                    column.Item().Text("Analysis Statistics")
                                        .FontFamily("Times")
                                        .FontSize(18)
                                        .Bold()
                                        .AlignRight();
                                    column.Item().Text($"From {startDate:dd-MMM-yyyy} through {endDate:dd-MMM-yyyy}")
                                        .FontFamily("Times")
                                        .FontSize(10)
                                        .AlignRight();
                                });
                            });

                        page.Content().Background(Colors.Green.Lighten5)
                            .PaddingVertical(20)
                            .Table(table =>
                            {
                                // Define columns
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(); // First column for Prod Date
                                    columns.RelativeColumn(); // Column for Al2O3
                                    columns.RelativeColumn(); // Column for CaO
                                    columns.RelativeColumn(); // Column for Fe
                                    columns.RelativeColumn(); // Column for H2O
                                    columns.RelativeColumn(); // Column for Mg
                                    columns.RelativeColumn(); // Column for MgO
                                    columns.RelativeColumn(); // Column for Mn
                                    columns.RelativeColumn(); // Column for P
                                    columns.RelativeColumn(); // Column for SiO2
                                    columns.RelativeColumn(); // Last column for Totals
                                });

                                // Add table headers
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Prod Date").Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Al2O3").Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("CaO").Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Fe").Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("H2O").Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Mg").Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("MgO").Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Mn").Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("P").Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("SiO2").Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Totals").Italic().Bold();
                                });

                                // Add table rows
                                foreach (var result in groupedResults)
                                {
                                    table.Cell().Element(CellStyle).Text(result.Date.ToString("dd/MMM/yyyy"));
                                    table.Cell().Element(CellStyle).Text(result.Al2O3.ToString());
                                    table.Cell().Element(CellStyle).Text(result.CaO.ToString());
                                    table.Cell().Element(CellStyle).Text(result.Fe.ToString());
                                    table.Cell().Element(CellStyle).Text(result.H2O.ToString());
                                    table.Cell().Element(CellStyle).Text(result.Mg.ToString());
                                    table.Cell().Element(CellStyle).Text(result.MgO.ToString());
                                    table.Cell().Element(CellStyle).Text(result.Mn.ToString());
                                    table.Cell().Element(CellStyle).Text(result.P.ToString());
                                    table.Cell().Element(CellStyle).Text(result.SiO2.ToString());
                                    table.Cell().Element(CellStyle).Text(result.Total.ToString());
                                }

                                // Add totals row
                                table.Cell().Element(CellStyle).Text("Totals:");
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.Al2O3).ToString());
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.CaO).ToString());
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.Fe).ToString());
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.H2O).ToString());
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.Mg).ToString());
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.MgO).ToString());
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.Mn).ToString());
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.P).ToString());
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.SiO2).ToString());
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.Total).ToString());
                            });

                        page.Footer().Height(50).Background(Colors.White)
                            .AlignCenter()
                            .Text($"Report generated on {DateTime.Now:dd-MMM-yyyy}");
                    });
                });

                // Generate PDF file
                byte[] pdfData;
                using (var stream = new MemoryStream())
                {
                    document.GeneratePdf(stream);
                    pdfData = stream.ToArray();
                }

                _logger.LogInformation("Report generated successfully from {StartDate} to {EndDate}", startDate, endDate);
                return pdfData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the report.");
                throw;
            }
        }


        public async Task<byte[]> GeneratePlantReportAsync(DateTime startDate, DateTime endDate, List<int> selectedPlantIds)
        {
            try
            {
                // Fetch data from the database based on selected plants
                var labRequests = await _context.LabRequests
                    .Where(lr => lr.ProductionDate >= startDate && lr.ProductionDate <= endDate && selectedPlantIds.Contains(lr.PlantSourceId))
                    .Include(lr => lr.LabResults)
                    .Include(lr => lr.PlantSource)
                    .ToListAsync();

                var plantSources = await _context.PlantSources.Where(ps => selectedPlantIds.Contains(ps.Id)).ToListAsync();

                var groupedResults = labRequests.GroupBy(lr => lr.ProductionDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        PlantSourceCounts = plantSources.ToDictionary(ps => ps.PlantSourceName, ps => g.Sum(lr => lr.LabResults.Count(res => lr.PlantSourceId == ps.Id)))
                    }).ToList();

                // Define PDF document
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);

                        page.Header().Height(100).Background(Colors.White)
                            .AlignLeft()
                            .Row(row =>
                            {
                                row.ConstantItem(100).Image(Path.Combine(_env.WebRootPath, "img", "GMC-LOGO-1.png")); // Add logo
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text("GMC Plant Report").FontFamily("Times")
                                        .FontSize(18)
                                        .Bold()
                                        .AlignRight();
                                    column.Item().Text("Analysis Statistics")
                                        .FontFamily("Times")
                                        .FontSize(18)
                                        .Bold()
                                        .AlignRight();
                                    column.Item().Text($"From {startDate:dd-MMM-yyyy} through {endDate:dd-MMM-yyyy}")
                                        .FontFamily("Times")
                                        .FontSize(10)
                                        .AlignRight();
                                });
                            });

                        page.Content().Background(Colors.Green.Lighten5)
                            .PaddingVertical(20)
                            .Table(table =>
                            {
                                // Define columns
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(); // First column for Production Date
                                    foreach (var plantSource in plantSources)
                                    {
                                        columns.RelativeColumn(); // Columns for each selected Plant Source
                                    }
                                    columns.RelativeColumn(); // Last column for Totals
                                });

                                // Add table headers
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Production Date").Italic().Bold();
                                    foreach (var plantSource in plantSources)
                                    {
                                        header.Cell().Element(CellStyle).Text(plantSource.PlantSourceName).Italic().Bold();
                                    }
                                    header.Cell().Element(CellStyle).Text("Totals").Italic().Bold();
                                });

                                // Add table rows
                                foreach (var result in groupedResults)
                                {
                                    table.Cell().Element(CellStyle).Text(result.Date.ToString("dd/MMM/yyyy"));
                                    foreach (var plantSource in plantSources)
                                    {
                                        table.Cell().Element(CellStyle).Text(result.PlantSourceCounts[plantSource.PlantSourceName].ToString());
                                    }
                                    table.Cell().Element(CellStyle).Text(result.PlantSourceCounts.Values.Sum().ToString());
                                }

                                // Add totals row
                                table.Cell().Element(CellStyle).Text("Totals:");
                                foreach (var plantSource in plantSources)
                                {
                                    table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.PlantSourceCounts[plantSource.PlantSourceName]).ToString());
                                }
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.PlantSourceCounts.Values.Sum()).ToString());
                            });

                        page.Footer().Height(50).Background(Colors.White)
                            .AlignCenter()
                            .Text($"Report generated on {DateTime.Now:dd-MMM-yyyy}");
                    });
                });

                // Generate PDF file
                byte[] pdfData;
                using (var stream = new MemoryStream())
                {
                    document.GeneratePdf(stream);
                    pdfData = stream.ToArray();
                }

                _logger.LogInformation("Plant report generated successfully from {StartDate} to {EndDate}", startDate, endDate);
                return pdfData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the plant report.");
                throw;
            }
        }


    }

}
