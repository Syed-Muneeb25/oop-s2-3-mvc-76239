using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin,Faculty")]
public class FacultyProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public FacultyProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var faculty = await _context.FacultyProfiles.ToListAsync();
        return View(faculty);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Details(int id)
    {
        var faculty = await _context.FacultyProfiles
            .Include(f => f.Courses)
                .ThenInclude(c => c.Branch)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (faculty == null) return NotFound();
        return View(faculty);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFacultyViewModel model)
    {
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

        await _userManager.AddToRoleAsync(user, "Faculty");

        var profile = new FacultyProfile
        {
            IdentityUserId = user.Id,
            Name = model.Name,
            Email = model.Email,
            Phone = model.Phone
        };

        _context.FacultyProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var faculty = await _context.FacultyProfiles.FindAsync(id);
        if (faculty == null) return NotFound();
        return View(faculty);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FacultyProfile model)
    {
        if (id != model.Id) return BadRequest();

        ModelState.Remove("IdentityUserId");
        ModelState.Remove("Email");

        if (!ModelState.IsValid) return View(model);

        var existing = await _context.FacultyProfiles.FindAsync(id);
        if (existing == null) return NotFound();

        var duplicatePhone = await _context.FacultyProfiles
            .AnyAsync(f => f.Phone == model.Phone && f.Id != id);
        if (duplicatePhone)
        {
            ModelState.AddModelError("Phone", "Another faculty member already has this phone number.");
            return View(model);
        }

        existing.Name = model.Name;
        existing.Phone = model.Phone;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Faculty")]
    public async Task<IActionResult> MyProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        var faculty = await _context.FacultyProfiles
            .Include(f => f.Courses)
                .ThenInclude(c => c.Branch)
            .FirstOrDefaultAsync(f => f.IdentityUserId == user!.Id);

        if (faculty == null) return NotFound();
        return View(faculty);
    }
}
