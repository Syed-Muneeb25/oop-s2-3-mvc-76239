using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Web.Models;

public class CreateStudentViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Date of Birth")]
    public DateOnly DateOfBirth { get; set; }
}