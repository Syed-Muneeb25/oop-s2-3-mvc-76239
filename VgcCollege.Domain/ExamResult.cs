using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VgcCollege.Domain
{
    public class ExamResult
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Score is required")]
        [Range(0, 1000, ErrorMessage = "Score must be between 0 and 1000")]
        public int Score { get; set; }

        [StringLength(10)]
        public string? Grade { get; set; }

        public int ExamId { get; set; }
        public Exam Exam { get; set; } = null!;

        public int StudentProfileId { get; set; }
        public StudentProfile StudentProfile { get; set; } = null!;
    }
}
