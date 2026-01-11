using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GroupsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string? GetCurrentUserId()
        {
            return _userManager.GetUserId(User);
        }

        // GET: Groups
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            IQueryable<Group> groupsQuery;

            if (User.IsInRole("Administrator") || User.IsInRole("Manager"))
            {
                groupsQuery = _context.Groups.Include(g => g.Manager);
            }
            else
            {
                groupsQuery = _context.Groups
                    .Include(g => g.Manager)
                    .Include(g => g.Members)
                    .Where(g => g.Members.Any(m => m.Id == currentUserId));
            }

            return View(await groupsQuery.ToListAsync());
        }

        // GET: Groups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Manager)
                .Include(g => g.Members)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Administrator") && !User.IsInRole("Manager"))
            {
                if (!group.Members.Any(m => m.Id == currentUserId))
                {
                    return Forbid();
                }
            }

            return View(group);
        }

        // GET: Groups/Create
        [Authorize(Roles = "Administrator,Manager")]
        public IActionResult Create()
        {
            ViewData["ManagerId"] = new SelectList(_context.Users, "Id", "Email");
            ViewData["AvailableUsers"] = new MultiSelectList(_context.Users.OrderBy(u => u.Email), "Id", "Email");
            return View();
        }

        // POST: Groups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,CreatedDate,ManagerId")] Group group, List<string>? selectedMembers)
        {
            if (ModelState.IsValid)
            {
                if (selectedMembers != null && selectedMembers.Any())
                {
                    var members = await _context.Users
                        .Where(u => selectedMembers.Contains(u.Id))
                        .ToListAsync();
                    
                    foreach (var member in members)
                    {
                        group.Members.Add(member);
                    }
                }

                _context.Add(group);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ManagerId"] = new SelectList(_context.Users, "Id", "Email", group.ManagerId);
            ViewData["AvailableUsers"] = new MultiSelectList(_context.Users.OrderBy(u => u.Email), "Id", "Email", selectedMembers);
            return View(group);
        }

        // GET: Groups/Edit/5
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var group = await _context.Groups
                .Include(g => g.Members)
                .Include(g => g.Manager)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Manager"))
            {
                var currentUserId = GetCurrentUserId();
                if (group.ManagerId != currentUserId)
                {
                    return Forbid();
                }
            }


            ViewBag.CanChangeManager = User.IsInRole("Administrator");

            ViewData["ManagerId"] = new SelectList(_context.Users, "Id", "Email", group.ManagerId);
            ViewData["AvailableUsers"] = new MultiSelectList(
                _context.Users.OrderBy(u => u.Email), 
                "Id", 
                "Email", 
                group.Members.Select(m => m.Id));
            
            return View(group);
        }

        // POST: Groups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CreatedDate,ManagerId")] Group group, List<string>? selectedMembers)
        {
            if (id != group.Id)
            {
                return NotFound();
            }

            var existingGroupWithMembers = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (existingGroupWithMembers == null)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();

            if (User.IsInRole("Manager"))
            {
                if (existingGroupWithMembers.ManagerId != currentUserId)
                {
                    return Forbid();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingGroupWithMembers.Name = group.Name;
                    existingGroupWithMembers.Description = group.Description;

                    if (User.IsInRole("Administrator"))
                    {
                        existingGroupWithMembers.ManagerId = group.ManagerId;
                    }

                    existingGroupWithMembers.Members.Clear();
                    
                    if (selectedMembers != null && selectedMembers.Any())
                    {
                        var members = await _context.Users
                            .Where(u => selectedMembers.Contains(u.Id))
                            .ToListAsync();
                        
                        foreach (var member in members)
                        {
                            existingGroupWithMembers.Members.Add(member);
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(group.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            ViewBag.CanChangeManager = User.IsInRole("Administrator");
            ViewData["ManagerId"] = new SelectList(_context.Users, "Id", "Email", group.ManagerId);
            ViewData["AvailableUsers"] = new MultiSelectList(
                _context.Users.OrderBy(u => u.Email), 
                "Id", 
                "Email", 
                selectedMembers);
            
            return View(group);
        }

        // GET: Groups/Delete/5
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var group = await _context.Groups
                .Include(g => g.Manager)
                .Include(g => g.Members)
                .Include(g => g.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (group == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Manager"))
            {
                var currentUserId = GetCurrentUserId();
                if (group.ManagerId != currentUserId)
                {
                    return Forbid();
                }
            }

            ViewBag.HasActiveTasks = group.HasActiveTasks();
            ViewBag.ActiveTasksCount = group.Tasks.Count(t => t.Status != Models.TaskStatus.Done && t.Status != Models.TaskStatus.Cancelled);
            ViewBag.TotalTasksCount = group.Tasks.Count;
            ViewBag.MembersCount = group.Members.Count;

            return View(group);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Members)
                .Include(g => g.Tasks)
                .FirstOrDefaultAsync(g => g.Id == id);
            
            if (group == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Manager"))
            {
                var currentUserId = GetCurrentUserId();
                if (group.ManagerId != currentUserId)
                {
                    return Forbid();
                }
            }

            if (group.HasActiveTasks())
            {
                TempData["Error"] = "Nie można usunąć grupy, która ma aktywne zadania. Najpierw zakończ lub usuń wszystkie zadania w tej grupie.";
                return RedirectToAction(nameof(Index));
            }

            group.Members.Clear();

            if (group.Tasks.Any())
            {
                _context.TaskItems.RemoveRange(group.Tasks);
            }

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Grupa usunięta pomyślnie.";
            return RedirectToAction(nameof(Index));
        }

        private bool GroupExists(int id)
        {
            return _context.Groups.Any(e => e.Id == id);
        }
    }
}
