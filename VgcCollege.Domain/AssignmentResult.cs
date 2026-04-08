using System.ComponentModel.DataAnnotations;
using Xunit.Sdk;

namespace VgcCollege.Domain
{
    public class AssignmentResult
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Score is required")]
        [Range(0, 1000, ErrorMessage = "Score must be between 0 and 1000")]
        public int Score { get; set; }

        [StringLength(500)]
        public string? Feedback { get; set; }

        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; } = null!;

        public int StudentProfileId { get; set; }
        public StudentProfile StudentProfile { get; set; } = null!;
    }
}
