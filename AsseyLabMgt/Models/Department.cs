using System.ComponentModel.DataAnnotations;

namespace AsseyLabMgt.Models;

public class Department
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string DeptName { get; set; }
    [Required]
    public string DeptCode { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; } = false;
}
