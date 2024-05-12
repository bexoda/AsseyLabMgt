using System.ComponentModel.DataAnnotations;

namespace AsseyLabMgt.Models;

public class PlantSource
{
    [Key]
    public int Id { get; set; }
    public string PlantSourceName { get; set; }
    public string PlantSourceDescription { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; } = false;

}
