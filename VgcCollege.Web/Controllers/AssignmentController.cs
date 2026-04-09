using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain;
using VgcCollege.Web.Data;


namespace VgcCollege.Web.Controllers;

[Authorize]
public class AssignmentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public AssignmentController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> ByCourse(int courseId)
    {
        var course = await _context.Courses
            .Include(c => c.Assignments)
                .ThenInclude(a => a.Results)
                    .ThenInclude(r => r.StudentProfile)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null) return NotFound();

        if (User.IsInRole("Faculty"))
        {
            var user = await _userManager.GetUserAsync(User);
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == user!.Id);
            if (course.FacultyProfileId != faculty!.Id) return Forbid();
        }

        ViewBag.CourseName = course.Name;
        ViewBag.CourseId = courseId;
        return View(course.Assignments.ToList());
    }

    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> Create(int courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return NotFound();

        ViewBag.CourseName = course.Name;
        ViewBag.CourseId = courseId;
        ViewBag.CourseStart = course.StartDate.ToString("yyyy-MM-dd");
        ViewBag.CourseEnd = course.EndDate.ToString("yyyy-MM-dd");
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Faculty")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Assignment model)
    {
        ModelState.Remove("Course");
        ModelState.Remove("Results");

        var course = await _context.Courses.FindAsync(model.CourseId);
        if (course != null)
        {
            if (model.DueDate < course.StartDate)
                ModelState.AddModelError("DueDate",
                    $"Due date cannot be before course start date ({course.StartDate}).");
            if (model.DueDate > course.EndDate)
                ModelState.AddModelError("DueDate",
                    $"Due date cannot be after course end date ({course.EndDate}).");
        }

        var duplicateAssignment = await _context.Assignments
            .AnyAsync(a => a.Title == model.Title && a.CourseId == model.CourseId);
        if (duplicateAssignment)
            ModelState.AddModelError("Title",
                "An assignment with this title already exists in this course.");

        if (!ModelState.IsValid)
        {
            ViewBag.CourseName = course?.Name;
            ViewBag.CourseId = model.CourseId;
            ViewBag.CourseStart = course?.StartDate.ToString("yyyy-MM-dd");
            ViewBag.CourseEnd = course?.EndDate.ToString("yyyy-MM-dd");
            return View(model);
        }

        _context.Assignments.Add(model);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(ByCourse), new { courseId = model.CourseId });
    }

    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> Edit(int id)
    {
        var assignment = await _context.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (assignment == null) return NotFound();

        if (User.IsInRole("Faculty"))
        {
            var user = await _userManager.GetUserAsync(User);
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == user!.Id);
            if (assignment.Course.FacultyProfileId != faculty!.Id) return Forbid();
        }

        ViewBag.CourseStart = assignment.Course.StartDate.ToString("yyyy-MM-dd");
        ViewBag.CourseEnd = assignment.Course.EndDate.ToString("yyyy-MM-dd");
        return View(assignment);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Faculty")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Assignment model)
    {
        if (id != model.Id) return BadRequest();

        ModelState.Remove("Course");
        ModelState.Remove("Results");

        var existing = await _context.Assignments
            .Include(a => a.Course)
            .Include(a => a.Results)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (existing == null) return NotFound();

        if (model.DueDate < existing.Course.StartDate)
            ModelState.AddModelError("DueDate",
                $"Due date cannot be before course start date ({existing.Course.StartDate}).");
        if (model.DueDate > existing.Course.EndDate)
            ModelState.AddModelError("DueDate",
                $"Due date cannot be after course end date ({existing.Course.EndDate}).");

        var duplicateAssignment = await _context.Assignments
            .AnyAsync(a => a.Title == model.Title && a.CourseId == existing.CourseId && a.Id != id);
        if (duplicateAssignment)
            ModelState.AddModelError("Title",
                "An assignment with this title already exists in this course.");

        var maxExistingScore = existing.Results.Any()
            ? existing.Results.Max(r => r.Score)
            : 0;
        if (model.MaxScore < maxExistingScore)
            ModelState.AddModelError("MaxScore",
                $"MaxScore cannot be less than existing scores. Highest score: {maxExistingScore}.");

        if (!ModelState.IsValid)
        {
            ViewBag.CourseStart = existing.Course.StartDate.ToString("yyyy-MM-dd");
            ViewBag.CourseEnd = existing.Course.EndDate.ToString("yyyy-MM-dd");
            return View(model);
        }

        existing.Title = model.Title;
        existing.DueDate = model.DueDate;
        existing.MaxScore = model.MaxScore;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(ByCourse), new { courseId = existing.CourseId });
    }

    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> Results(int assignmentId)
    {
        var assignment = await _context.Assignments
            .Include(a => a.Course)
            .Include(a => a.Results)
                .ThenInclude(r => r.StudentProfile)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (assignment == null) return NotFound();

        if (User.IsInRole("Faculty"))
        {
            var user = await _userManager.GetUserAsync(User);
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == user!.Id);
            if (assignment.Course.FacultyProfileId != faculty!.Id) return Forbid();
        }

        var enrolled = await _context.CourseEnrolments
            .Include(e => e.StudentProfile)
            .Where(e => e.CourseId == assignment.CourseId)
            .ToListAsync();

        ViewBag.Enrolled = enrolled;

        if (TempData["Error"] is string error)
            ModelState.AddModelError("", error);

        return View(assignment);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Faculty")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResult(int assignmentId, int studentProfileId, int score, string? feedback)
    {
        var assignment = await _context.Assignments.FindAsync(assignmentId);
        if (assignment == null) return NotFound();

        if (score < 0 || score > assignment.MaxScore)
        {
            TempData["Error"] = $"Score must be between 0 and {assignment.MaxScore}.";
            return RedirectToAction(nameof(Results), new { assignmentId });
        }

        var existing = await _context.AssignmentResults
            .FirstOrDefaultAsync(r => r.AssignmentId == assignmentId
                                   && r.StudentProfileId == studentProfileId);
        if (existing != null)
        {
            existing.Score = score;
            existing.Feedback = feedback;
        }
        else
        {
            _context.AssignmentResults.Add(new AssignmentResult
            {
                AssignmentId = assignmentId,
                StudentProfileId = studentProfileId,
                Score = score,
                Feedback = feedback
            });
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Results), new { assignmentId });
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyResults()
    {
        var user = await _userManager.GetUserAsync(User);
        var student = await _context.StudentProfiles
            .FirstOrDefaultAsync(s => s.IdentityUserId == user!.Id);

        if (student == null) return NotFound();

        var results = await _context.AssignmentResults
            .Include(r => r.Assignment)
                .ThenInclude(a => a.Course)
            .Where(r => r.StudentProfileId == student.Id)
            .ToListAsync();

        return View(results);
    }
}