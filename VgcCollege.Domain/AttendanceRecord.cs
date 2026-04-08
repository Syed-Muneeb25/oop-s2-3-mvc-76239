using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VgcCollege.Domain
{
    public enum AttendanceStatus { Present, Absent, NA }

    public class AttendanceRecord
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Week Number")]
        public int WeekNumber { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Absent;

        public int CourseEnrolmentId { get; set; }
        public CourseEnrolment CourseEnrolment { get; set; } = null!;
    }
}
