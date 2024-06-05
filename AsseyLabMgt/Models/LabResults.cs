using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AsseyLabMgt.Models;

public class LabResults
{
    [Key]
    [Display(Name = "Lab Result ID")]
    public int Id { get; set; }

    [ForeignKey("LabRequestId")]
    [Display(Name = "Lab Request")]
    public LabRequest LabRequest { get; set; }
    [Display(Name = "Lab Request ID")]
    public int LabRequestId { get; set; }

    [DisplayName("Time")]
    [DataType(DataType.Time)]
    public TimeOnly? Time { get; set; }

    [Required]
    [Display(Name = "Sample ID")]
    public string? SampleId { get; set; }

    [Display(Name = "Manganese (Mn)")]
    public decimal? Mn { get; set; }

    [Display(Name = "Soluble Manganese (Sol_Mn)")]
    public decimal? Sol_Mn { get; set; }

    [Display(Name = "Iron (Fe)")]
    public decimal? Fe { get; set; }

    [Display(Name = "Boron (B)")]
    public decimal? B { get; set; }

    [Display(Name = "Manganese Dioxide (MnO2)")]
    public decimal? MnO2 { get; set; }

    [Display(Name = "Silicon Dioxide (SiO2)")]
    public decimal? SiO2 { get; set; }

    [Display(Name = "Aluminum Oxide (Al2O3)")]
    public decimal? Al2O3 { get; set; }

    [Display(Name = "Magnesium Oxide (MgO)")]
    public decimal? MgO { get; set; }

    [Display(Name = "Calcium Oxide (CaO)")]
    public decimal? CaO { get; set; }

    [Display(Name = "Gold (Au)")]
    public decimal? Au { get; set; }

    [Display(Name = "Water (H2O)")]
    public decimal? H2O { get; set; }

    [Display(Name = "Magnesium (Mg)")]
    public decimal? Mg { get; set; }

    [Display(Name = "Phosphorus (P)")]
    public decimal? P { get; set; }

    [Display(Name = "Arsenic (As)")]
    public decimal? As { get; set; }

    // Record keeping fields
    [Display(Name = "Created Date")]
    [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Updated Date")]
    [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
    public DateTime? UpdatedDate { get; set; }

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; } = false;
}
