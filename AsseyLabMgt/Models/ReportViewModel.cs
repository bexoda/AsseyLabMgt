﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace AsseyLabMgt.Models
{
    public class ReportViewModel
    {
        public List<string> SelectedElements { get; set; } = new List<string>();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? JobNumber { get; set; }
        public string? Description { get; set; }
        public List<SelectListItem> ElementList { get; set; }
        public List<string> JobNumbers { get; set; } = new List<string>();
        public List<int> SelectedPlantIds { get; set; } = new List<int>();
        public List<SelectListItem> Plants { get; set; } = new List<SelectListItem>();
    }

}
