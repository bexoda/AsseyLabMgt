using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AsseyLabMgt.Models;

public class LabRequest
{
    [Key]
    public int Id { get; set; }
    public int JobNumber { get; set; }
    public DateTime Date { get; set; }
    public DateTime ProductionDate { get; set; }

    [ForeignKey("ClientId")]
    public Client Client { get; set; }
    public int ClientId { get; set; }

    [ForeignKey("DepartmentId")]
    public Department Department { get; set; }
    public int DepartmentId { get; set; }

    [ForeignKey("PlantSourceId")]
    public PlantSource PlantSource { get; set; }
    public int PlantSourceId { get; set; }
    public string Description { get; set; }
    public int NumberOfSamples { get; set; }

    [ForeignKey("DeliveredById")]
    public Staff DeliveredBy { get; set; }
    public int DeliveredById { get; set; }

    [ForeignKey("ReceivedById")]
    public Staff ReceivedBy { get; set; }
    public int ReceivedById { get; set; }
    public TimeOnly TimeReceived { get; set; }

    [ForeignKey("PreparedById")]
    public Staff PreparedBy { get; set; }
    public int PreparedById { get; set; }

    [ForeignKey("WeighedById")]
    public Staff WeighedBy { get; set; }
    public int WeighedById { get; set; }

    [ForeignKey("DigestedById")]
    public Staff DigestedBy { get; set; }
    public int DigestedById { get; set; }

    [ForeignKey("TitratedById")]
    public Staff TitratedBy { get; set; }
    public int TitratedById { get; set; }
    public DateTime DateReported { get; set; }

    [ForeignKey("EnteredById")]
    public Staff EnteredBy { get; set; }
    public int EnteredById { get; set; }
    public TimeOnly TimePreoared { get; set; }

    // for the record
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; } = false;
    public ICollection<LabResults> LabResults { get; set; } // Navigation property for related LabResults

}
