using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Authorize]
    public class TaskLogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskLogsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string? GetCurrentUserId()
        {
            return _userManager.GetUserId(User);
        }

        // GET: TaskLogs 
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            IQueryable<TaskLog> logsQuery;

            if (User.IsInRole("Administrator"))
            {
                logsQuery = _context.TaskLogs
                    .Include(t => t.Group)
                    .Include(t => t.TaskItem)
                    .Include(t => t.User)
                    .OrderByDescending(t => t.Timestamp);
            }
            else if (User.IsInRole("Manager"))
            {
                logsQuery = _context.TaskLogs
                    .Include(t => t.Group)
                    .Include(t => t.TaskItem)
                    .Include(t => t.User)
                    .Where(t => t.Group.ManagerId == currentUserId)
                    .OrderByDescending(t => t.Timestamp);
            }
            else
            {
                logsQuery = _context.TaskLogs
                    .Include(t => t.Group)
                    .Include(t => t.TaskItem)
                    .Include(t => t.User)
                    .Where(t => t.AssignedUserId == currentUserId)
                    .OrderByDescending(t => t.Timestamp);
            }

            return View(await logsQuery.Take(100).ToListAsync());
        }

        // GET: TaskLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();

            var taskLog = await _context.TaskLogs
                .Include(t => t.Group)
                .Include(t => t.TaskItem)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (taskLog == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Administrator"))
            {
                if (User.IsInRole("Manager"))
                {
                    if (taskLog.Group?.ManagerId != currentUserId)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    if (taskLog.AssignedUserId != currentUserId)
                    {
                        return Forbid();
                    }
                }
            }

            return View(taskLog);
        }
    }
}
