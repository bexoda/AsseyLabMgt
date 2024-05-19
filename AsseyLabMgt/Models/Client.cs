using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AsseyLabMgt.Models;

public class Client
{
    [Key]
    [DisplayName("Client ID")]
    public int Id { get; set; }

    [Required]
    [DisplayName("Client Name")]
    [StringLength(100, ErrorMessage = "Client name must not exceed 100 characters.")]
    public string ClientName { get; set; }

    [Required]
    [DisplayName("Client Code")]
    [StringLength(50, ErrorMessage = "Client code must not exceed 50 characters.")]
    public string ClientCode { get; set; }

    [DisplayName("Created Date")]
    [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [DisplayName("Updated Date")]
    [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
    public DateTime? UpdatedDate { get; set; }

    [DisplayName("Active Status")]
    public bool IsActive { get; set; } = false;
}
