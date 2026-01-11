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
    public class TaskItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskItemsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string? GetCurrentUserId()
        {
            return _userManager.GetUserId(User);
        }

        private async Task CreateTaskLog(TaskItem task, LogAction action, string description)
        {
            var currentUserId = GetCurrentUserId();
            
            var log = new TaskLog
            {
                UserId = currentUserId ?? string.Empty,
                Timestamp = DateTime.UtcNow,
                Action = action,
                Description = description,
                TaskItemId = task.Id,
                TaskTitle = task.Title,
                TaskStatus = task.Status,
                TaskPriority = task.Priority,
                GroupId = task.GroupId,
                GroupName = task.Group?.Name ?? string.Empty,
                AssignedUserId = task.AssignedUserId,
                AssignedUserFirstName = task.AssignedUser?.FirstName,
                AssignedUserLastName = task.AssignedUser?.LastName
            };

            _context.TaskLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        // GET: TaskItems
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            IQueryable<TaskItem> tasksQuery;

            if (User.IsInRole("Administrator"))
            {
                tasksQuery = _context.TaskItems
                    .Include(t => t.AssignedUser)
                    .Include(t => t.CreatedBy)
                    .Include(t => t.Group);
            }
            else if (User.IsInRole("Manager"))
            {
                tasksQuery = _context.TaskItems
                    .Include(t => t.AssignedUser)
                    .Include(t => t.CreatedBy)
                    .Include(t => t.Group)
                    .Where(t => t.Group.ManagerId == currentUserId);
            }
            else
            {
                tasksQuery = _context.TaskItems
                    .Include(t => t.AssignedUser)
                    .Include(t => t.CreatedBy)
                    .Include(t => t.Group)
                    .Where(t => t.AssignedUserId == currentUserId);
            }

            return View(await tasksQuery.ToListAsync());
        }

        // GET: TaskItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();

            var taskItem = await _context.TaskItems
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedBy)
                .Include(t => t.Group)
                    .ThenInclude(g => g.Manager)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (taskItem == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Administrator"))
            {
                if (User.IsInRole("Manager"))
                {
                    if (taskItem.Group?.ManagerId != currentUserId)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    if (taskItem.AssignedUserId != currentUserId)
                    {
                        return Forbid();
                    }
                }
            }

            return View(taskItem);
        }

        // GET: TaskItems/Create
        [Authorize(Roles = "Administrator,Manager")]
        public IActionResult Create()
        {
            var currentUserId = GetCurrentUserId();

            var users = _context.Users
                .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName })
                .ToList();
            users.Insert(0, new { Id = "", FullName = "-- Brak przypisania --" });
            ViewData["AssignedUserId"] = new SelectList(users, "Id", "FullName");

            ViewData["CurrentUserId"] = currentUserId;
            
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "Name");
            ViewData["Status"] = new SelectList(Enum.GetValues(typeof(Models.TaskStatus)));
            ViewData["Priority"] = new SelectList(Enum.GetValues(typeof(Models.TaskPriority)));
            return View();
        }

        // POST: TaskItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Status,Priority,DueDate,GroupId,AssignedUserId")] TaskItem taskItem)
        {
            var currentUserId = GetCurrentUserId();

            taskItem.CreatedById = currentUserId ?? string.Empty;
            taskItem.CreatedDate = DateTime.UtcNow;

            if (string.IsNullOrEmpty(taskItem.AssignedUserId))
            {
                taskItem.AssignedUserId = null;
            }


            ModelState.Remove("CreatedById");
            ModelState.Remove("CreatedDate");

            if (ModelState.IsValid)
            {
                _context.Add(taskItem);
                await _context.SaveChangesAsync();

                var createdTask = await _context.TaskItems
                    .Include(t => t.Group)
                    .Include(t => t.AssignedUser)
                    .FirstOrDefaultAsync(t => t.Id == taskItem.Id);

                if (createdTask != null)
                {
                    var description = createdTask.AssignedUserId != null 
                        ? $"Zadanie utworzone i przypisane do {createdTask.AssignedUser?.GetFullName()}"
                        : "Zadanie utworzone (nieprzypisane)";
                    await CreateTaskLog(createdTask, LogAction.Created, description);
                }

                return RedirectToAction(nameof(Index));
            }

            var users = _context.Users
                .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName })
                .ToList();
            users.Insert(0, new { Id = "", FullName = "-- Brak przypisania --" });
            ViewData["AssignedUserId"] = new SelectList(users, "Id", "FullName", taskItem.AssignedUserId);
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "Name", taskItem.GroupId);
            ViewData["Status"] = new SelectList(Enum.GetValues(typeof(Models.TaskStatus)), taskItem.Status);
            ViewData["Priority"] = new SelectList(Enum.GetValues(typeof(Models.TaskPriority)), taskItem.Priority);
            return View(taskItem);
        }

        // GET: TaskItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();

            var taskItem = await _context.TaskItems
                .Include(t => t.Group)
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (taskItem == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Administrator"))
            {
                if (User.IsInRole("Manager"))
                {
                    if (taskItem.Group?.ManagerId != currentUserId)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    if (taskItem.AssignedUserId != currentUserId)
                    {
                        return Forbid();
                    }
                }
            }

            ViewBag.IsUser = User.IsInRole("User");
            ViewBag.CanEditAllFields = User.IsInRole("Administrator") || User.IsInRole("Manager");

            if (User.IsInRole("Administrator") || User.IsInRole("Manager"))
            {
                var users = _context.Users
                    .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName })
                    .ToList();
                users.Insert(0, new { Id = "", FullName = "-- Brak przypisania --" });
                ViewData["AssignedUserId"] = new SelectList(users, "Id", "FullName", taskItem.AssignedUserId ?? "");
            }
            
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "Name", taskItem.GroupId);
            ViewData["Status"] = new SelectList(Enum.GetValues(typeof(Models.TaskStatus)), taskItem.Status);
            ViewData["Priority"] = new SelectList(Enum.GetValues(typeof(Models.TaskPriority)), taskItem.Priority);
            
            return View(taskItem);
        }

        // POST: TaskItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Status,Priority,CreatedDate,UpdatedDate,DueDate,GroupId,AssignedUserId,CreatedById")] TaskItem taskItem)
        {
            if (id != taskItem.Id)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();
            var existingTask = await _context.TaskItems
                .Include(t => t.Group)
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTask == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Administrator"))
            {
                if (User.IsInRole("Manager"))
                {
                    if (existingTask.Group?.ManagerId != currentUserId)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    if (existingTask.AssignedUserId != currentUserId)
                    {
                        return Forbid();
                    }
                }
            }

            if (string.IsNullOrEmpty(taskItem.AssignedUserId))
            {
                taskItem.AssignedUserId = null;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var oldStatus = existingTask.Status;
                    var oldPriority = existingTask.Priority;
                    var oldAssignedUser = existingTask.AssignedUserId;
                    bool hasChanges = false;

                    if (User.IsInRole("User"))
                    {
                        if (oldStatus != taskItem.Status)
                        {
                            existingTask.Status = taskItem.Status;
                            existingTask.UpdatedDate = DateTime.UtcNow;
                            hasChanges = true;
                        }
                    }
                    else
                    {
                        existingTask.Title = taskItem.Title;
                        existingTask.Description = taskItem.Description;
                        existingTask.Status = taskItem.Status;
                        existingTask.Priority = taskItem.Priority;
                        existingTask.DueDate = taskItem.DueDate;
                        existingTask.GroupId = taskItem.GroupId;
                        existingTask.AssignedUserId = taskItem.AssignedUserId;
                        existingTask.UpdatedDate = DateTime.UtcNow;
                        hasChanges = true;
                    }

                    if (hasChanges)
                    {
                        await _context.SaveChangesAsync();

                        await _context.Entry(existingTask).Reference(t => t.Group).LoadAsync();
                        await _context.Entry(existingTask).Reference(t => t.AssignedUser).LoadAsync();

                        if (oldStatus != taskItem.Status)
                        {
                            await CreateTaskLog(existingTask, LogAction.StatusChanged, 
                                $"Status zmieniony z {oldStatus} na {taskItem.Status}");
                        }

                        if (!User.IsInRole("User"))
                        {
                            if (oldPriority != taskItem.Priority)
                            {
                                await CreateTaskLog(existingTask, LogAction.PriorityChanged, 
                                    $"Priorytet zmieniony z {oldPriority} na {taskItem.Priority}");
                            }

                            if (oldAssignedUser != taskItem.AssignedUserId)
                            {
                                if (taskItem.AssignedUserId != null)
                                {
                                    await CreateTaskLog(existingTask, LogAction.AssignedToUser, 
                                        $"Zadanie przypisane do {existingTask.AssignedUser?.GetFullName() ?? "użytkownika"}");
                                }
                                else
                                {
                                    await CreateTaskLog(existingTask, LogAction.UnassignedFromUser, 
                                        "Zadanie odpisane");
                                }
                            }

                            if (oldStatus == taskItem.Status && oldPriority == taskItem.Priority && 
                                oldAssignedUser == taskItem.AssignedUserId)
                            {
                                await CreateTaskLog(existingTask, LogAction.Updated, "Szczegóły zadania zaktualizowane");
                            }
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(taskItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
                return RedirectToAction(nameof(Index));
            }

            ViewBag.IsUser = User.IsInRole("User");
            ViewBag.CanEditAllFields = User.IsInRole("Administrator") || User.IsInRole("Manager");
            
            if (User.IsInRole("Administrator") || User.IsInRole("Manager"))
            {
                var users = _context.Users
                    .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName })
                    .ToList();
                users.Insert(0, new { Id = "", FullName = "-- Brak przypisania --" });
                ViewData["AssignedUserId"] = new SelectList(users, "Id", "FullName", taskItem.AssignedUserId ?? "");
            }
            
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "Name", taskItem.GroupId);
            ViewData["Status"] = new SelectList(Enum.GetValues(typeof(Models.TaskStatus)), taskItem.Status);
            ViewData["Priority"] = new SelectList(Enum.GetValues(typeof(Models.TaskPriority)), taskItem.Priority);
            return View(taskItem);
        }

        // GET: TaskItems/Delete/5
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItems
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedBy)
                .Include(t => t.Group)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (taskItem == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Manager"))
            {
                var currentUserId = GetCurrentUserId();
                if (taskItem.Group?.ManagerId != currentUserId)
                {
                    return Forbid();
                }
            }

            return View(taskItem);
        }

        // POST: TaskItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskItem = await _context.TaskItems
                .Include(t => t.Group)
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (taskItem == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Manager"))
            {
                var currentUserId = GetCurrentUserId();
                if (taskItem.Group?.ManagerId != currentUserId)
                {
                    return Forbid();
                }
            }


            await CreateTaskLog(taskItem, LogAction.Deleted, $"Zadanie '{taskItem.Title}' zostało usunięte");

            _context.TaskItems.Remove(taskItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaskItemExists(int id)
        {
            return _context.TaskItems.Any(e => e.Id == id);
        }
    }
}
