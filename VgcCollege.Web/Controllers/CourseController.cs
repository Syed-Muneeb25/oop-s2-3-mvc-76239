using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize]
public class CourseController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CourseController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        IQueryable<Course> query = _context.Courses
            .Include(c => c.Branch)
            .Include(c => c.FacultyProfile);

        if (User.IsInRole("Faculty"))
        {
            var user = await _userManager.GetUserAsync(User);
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == user!.Id);
            query = query.Where(c => c.FacultyProfileId == faculty!.Id);
        }
        else if (User.IsInRole("Student"))
        {
            var user = await _userManager.GetUserAsync(User);
            var student = await _context.StudentProfiles
                .FirstOrDefaultAsync(s => s.IdentityUserId == user!.Id);
            query = query.Where(c => c.Enrolments
                .Any(e => e.StudentProfileId == student!.Id
                       && e.Status == EnrolmentStatus.Active));
        }

        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Branch)
            .Include(c => c.FacultyProfile)
            .Include(c => c.Enrolments)
                .ThenInclude(e => e.StudentProfile)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return NotFound();

        if (User.IsInRole("Faculty"))
        {
            var user = await _userManager.GetUserAsync(User);
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == user!.Id);
            if (course.FacultyProfileId != faculty!.Id) return Forbid();
        }

        if (User.IsInRole("Student"))
        {
            var user = await _userManager.GetUserAsync(User);
            var student = await _context.StudentProfiles
                .FirstOrDefaultAsync(s => s.IdentityUserId == user!.Id);
            var isEnrolled = course.Enrolments
                .Any(e => e.StudentProfileId == student!.Id
                       && e.Status == EnrolmentStatus.Active);
            if (!isEnrolled) return Forbid();
        }

        if (User.IsInRole("Admin"))
        {
            ViewBag.AllStudents = new SelectList(
                await _context.StudentProfiles.ToListAsync(), "Id", "Name");

            if (TempData["Error"] is string error)
                ViewBag.Error = error;
        }

        return View(course);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Branches = new SelectList(
            await _context.Branches.ToListAsync(), "Id", "Name");
        ViewBag.Faculty = new SelectList(
            await _context.FacultyProfiles.ToListAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course model)
    {
        ModelState.Remove("Branch");
        ModelState.Remove("FacultyProfile");

        if (model.EndDate <= model.StartDate)
            ModelState.AddModelError("EndDate", "End date must be after start date.");

        var duplicate = await _context.Courses
            .AnyAsync(c => c.Name == model.Name && c.BranchId == model.BranchId);
        if (duplicate)
            ModelState.AddModelError("Name", "A course with this name already exists in this branch.");

        if (!ModelState.IsValid)
        {
            ViewBag.Branches = new SelectList(
                await _context.Branches.ToListAsync(), "Id", "Name");
            ViewBag.Faculty = new SelectList(
                await _context.FacultyProfiles.ToListAsync(), "Id", "Name");
            return View(model);
        }

        _context.Courses.Add(model);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null) return NotFound();

        ViewBag.Branches = new SelectList(
            await _context.Branches.ToListAsync(), "Id", "Name", course.BranchId);
        ViewBag.Faculty = new SelectList(
            await _context.FacultyProfiles.ToListAsync(), "Id", "Name", course.FacultyProfileId);
        return View(course);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Course model)
    {
        if (id != model.Id) return BadRequest();

        ModelState.Remove("Branch");
        ModelState.Remove("FacultyProfile");

        if (model.EndDate <= model.StartDate)
            ModelState.AddModelError("EndDate", "End date must be after start date.");

        var duplicate = await _context.Courses
            .AnyAsync(c => c.Name == model.Name && c.BranchId == model.BranchId && c.Id != id);
        if (duplicate)
            ModelState.AddModelError("Name", "A course with this name already exists in this branch.");

        if (!ModelState.IsValid)
        {
            ViewBag.Branches = new SelectList(
                await _context.Branches.ToListAsync(), "Id", "Name", model.BranchId);
            ViewBag.Faculty = new SelectList(
                await _context.FacultyProfiles.ToListAsync(), "Id", "Name", model.FacultyProfileId);
            return View(model);
        }

        _context.Update(model);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int courseId, int studentProfileId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return NotFound();

        if (DateOnly.FromDateTime(DateTime.Today) > course.EndDate)
        {
            TempData["Error"] = "Cannot enrol student — course has already ended.";
            return RedirectToAction(nameof(Details), new { id = courseId });
        }

        var alreadyEnrolled = await _context.CourseEnrolments
            .AnyAsync(e => e.StudentProfileId == studentProfileId
                        && e.CourseId == courseId);

        if (alreadyEnrolled)
        {
            TempData["Error"] = "This student is already enrolled in this course.";
            return RedirectToAction(nameof(Details), new { id = courseId });
        }

        _context.CourseEnrolments.Add(new CourseEnrolment
        {
            CourseId = courseId,
            StudentProfileId = studentProfileId,
            EnrolDate = DateOnly.FromDateTime(DateTime.Today),
            Status = EnrolmentStatus.Active
        });
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = courseId });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateEnrolmentStatus(int enrolmentId, EnrolmentStatus status, int courseId)
    {
        var enrolment = await _context.CourseEnrolments.FindAsync(enrolmentId);
        if (enrolment == null) return NotFound();

        enrolment.Status = status;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = courseId });
    }
}
