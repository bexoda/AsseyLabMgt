using System;
using System.ComponentModel.DataAnnotations;

namespace AsseyLabMgt.Models
{
    public class ReportViewModel
    {
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}
