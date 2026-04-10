using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VgcCollege.Domain
{
    public class Exam
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required")]
        public DateOnly Date { get; set; }

        [Required(ErrorMessage = "Max score is required")]
        [Range(1, 1000, ErrorMessage = "Max score must be between 1 and 1000")]
        [Display(Name = "Max Score")]
        public int MaxScore { get; set; }

        [Display(Name = "Results Released")]
        public bool ResultsReleased { get; set; } = false;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public ICollection<ExamResult> Results { get; set; } = new List<ExamResult>();
    }
}
