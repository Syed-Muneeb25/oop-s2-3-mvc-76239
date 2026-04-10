using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VgcCollege.Domain
{
    public class Assignment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Max score is required")]
        [Range(1, 1000, ErrorMessage = "Max score must be between 1 and 1000")]
        [Display(Name = "Max Score")]
        public int MaxScore { get; set; }

        [Required(ErrorMessage = "Due date is required")]
        [Display(Name = "Due Date")]
        public DateOnly DueDate { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public ICollection<AssignmentResult> Results { get; set; } = new List<AssignmentResult>();
    }
}
