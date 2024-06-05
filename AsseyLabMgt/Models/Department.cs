using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AsseyLabMgt.Models;

public class Department
{
    [Key]
    [DisplayName("Department ID")]
    public int Id { get; set; }

    [Required]
    [DisplayName("Department Name")]
    [StringLength(100, ErrorMessage = "Department name must not exceed 100 characters.")]
    public string DeptName { get; set; }

    [Required]
    [DisplayName("Department Code")]
    [StringLength(50, ErrorMessage = "Department code must not exceed 50 characters.")]
    public string DeptCode { get; set; }

    [DisplayName("Created Date")]
    [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [DisplayName("Updated Date")]
    [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
    public DateTime? UpdatedDate { get; set; }

    [DisplayName("Active Status")]
    public bool IsActive { get; set; } = false;
}
