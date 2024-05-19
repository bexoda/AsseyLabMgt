using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AsseyLabMgt.Models;

public class LabRequest
{
    [Key]
    [DisplayName("Lab Request ID")]
    public int Id { get; set; }

    [DisplayName("Job Number")]
    [Required(ErrorMessage = "Job number is required.")]
    public string JobNumber { get; set; }

    [DisplayName("Request Date")]
    [DataType(DataType.Date)]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime Date { get; set; }

    [DisplayName("Production Date")]
    [DataType(DataType.Date)]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime ProductionDate { get; set; }

    [DisplayName("Client")]
    [ForeignKey("ClientId")]
    public Client Client { get; set; }
    [DisplayName("Client")]
    public int ClientId { get; set; }

    [DisplayName("Department")]
    [ForeignKey("DepartmentId")]
    public Department Department { get; set; }
    [DisplayName("Department")]
    public int DepartmentId { get; set; }

    [DisplayName("Plant Source")]
    [ForeignKey("PlantSourceId")]
    public PlantSource PlantSource { get; set; }
    [DisplayName("Plant Source")]
    public int PlantSourceId { get; set; }

    [DisplayName("Description")]
    public string Description { get; set; }

    [DisplayName("Number of Samples")]
    public int NumberOfSamples { get; set; }

    // Staff involvement
    [DisplayName("Delivered By")]
    [ForeignKey("DeliveredById")]
    public Staff DeliveredBy { get; set; }
    [DisplayName("Delivered By")]
    public int DeliveredById { get; set; }

    [DisplayName("Received By")]
    [ForeignKey("ReceivedById")]
    public Staff ReceivedBy { get; set; }
    [DisplayName("Received By")]
    public int ReceivedById { get; set; }

    [DisplayName("Time Received")]
    [DataType(DataType.Time)]
    public TimeOnly TimeReceived { get; set; }

    [DisplayName("Prepared By")]
    [ForeignKey("PreparedById")]
    public Staff PreparedBy { get; set; }
    [DisplayName("Prepared By")]
    public int PreparedById { get; set; }

    [DisplayName("Weighed By")]
    [ForeignKey("WeighedById")]
    public Staff WeighedBy { get; set; }
    [DisplayName("Weighed By")]
    public int WeighedById { get; set; }

    [DisplayName("Digested By")]
    [ForeignKey("DigestedById")]
    public Staff DigestedBy { get; set; }
    [DisplayName("Digested By")]
    public int DigestedById { get; set; }

    [DisplayName("Titrated By")]
    [ForeignKey("TitratedById")]
    public Staff TitratedBy { get; set; }
    [DisplayName("Titrated By")]
    public int TitratedById { get; set; }

    [DisplayName("Date Reported")]
    [DataType(DataType.Date)]
    public DateTime DateReported { get; set; }

    [DisplayName("Entered By")]
    [ForeignKey("EnteredById")]
    public Staff EnteredBy { get; set; }
    [DisplayName("Entered By")]
    public int EnteredById { get; set; }

    [DisplayName("Time Prepared")]
    [DataType(DataType.Time)]
    public TimeOnly TimePreoared { get; set; }

    [DisplayName("Created Date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [DisplayName("Updated Date")]
    public DateTime? UpdatedDate { get; set; }

    [DisplayName("Active Status")]
    public bool IsActive { get; set; } = false;

    [DisplayName("Lab Results")]
    public ICollection<LabResults> LabResults { get; set; } // Navigation property for related LabResults
}
