using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;

namespace AsseyLabMgt.Models
{
    public class ReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> SelectedPlantIds { get; set; } = new List<int>();
        public List<SelectListItem> Plants { get; set; } = new List<SelectListItem>();
    }


}
