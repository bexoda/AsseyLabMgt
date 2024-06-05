using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AsseyLabMgt.Models;

public class Designation
{
    [Key]
    [DisplayName("Designation ID")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Designation name is required.")]
    [DisplayName("Designation Name")]
    [StringLength(100, ErrorMessage = "Designation name must not exceed 100 characters.")]
    public string DesignationName { get; set; }

    [DisplayName("Designation Code")]
    [StringLength(50, ErrorMessage = "Designation code must not exceed 50 characters.")]
    public string? DesignationCode { get; set; }

    [DisplayName("Description")]
    [StringLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
    public string? Description { get; set; }

    [DisplayName("Created Date")]
    [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [DisplayName("Updated Date")]
    [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
    public DateTime? UpdatedDate { get; set; }

    [DisplayName("Active Status")]
    public bool IsActive { get; set; } = false;
}
