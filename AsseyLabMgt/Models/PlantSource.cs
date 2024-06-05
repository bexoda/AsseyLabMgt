using System.ComponentModel.DataAnnotations;

namespace AsseyLabMgt.Models;

public class PlantSource
{
    [Key]
    [Display(Name = "Plant Source ID")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Plant source name is required.")]
    [Display(Name = "Plant Source Name")]
    [StringLength(100, ErrorMessage = "Plant source name must not exceed 100 characters.")]
    public string PlantSourceName { get; set; }

    [Display(Name = "Plant Source Description")]
    [StringLength(500, ErrorMessage = "Plant source description must not exceed 500 characters.")]
    public string PlantSourceDescription { get; set; }

    [Display(Name = "Created Date")]
    [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Updated Date")]
    [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
    public DateTime? UpdatedDate { get; set; }

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; } = false;
}
