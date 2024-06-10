using AsseyLabMgt.Data;
using AsseyLabMgt.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AsseyLabMgt.Services
{
    public class ReportService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ReportService> _logger;
        private readonly ApplicationDbContext _context;

        public ReportService(IWebHostEnvironment env, ILogger<ReportService> logger, ApplicationDbContext context)
        {
            _env = env;
            _logger = logger;
            _context = context;
        }

        // Define a reusable cell style method


        public async Task<byte[]> GenerateGeologyReportAsync(DateTime startDate, DateTime? endDate, List<string> elements)
        {
            try
            {
                _logger.LogInformation("Starting report generation from {StartDate} to {EndDate}", startDate, endDate);

                var geologyDepartmentId = await _context.Departments
                    .Where(d => d.DeptName.Contains("Geology"))
                    .Select(d => d.Id)
                    .FirstOrDefaultAsync();

                if (geologyDepartmentId == 0)
                {
                    throw new InvalidOperationException("Geology department not found.");
                }

                _logger.LogInformation("Geology department found with ID {GeologyDepartmentId}", geologyDepartmentId);

                // Fetch data from the database
                var labResults = await _context.LabResults
                    .Where(lr => lr.LabRequest.ProductionDate >= startDate && lr.LabRequest.ProductionDate <= endDate && lr.LabRequest.DepartmentId == geologyDepartmentId)
                    .Include(lr => lr.LabRequest)
                    .ToListAsync();

                _logger.LogInformation("Fetched {LabResultsCount} lab results for the specified date range.", labResults.Count);

                // If elements list is empty or contains only null values, select all elements
                if (elements == null || !elements.Any() || elements.All(e => string.IsNullOrEmpty(e)))
                {
                    elements = typeof(LabResults).GetProperties()
                        .Where(prop => prop.PropertyType == typeof(decimal?))
                        .Select(prop => prop.Name)
                        .ToList();

                    _logger.LogInformation("No elements specified. Defaulting to all elements.");
                }
                else
                {
                    // If elements are passed as a comma-separated string, convert it to a list
                    elements = elements.SelectMany(e => e.Split(',')).ToList();
                    _logger.LogInformation("Elements specified: {Elements}", string.Join(", ", elements));
                }

                // Define PDF document
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(50);

                        // Define the header of the PDF
                        page.Header().Height(100).Background(Colors.White)
                             .AlignLeft()
                             .Column(column =>
                             {
                                 column.Item().Row(row =>
                                 {
                                     row.ConstantItem(100).Image(Path.Combine(_env.WebRootPath, "img", "GMC-LOGO-1.png")); // Add logo
                                     row.RelativeItem().Column(innerColumn =>
                                     {
                                         innerColumn.Item().Text("GMC Geology Report").FontFamily("Times")
                                             .FontSize(18)
                                             .Bold()
                                             .AlignRight();
                                         if (labResults.Any())
                                         {
                                             var productionDate = labResults.First().LabRequest.ProductionDate;
                                             var dateReported = labResults.First().LabRequest.DateReported;

                                             innerColumn.Item().Text($"Production Date: {productionDate:dd/MMM/yyyy}").FontFamily("Times").FontSize(12).AlignRight(); ;
                                             innerColumn.Item().Text($"Date Reported: {dateReported:dd/MMM/yyyy}").FontFamily("Times").FontSize(12).AlignRight(); ;
                                         }
                                     });
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
                                    columns.RelativeColumn(); // First column for Prod Date
                                    foreach (var element in elements)
                                    {
                                        columns.RelativeColumn(); // Columns for each selected element
                                    }
                                    columns.RelativeColumn(); // Column for Totals
                                });

                                // Add table headers
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Sample Identification").FontSize(8).Italic().Bold();
                                    foreach (var element in elements)
                                    {
                                        header.Cell().Element(CellStyle).Text(element).FontSize(8).Italic().Bold();
                                    }
                                    header.Cell().Element(CellStyle).Text("Totals").FontSize(8).Italic().Bold();
                                });

                                // Add table rows
                                foreach (var result in labResults)
                                {
                                    table.Cell().Element(CellStyle).Text(result.SampleId).FontSize(8);
                                    foreach (var element in elements)
                                    {
                                        var value = GetElementValue(result, element);
                                        table.Cell().Element(CellStyle).Text(value?.ToString("F2") ?? "0").FontSize(8);
                                    }
                                    var total = elements.Sum(element => GetElementValue(result, element) ?? 0);
                                    table.Cell().Element(CellStyle).Text(total.ToString("F2")).FontSize(8);
                                }

                                // Add totals row
                                table.Cell().Element(CellStyle).Text("Totals:").FontSize(8).Italic().Bold();
                                foreach (var element in elements)
                                {
                                    // Count the number of lab results where the element value is greater than zero
                                    var count = labResults.Count(lr => (GetElementValue(lr, element) ?? 0) > 0);

                                    // Add the count to the table cell
                                    table.Cell().Element(CellStyle).Text(count.ToString("F2")).FontSize(8).Italic().Bold();
                                }

                                // Calculate the grand total
                                var grandTotal = labResults.Count(result => elements.Any(element => (GetElementValue(result, element) ?? 0) > 0));

                                // Add the grand total to the table cell
                                table.Cell().Element(CellStyle).Text(grandTotal.ToString("F2")).FontSize(8).Italic().Bold();

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



        private decimal? GetElementValue(LabResults result, string elementName)
        {
            return elementName switch
            {
                "Mn" => result.Mn,
                "Fe" => result.Fe,
                "SiO2" => result.SiO2,
                "B" => result.B,
                "Sol_Mn" => result.Sol_Mn,
                "MnO2" => result.MnO2,
                "P" => result.P,
                "Al2O3" => result.Al2O3,
                "Mg" => result.Mg,
                "CaO" => result.CaO,
                "Au" => result.Au,
                "H2O" => result.H2O,
                "MgO" => result.MgO,
                "As" => result.As,
                _ => null
            };
        }

        private static IContainer CellStyle(IContainer container) =>
            container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(2);

        public async Task<byte[]> GenerateMetReportAsync(DateTime startDate, DateTime endDate, List<string> elements, string description, string? jobNumber)
        {
            try
            {
                _logger.LogInformation("Starting MET report generation from {StartDate} to {EndDate} for Job Number: {JobNumber}", startDate, endDate, jobNumber);

                // Create the base query
                var labResultsQuery = _context.LabResults
                    .Where(lr => lr.LabRequest.ProductionDate >= startDate && lr.LabRequest.ProductionDate <= endDate)
                    .Include(lr => lr.LabRequest)
                    .OrderBy(lr => lr.LabRequest.ProductionDate)
                    .AsQueryable();

                // Add job number filter if provided
                if (!string.IsNullOrEmpty(jobNumber))
                {
                    labResultsQuery = labResultsQuery.Where(lr => lr.LabRequest.JobNumber == jobNumber);
                }

                // Fetch lab results
                var labResults = await labResultsQuery.ToListAsync();

                if (!labResults.Any())
                {
                    _logger.LogWarning("No lab results found for the given date range and job number.");
                    throw new InvalidOperationException("No lab results found for the given date range and job number.");
                }

                // If elements list is empty or contains only null values, select all elements
                if (elements == null || !elements.Any() || elements.All(e => string.IsNullOrEmpty(e)))
                {
                    elements = typeof(LabResults).GetProperties()
                        .Where(prop => prop.PropertyType == typeof(decimal?))
                        .Select(prop => prop.Name)
                        .ToList();
                    _logger.LogInformation("Elements not specified, selecting all elements: {Elements}", string.Join(", ", elements));
                }
                else
                {
                    // If elements are passed as a comma-separated string, convert it to a list
                    elements = elements.SelectMany(e => e.Split(',')).ToList();
                    _logger.LogInformation("Elements specified: {Elements}", string.Join(", ", elements));
                }

                // Define PDF document
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape()); // Set the page size to A4 landscape
                        page.Margin(50);

                        // Define the header of the PDF
                        page.Header().Height(100).Background(Colors.White)
                            .AlignLeft()
                            .Column(column =>
                            {
                                column.Item().Row(row =>
                                {
                                    row.ConstantItem(100).Image(Path.Combine(_env.WebRootPath, "img", "GMC-LOGO-1.png")); // Add logo
                                    row.RelativeItem().Column(innerColumn =>
                                    {
                                        innerColumn.Item().Text("GMC Met Report").FontFamily("Times")
                                            .FontSize(18)
                                            .Bold()
                                            .AlignRight();
                                    });
                                });

                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(innerColumn =>
                                    {
                                        innerColumn.Item().Text(description).FontFamily("Times")
                                            .FontSize(18)
                                            .Bold()
                                            .AlignLeft()
                                            .Underline();
                                    });

                                    row.RelativeItem().Column(innerColumn =>
                                    {
                                        if (!string.IsNullOrEmpty(jobNumber))
                                        {
                                            innerColumn.Item().Text($"Job Number: {jobNumber}").FontFamily("Times").FontSize(12).AlignRight();
                                        }
                                        if (labResults.Any())
                                        {
                                            var productionDate = labResults.First().LabRequest.ProductionDate;
                                            innerColumn.Item().Text($"Production Date: {productionDate:dd-MMM-yyyy}").FontFamily("Times").FontSize(12).AlignRight();
                                        }
                                    });
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
                                    columns.RelativeColumn(); // First column for Sample ID
                                    foreach (var element in elements)
                                    {
                                        columns.RelativeColumn(); // Columns for each selected element
                                    }
                                });

                                // Add table headers
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Sample Identification").FontSize(8).Italic().Bold();
                                    foreach (var element in elements)
                                    {
                                        header.Cell().Element(CellStyle).Text("%" + element).FontSize(8).Italic().Bold();
                                    }
                                });

                                // Add table rows and counts for each lab result
                                foreach (var result in labResults)
                                {
                                    table.Cell().Element(CellStyle).Text(result.SampleId).FontSize(8);
                                    foreach (var element in elements)
                                    {
                                        var value = GetElementValue(result, element);
                                        table.Cell().Element(CellStyle).Text(value?.ToString("F2") ?? "0").FontSize(8);
                                    }
                                }

                                // Add grand totals row
                                table.Cell().Element(CellStyle).Text("Grand Totals:").FontSize(8).Italic().Bold();
                                foreach (var element in elements)
                                {
                                    var count = labResults.Count(lr => GetElementValue(lr, element) != null);
                                    table.Cell().Element(CellStyle).Text(count.ToString()).FontSize(8).Italic().Bold();
                                }
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

                _logger.LogInformation("MET report generated successfully from {StartDate} to {EndDate} for Job Number: {JobNumber}", startDate, endDate, jobNumber);
                return pdfData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the MET report: {Message}", ex.Message);
                throw;
            }
        }

        public List<SelectListItem> GetElementNames()
        {
            var elementNames = typeof(LabResults).GetProperties()
                .Where(prop => prop.PropertyType == typeof(decimal?))
                .Select(prop => new SelectListItem
                {
                    Value = prop.Name,
                    Text = prop.Name
                })
                .ToList();

            return elementNames;
        }

        public List<string> SearchJobNumbers(string term)
        {
            return _context.LabRequests
                .Where(lr => lr.JobNumber.Contains(term))
                .Select(lr => lr.JobNumber)
                .Distinct()
                .ToList();
        }

        public async Task<byte[]> GenerateDailyReportAsync(DateTime startDate, List<int> selectedPlantIds, List<string> elements)
        {
            try
            {
                _logger.LogInformation("Starting daily report generation for date: {StartDate}", startDate);

                // Fetch lab results based on the selected plant IDs and the start date
                var labResults = await _context.LabResults
                    .Where(lr => lr.LabRequest.ProductionDate == startDate && selectedPlantIds.Contains(lr.LabRequest.PlantSourceId))
                    .Include(lr => lr.LabRequest)
                    .ThenInclude(lr => lr.PlantSource)
                    .OrderBy(lr => lr.LabRequest.PlantSource.PlantSourceName)
                    .ToListAsync();

                if (!labResults.Any())
                {
                    _logger.LogWarning("No lab results found for the given date and plant sources.");
                    throw new InvalidOperationException("No lab results found for the given date and plant sources.");
                }

                // If elements list is empty or contains only null values, select all elements
                if (elements == null || !elements.Any() || elements.All(e => string.IsNullOrEmpty(e)))
                {
                    elements = typeof(LabResults).GetProperties()
                        .Where(prop => prop.PropertyType == typeof(decimal?))
                        .Select(prop => prop.Name)
                        .ToList();
                    _logger.LogInformation("Elements not specified, selecting all elements: {Elements}", string.Join(", ", elements));
                }
                else
                {
                    // If elements are passed as a comma-separated string, convert it to a list
                    elements = elements.SelectMany(e => e.Split(',')).ToList();
                    _logger.LogInformation("Elements specified: {Elements}", string.Join(", ", elements));
                }

                // Group the lab results by plant source
                var groupedResults = labResults
                    .Where(lr => lr.LabRequest?.PlantSource != null)
                    .GroupBy(lr => lr.LabRequest.PlantSource.PlantSourceName);

                if (!groupedResults.Any())
                {
                    _logger.LogWarning("No lab results were grouped by plant source.");
                    throw new InvalidOperationException("No lab results were grouped by plant source.");
                }

                // Define PDF document
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape()); // Set the page size to A4 landscape
                        page.Margin(50);

                        // Define the header of the PDF
                        page.Header().Height(100).Background(Colors.White)
                            .AlignLeft()
                            .Column(column =>
                            {
                                column.Item().Row(row =>
                                {
                                    row.ConstantItem(100).Image(Path.Combine(_env.WebRootPath, "img", "GMC-LOGO-1.png")); // Add logo
                                    row.RelativeItem().Column(innerColumn =>
                                    {
                                        innerColumn.Item().Text("GMC Daily Assays").FontFamily("Times")
                                            .FontSize(18)
                                            .Bold()
                                            .AlignRight();
                                        if (labResults.Any())
                                        {
                                            var productionDate = labResults.First().LabRequest.ProductionDate;
                                            var dateReported = labResults.First().LabRequest.DateReported;

                                            innerColumn.Item().Text($"Production Date: {productionDate:dd-MMM-yyyy}").FontFamily("Times").FontSize(12).AlignRight();
                                            innerColumn.Item().Text($"Date Reported: {dateReported:dd-MMM-yyyy}").FontFamily("Times").FontSize(12).AlignRight();
                                        }
                                    });
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
                                    columns.RelativeColumn(); // First column for Plant Source
                                    columns.RelativeColumn(); // Column for Time
                                    columns.RelativeColumn(); // Column for Sample ID
                                    foreach (var element in elements)
                                    {
                                        columns.RelativeColumn(); // Columns for each selected element
                                    }
                                });

                                // Add table headers
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Plant/Source").FontSize(8).Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Time").FontSize(8).Italic().Bold();
                                    header.Cell().Element(CellStyle).Text("Sample Identification").FontSize(8).Italic().Bold();
                                    foreach (var element in elements)
                                    {
                                        header.Cell().Element(CellStyle).Text(element).FontSize(8).Italic().Bold();
                                    }
                                });

                                // Add table rows and totals for each plant source
                                foreach (var group in groupedResults)
                                {
                                    decimal[] plantTotals = new decimal[elements.Count];
                                    _logger.LogInformation("Processing group for Plant Source: {PlantSource}", group.Key);

                                    foreach (var result in group)
                                    {
                                        table.Cell().Element(CellStyle).Text(result.LabRequest.PlantSource.PlantSourceName).FontSize(8);
                                        table.Cell().Element(CellStyle).Text(result.LabRequest.TimeReceived.ToString("HH:mm")).FontSize(8);
                                        table.Cell().Element(CellStyle).Text(result.SampleId).FontSize(8);

                                        for (int i = 0; i < elements.Count; i++)
                                        {
                                            var element = elements[i];
                                            var value = GetElementValue(result, element);
                                            plantTotals[i] += value ?? 0;
                                            table.Cell().Element(CellStyle).Text(value?.ToString("F2") ?? "0").FontSize(8);
                                        }
                                    }

                                    // Add totals row for each plant source
                                    table.Cell().Element(CellStyle).Text("Totals:").FontSize(8).Italic().Bold();
                                    table.Cell().Element(CellStyle).Text("-").FontSize(8).Italic().Bold();
                                    table.Cell().Element(CellStyle).Text("-").FontSize(8).Italic().Bold();
                                    foreach (var total in plantTotals)
                                    {
                                        table.Cell().Element(CellStyle).Text(total.ToString("F2")).FontSize(8).Italic().Bold();
                                    }
                                }
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

                _logger.LogInformation("Daily report generated successfully for {StartDate}", startDate);
                return pdfData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the daily report: {Message}", ex.Message);
                throw;
            }
        }


    }
}
