using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VgcCollege.Domain
{
    public enum EnrolmentStatus { Active, Withdrawn, Completed }

    public class CourseEnrolment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Enrol date is required")]
        [Display(Name = "Enrol Date")]
        public DateOnly EnrolDate { get; set; }

        public EnrolmentStatus Status { get; set; } = EnrolmentStatus.Active;

        public int StudentProfileId { get; set; }
        public StudentProfile StudentProfile { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }
}
