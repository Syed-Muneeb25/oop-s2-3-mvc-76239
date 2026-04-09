using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize]
public class StudentProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public StudentProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var students = await _context.StudentProfiles.ToListAsync();
        return View(students);
    }

    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> Details(int id)
    {
        var student = await _context.StudentProfiles
            .Include(s => s.Enrolments)
                .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null) return NotFound();

        if (User.IsInRole("Faculty"))
        {
            var facultyUser = await _userManager.GetUserAsync(User);
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == facultyUser!.Id);

            var isMyStudent = await _context.CourseEnrolments
                .AnyAsync(e => e.StudentProfileId == id &&
                               _context.Courses.Any(c => c.Id == e.CourseId && c.FacultyProfileId == faculty!.Id));

            if (!isMyStudent) return Forbid();
        }

        return View(student);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStudentViewModel model)
    {
        if (model.DateOfBirth > DateOnly.FromDateTime(DateTime.Today))
            ModelState.AddModelError("DateOfBirth", "Date of birth cannot be in the future.");
        else if (model.DateOfBirth > DateOnly.FromDateTime(DateTime.Today.AddYears(-16)))
            ModelState.AddModelError("DateOfBirth", "Student must be at least 16 years old.");

        if (!ModelState.IsValid) return View(model);

        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "A user with this email already exists.");
            return View(model);
        }

        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "Student");

        var count = await _context.StudentProfiles.CountAsync();
        var profile = new StudentProfile
        {
            IdentityUserId = user.Id,
            Name = model.Name,
            Email = model.Email,
            Phone = model.Phone,
            Address = model.Address,
            DateOfBirth = model.DateOfBirth,
            StudentNumber = $"STU{(count + 1):D3}"
        };

        _context.StudentProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var student = await _context.StudentProfiles.FindAsync(id);
        if (student == null) return NotFound();
        return View(student);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StudentProfile model)
    {
        if (id != model.Id) return BadRequest();

        ModelState.Remove("StudentNumber");
        ModelState.Remove("IdentityUserId");
        ModelState.Remove("Email");

        if (model.DateOfBirth > DateOnly.FromDateTime(DateTime.Today))
            ModelState.AddModelError("DateOfBirth", "Date of birth cannot be in the future.");
        else if (model.DateOfBirth > DateOnly.FromDateTime(DateTime.Today.AddYears(-16)))
            ModelState.AddModelError("DateOfBirth", "Student must be at least 16 years old.");

        if (!ModelState.IsValid) return View(model);

        var existing = await _context.StudentProfiles.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Name = model.Name;
        existing.Phone = model.Phone;
        existing.Address = model.Address;
        existing.DateOfBirth = model.DateOfBirth;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        var student = await _context.StudentProfiles
            .Include(s => s.Enrolments
                .Where(e => e.Status == EnrolmentStatus.Active))
                .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.IdentityUserId == user!.Id);

        if (student == null) return NotFound();
        return View(student);
    }
}