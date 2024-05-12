using System.ComponentModel.DataAnnotations;

namespace AsseyLabMgt.Models;

public class Designation
{
    [Key]
    public int Id { get; set; }
    public string DesignationName { get; set; }
    public string? DesignationCode { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; } = false;
}
