using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AsseyLabMgt.Models;

public class LabResults
{
    [Key]
    public int Id { get; set; }
    [ForeignKey("LabRequestId")]
    public LabRequest LabRequest { get; set; }
    public int LabRequestId { get; set; }

    public string SampleId { get; set; }
    public decimal Mn { get; set; }
    public decimal Sol_Mn { get; set; }
    public decimal Fe { get; set; }
    public decimal B { get; set; }
    public decimal MnO2 { get; set; }
    public decimal SiO2 { get; set; }
    public decimal Al2O3 { get; set; }
    public decimal MgO { get; set; }
    public decimal CaO { get; set; }
    public decimal Au { get; set; }
    public decimal H2O { get; set; }
    public decimal Mg { get; set; }


    //for the record
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; } = false;
}
