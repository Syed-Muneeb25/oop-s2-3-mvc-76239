using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin")]
public class BranchController : Controller
{
    private readonly ApplicationDbContext _context;

    public BranchController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Branches.ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var branch = await _context.Branches
            .Include(b => b.Courses)
                .ThenInclude(c => c.FacultyProfile)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (branch == null) return NotFound();
        return View(branch);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Branch model)
    {
        if (!ModelState.IsValid) return View(model);

        var duplicate = await _context.Branches
            .AnyAsync(b => b.Name == model.Name);
        if (duplicate)
        {
            ModelState.AddModelError("Name", "A branch with this name already exists.");
            return View(model);
        }

        _context.Branches.Add(model);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch == null) return NotFound();
        return View(branch);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Branch model)
    {
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid) return View(model);

        var duplicate = await _context.Branches
            .AnyAsync(b => b.Name == model.Name && b.Id != id);
        if (duplicate)
        {
            ModelState.AddModelError("Name", "A branch with this name already exists.");
            return View(model);
        }

        _context.Update(model);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
