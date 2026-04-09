using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Web.Models;

public class CreateAdminViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}