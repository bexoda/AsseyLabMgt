using Microsoft.AspNetCore.Mvc.Rendering;

namespace AsseyLabMgt.Models
{
    public class StatisticViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> SelectedPlantIds { get; set; } = new List<int>();
        public List<SelectListItem> Plants { get; set; } = new List<SelectListItem>();
    }


}
