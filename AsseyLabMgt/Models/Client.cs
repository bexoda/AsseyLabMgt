using System.ComponentModel.DataAnnotations;

namespace AsseyLabMgt.Models;

public class Client
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string ClientName { get; set; }
    [Required]
    public string ClientCode { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; } = false;
}
