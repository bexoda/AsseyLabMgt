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
using Newtonsoft.Json;

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
        // Define a reusable cell style method
        private static IContainer CellStyle(IContainer container) =>
            container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(2);


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

                        // Define the header of the PDF
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

                        // Define the content of the PDF
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
                                    header.Cell().Element(CellStyle).Text("Prod Date").FontSize(8).Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Fe").FontSize(8).Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Mn").FontSize(8).Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Totals").FontSize(8).Italic().Bold();
                                });

                                // Add table rows
                                foreach (var result in groupedResults)
                                {
                                    table.Cell().Element(CellStyle).Text(result.Date.ToString("dd/MMM/yyyy")).FontSize(8);
                                    table.Cell().Element(CellStyle).Text(result.FeCount.ToString()).FontSize(8);
                                    table.Cell().Element(CellStyle).Text(result.MnCount.ToString()).FontSize(8);
                                    table.Cell().Element(CellStyle).Text((result.FeCount + result.MnCount).ToString()).FontSize(8);
                                }

                                // Add totals row
                                table.Cell().Element(CellStyle).Text("Totals:").FontSize(8);
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.FeCount).ToString()).FontSize(8);
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.MnCount).ToString()).FontSize(8);
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.FeCount + gr.MnCount).ToString()).FontSize(8);
                            });

                        // Define the footer of the PDF
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
                _logger.LogInformation("Generating MET report from {StartDate} to {EndDate}", startDate, endDate);

                // Fetch data from the database
                var labResults = await _context.LabResults
                    .Where(lr => lr.CreatedDate >= startDate && lr.CreatedDate <= endDate)
                    .Include(lr => lr.LabRequest) // Ensure LabRequest is included to access ProductionDate
                    .ToListAsync();

                _logger.LogInformation("Fetched {Count} lab results between {StartDate} and {EndDate}", labResults.Count, startDate, endDate);

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

                _logger.LogInformation("Grouped results: {GroupedResults}", JsonConvert.SerializeObject(groupedResults));

                // Define PDF document
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30);

                        // Define the header of the PDF
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

                        // Define the content of the PDF
                        page.Content().Background(Colors.Green.Lighten5)
                            .PaddingVertical(20)
                            .Table(table =>
                            {
                                // Define columns
                                table.ColumnsDefinition(columns =>
                                {
                                    int totalColumns = 11; // Total number of columns
                                    for (int i = 0; i < totalColumns; i++)
                                    {
                                        columns.RelativeColumn(); // Distribute columns evenly
                                    }
                                });

                                // Add table headers
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Prod Date").FontSize(8).Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Al2O3").FontSize(8).Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("CaO").FontSize(8).Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Fe").FontSize(8).Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("H2O").FontSize(8).Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Mg").FontSize(8).Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("MgO").FontSize(8).Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Mn").FontSize(8).Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("P").FontSize(8).Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("SiO2").FontSize(8).Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Totals").FontSize(8).Italic().Bold();
                                });

                                // Add table rows
                                foreach (var result in groupedResults)
                                {
                                    table.Cell().Element(CellStyle).Text(result.Date.ToString("dd/MMM/yyyy")).FontSize(8);
                                    table.Cell().Element(CellStyle).Text(result.Al2O3.ToString()).FontSize(8);
                                    table.Cell().Element(CellStyle).Text(result.CaO.ToString()).FontSize(8);
                                    table.Cell().Element(CellStyle).Text(result.Fe.ToString()).FontSize(8);
                                    table.Cell().Element(CellStyle).Text(result.H2O.ToString()).FontSize(8);
                                    table.Cell().Element(CellStyle).Text(result.Mg.ToString()).FontSize(8);
                                    table.Cell().Element(CellStyle).Text(result.MgO.ToString()).FontSize(8);
                                    table.Cell().Element(CellStyle).Text(result.Mn.ToString()).FontSize(8);
                                    table.Cell().Element(CellStyle).Text(result.P.ToString()).FontSize(8);
                                    table.Cell().Element(CellStyle).Text(result.SiO2.ToString()).FontSize(8);
                                    table.Cell().Element(CellStyle).Text(result.Total.ToString()).FontSize(8);
                                }

                                // Add totals row
                                table.Cell().Element(CellStyle).Text("Totals:").FontSize(8);
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.Al2O3).ToString()).FontSize(8);
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.CaO).ToString()).FontSize(8);
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.Fe).ToString()).FontSize(8);
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.H2O).ToString()).FontSize(8);
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.Mg).ToString()).FontSize(8);
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.MgO).ToString()).FontSize(8);
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.Mn).ToString()).FontSize(8);
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.P).ToString()).FontSize(8);
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.SiO2).ToString()).FontSize(8);
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(r => r.Total).ToString()).FontSize(8);
                            });

                        // Define the footer of the PDF
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



        // The following methods should be similarly converted to use QuestPDF
        public async Task<byte[]> GenerateSamplesReceivedReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Fetch lab requests within the date range
                var labRequests = await _context.LabRequests
                    .Where(lr => lr.ProductionDate >= startDate && lr.ProductionDate <= endDate)
                    .Include(lr => lr.PlantSource)
                    .Select(lr => new { lr.ProductionDate, lr.NumberOfSamples, lr.PlantSource.PlantSourceName })
                    .ToListAsync();

                // Fetch all plant sources
                var plantSources = await _context.PlantSources.ToListAsync();

                // Group by ProductionDate and then by PlantSource, summing NumberOfSamples
                var groupedResults = labRequests
                    .GroupBy(lr => lr.ProductionDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        PlantSourceCounts = plantSources.ToDictionary(
                            ps => ps.PlantSourceName,
                            ps => g.Where(lr => lr.PlantSourceName == ps.PlantSourceName).Sum(lr => lr.NumberOfSamples))
                    })
                    .ToList();

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
                                    header.Cell().Element(CellStyle).Text("Production Date").Italic().Bold().FontSize(8);
                                    foreach (var plantSource in plantSources)
                                    {
                                        header.Cell().Element(CellStyle).Text(plantSource.PlantSourceName).Italic().Bold().FontSize(8);
                                    }
                                    header.Cell().Element(CellStyle).Text("Totals").Italic().Bold().FontSize(8);
                                });

                                // Add table rows
                                foreach (var result in groupedResults)
                                {
                                    table.Cell().Element(CellStyle).Text(result.Date.ToString("dd/MMM/yyyy")).FontSize(8);
                                    foreach (var plantSource in plantSources)
                                    {
                                        table.Cell().Element(CellStyle).Text(result.PlantSourceCounts[plantSource.PlantSourceName].ToString()).FontSize(8);
                                    }
                                    table.Cell().Element(CellStyle).Text(result.PlantSourceCounts.Values.Sum().ToString()).FontSize(8);
                                }

                                // Add totals row
                                table.Cell().Element(CellStyle).Text("Totals:").FontSize(8);
                                foreach (var plantSource in plantSources)
                                {
                                    table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.PlantSourceCounts[plantSource.PlantSourceName]).ToString()).FontSize(8);
                                }
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.PlantSourceCounts.Values.Sum()).ToString()).FontSize(8);
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
                // Fetch lab requests within the date range
                var labRequests = await _context.LabRequests
                    .Where(lr => lr.ProductionDate >= startDate && lr.ProductionDate <= endDate)
                    .Include(lr => lr.PlantSource)
                    .Select(lr => new { lr.ProductionDate, lr.NumberOfSamples, lr.PlantSource.PlantSourceName })
                    .ToListAsync();

                // Fetch all plant sources
                var plantSources = await _context.PlantSources.ToListAsync();

                // Group by Year and Month, then by PlantSource, summing NumberOfSamples
                var groupedResults = labRequests
                    .GroupBy(lr => new { lr.ProductionDate.Year, lr.ProductionDate.Month })
                    .Select(g => new
                    {
                        Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                        PlantSourceCounts = plantSources.ToDictionary(
                            ps => ps.PlantSourceName,
                            ps => g.Where(lr => lr.PlantSourceName == ps.PlantSourceName).Sum(lr => lr.NumberOfSamples))
                    })
                    .ToList();

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
                                    columns.RelativeColumn(); // First column for Month
                                    foreach (var plantSource in plantSources)
                                    {
                                        columns.RelativeColumn(); // Columns for each Plant Source
                                    }
                                    columns.RelativeColumn(); // Last column for Totals
                                });

                                // Add table headers
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Month").Italic().Bold().FontSize(8);
                                    foreach (var plantSource in plantSources)
                                    {
                                        header.Cell().Element(CellStyle).Text(plantSource.PlantSourceName).Italic().Bold().FontSize(8);
                                    }
                                    header.Cell().Element(CellStyle).Text("Totals").FontSize(8).Italic().Bold();
                                });

                                // Add table rows
                                foreach (var result in groupedResults)
                                {
                                    table.Cell().Element(CellStyle).Text(result.Month.ToString("MMM-yyyy")).FontSize(8);
                                    foreach (var plantSource in plantSources)
                                    {
                                        table.Cell().Element(CellStyle).Text(result.PlantSourceCounts[plantSource.PlantSourceName].ToString()).FontSize(8);
                                    }
                                    table.Cell().Element(CellStyle).Text(result.PlantSourceCounts.Values.Sum().ToString()).FontSize(8);
                                }

                                // Add totals row
                                table.Cell().Element(CellStyle).Text("Totals:").FontSize(8);
                                foreach (var plantSource in plantSources)
                                {
                                    table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.PlantSourceCounts[plantSource.PlantSourceName]).ToString()).FontSize(8);
                                }
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.PlantSourceCounts.Values.Sum()).ToString()).FontSize(8);
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
                _logger.LogInformation("Generating year-to-date analysis statistics report from {StartDate} to {EndDate}", startDate, endDate);

                // Fetch lab requests within the date range and their associated lab results and plant sources
                var labRequests = await _context.LabRequests
                    .Where(lr => lr.ProductionDate >= startDate && lr.ProductionDate <= endDate)
                    .Include(lr => lr.LabResults)
                    .Include(lr => lr.PlantSource)
                    .Select(lr => new { lr.ProductionDate, lr.LabResults, lr.PlantSource.PlantSourceName })
                    .ToListAsync();

                _logger.LogInformation("Fetched {Count} lab requests between {StartDate} and {EndDate}", labRequests.Count, startDate, endDate);

                // Fetch all plant sources
                var plantSources = await _context.PlantSources.ToListAsync();

                // Group by Year and Month, then by PlantSource, summing relevant lab results for each date
                var groupedResults = labRequests
                    .GroupBy(lr => new { lr.ProductionDate.Year, lr.ProductionDate.Month })
                    .Select(g => new
                    {
                        Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                        PlantSourceCounts = plantSources.ToDictionary(
                            ps => ps.PlantSourceName,
                            ps => g.Where(lr => lr.PlantSourceName == ps.PlantSourceName)
                                   .SelectMany(lr => lr.LabResults)
                                   .Count(lr => lr.Al2O3 > 0 || lr.CaO > 0 || lr.Fe > 0 || lr.H2O > 0 || lr.Mg > 0 || lr.MgO > 0 || lr.Mn > 0 || lr.B > 0 || lr.SiO2 > 0)) // Assuming Al2O3, CaO, etc. as relevant fields
                    })
                    .ToList();

                _logger.LogInformation("Grouped results: {GroupedResults}", JsonConvert.SerializeObject(groupedResults));

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
                                    header.Cell().Element(CellStyle).Text("Month").Italic().Bold().FontSize(8);
                                    foreach (var plantSource in plantSources)
                                    {
                                        header.Cell().Element(CellStyle).Text(plantSource.PlantSourceName).FontSize(8).Italic().Bold();
                                    }
                                    header.Cell().Element(CellStyle).Text("Totals").FontSize(8).Italic().FontSize(8).Bold();
                                });

                                // Add table rows
                                foreach (var result in groupedResults)
                                {
                                    table.Cell().Element(CellStyle).Text(result.Month.ToString("MMM-yyyy")).FontSize(8).FontSize(8);
                                    foreach (var plantSource in plantSources)
                                    {
                                        table.Cell().Element(CellStyle).Text(result.PlantSourceCounts[plantSource.PlantSourceName].ToString()).FontSize(8);
                                    }
                                    table.Cell().Element(CellStyle).Text(result.PlantSourceCounts.Values.Sum().ToString()).FontSize(8);
                                }

                                // Add totals row
                                table.Cell().Element(CellStyle).Text("Totals:").FontSize(8);
                                foreach (var plantSource in plantSources)
                                {
                                    table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.PlantSourceCounts[plantSource.PlantSourceName]).ToString()).FontSize(8);
                                }
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.PlantSourceCounts.Values.Sum()).ToString()).FontSize(8);
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
                _logger.LogInformation("Generating plant report from {StartDate} to {EndDate} for selected plants: {SelectedPlantIds}", startDate, endDate, string.Join(", ", selectedPlantIds));

                // Fetch lab requests within the date range and for the selected plants
                var labRequests = await _context.LabRequests
                    .Where(lr => lr.ProductionDate >= startDate && lr.ProductionDate <= endDate && selectedPlantIds.Contains(lr.PlantSourceId))
                    .Include(lr => lr.LabResults)
                    .Include(lr => lr.PlantSource)
                    .ToListAsync();

                _logger.LogInformation("Fetched {Count} lab requests between {StartDate} and {EndDate} for selected plants", labRequests.Count, startDate, endDate);

                // Fetch the selected plant sources
                var plantSources = await _context.PlantSources.Where(ps => selectedPlantIds.Contains(ps.Id)).ToListAsync();

                // Group by ProductionDate and sum the NumberOfSamples for each selected PlantSource
                var groupedResults = labRequests
                    .GroupBy(lr => lr.ProductionDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        PlantSourceCounts = plantSources.ToDictionary(
                            ps => ps.PlantSourceName,
                            ps => g.Where(lr => lr.PlantSourceId == ps.Id).Sum(lr => lr.NumberOfSamples))
                    })
                    .ToList();

                _logger.LogInformation("Grouped results: {GroupedResults}", JsonConvert.SerializeObject(groupedResults));

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
                                    header.Cell().Element(CellStyle).Text("Production Date").FontSize(8).Italic().Bold();
                                    foreach (var plantSource in plantSources)
                                    {
                                        header.Cell().Element(CellStyle).Text(plantSource.PlantSourceName).FontSize(8).Italic().Bold();
                                    }
                                    header.Cell().Element(CellStyle).Text("Totals").FontSize(8).Italic().Bold();
                                });

                                // Add table rows
                                foreach (var result in groupedResults)
                                {
                                    table.Cell().Element(CellStyle).Text(result.Date.ToString("dd/MMM/yyyy")).FontSize(8);
                                    foreach (var plantSource in plantSources)
                                    {
                                        table.Cell().Element(CellStyle).Text(result.PlantSourceCounts[plantSource.PlantSourceName].ToString()).FontSize(8);
                                    }
                                    table.Cell().Element(CellStyle).Text(result.PlantSourceCounts.Values.Sum().ToString()).FontSize(8);
                                }

                                // Add totals row
                                table.Cell().Element(CellStyle).Text("Totals:").FontSize(8);
                                foreach (var plantSource in plantSources)
                                {
                                    table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.PlantSourceCounts[plantSource.PlantSourceName]).ToString()).FontSize(8);
                                }
                                table.Cell().Element(CellStyle).Text(groupedResults.Sum(gr => gr.PlantSourceCounts.Values.Sum()).ToString()).FontSize(8)    ;
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
