using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AsseyLabMgt.Models
{
    public class Staff
    {
        [Key]
        public int Id { get; set; }
        public string StaffNumber { get; set; }
        public string Surname { get; set; }
        public string Firstname { get; set; }
        public string Othername { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
        public string Phone { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
        public int DepartmentId { get; set; }

        [ForeignKey("DesignationId")]
        public Designation Designation { get; set; }
        public int DesignationId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; } = false;

    }
}
