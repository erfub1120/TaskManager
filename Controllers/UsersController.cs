using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var usersWithRoles = new List<UserWithRolesViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserWithRolesViewModel
                {
                    User = user,
                    Roles = roles.ToList()
                });
            }

            return View(usersWithRoles);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var groups = await _context.Groups
                .Where(g => g.Members.Any(m => m.Id == id))
                .ToListAsync();

            ViewBag.Roles = roles;
            ViewBag.Groups = groups;

            return View(user);
        }

        // GET: Users/ManageRoles/5
        public async Task<IActionResult> ManageRoles(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var model = new ManageUserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserEmail = user.Email,
                UserRoles = userRoles.ToList(),
                AllRoles = allRoles
            };

            return View(model);
        }

        // POST: Users/AssignRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }


            var currentRoles = await _userManager.GetRolesAsync(user);
            
            if (currentRoles.Contains(role))
            {
                TempData["Info"] = $"Użytkownik już posiada rolę '{role}'.";
                return RedirectToAction(nameof(ManageRoles), new { id = userId });
            }

            if (currentRoles.Any())
            {
                if (currentRoles.Contains("Administrator"))
                {
                    var admins = await _userManager.GetUsersInRoleAsync("Administrator");
                    if (admins.Count <= 1 && role != "Administrator")
                    {
                        TempData["Error"] = "Nie można zmienić roli: To jest ostatni Administrator. Najpierw przypisz rolę Administrator innemu użytkownikowi.";
                        return RedirectToAction(nameof(ManageRoles), new { id = userId });
                    }
                }

                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    TempData["Error"] = $"Nie udało się usunąć istniejących ról: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}";
                    return RedirectToAction(nameof(ManageRoles), new { id = userId });
                }
            }

            var result = await _userManager.AddToRoleAsync(user, role);
            if (result.Succeeded)
            {
                if (currentRoles.Any())
                {
                    TempData["Success"] = $"Rola zmieniona z '{string.Join(", ", currentRoles)}' na '{role}' pomyślnie.";
                }
                else
                {
                    TempData["Success"] = $"Rola '{role}' przypisana pomyślnie.";
                }
            }
            else
            {
                if (currentRoles.Any())
                {
                    await _userManager.AddToRolesAsync(user, currentRoles);
                }
                TempData["Error"] = $"Nie udało się przypisać roli: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(ManageRoles), new { id = userId });
        }

        // POST: Users/RemoveRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(role))
            {
                TempData["Info"] = $"Użytkownik nie posiada roli '{role}'.";
                return RedirectToAction(nameof(ManageRoles), new { id = userId });
            }

            if (role == "Administrator")
            {
                var admins = await _userManager.GetUsersInRoleAsync("Administrator");
                if (admins.Count <= 1)
                {
                    TempData["Error"] = "Nie można usunąć ostatniej roli Administrator.";
                    return RedirectToAction(nameof(ManageRoles), new { id = userId });
                }
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            if (result.Succeeded)
            {
                TempData["Success"] = $"Rola '{role}' usunięta pomyślnie. Użytkownik nie ma teraz przypisanej roli.";
                TempData["Warning"] = "Użytkownikowi należy przypisać nową rolę, aby mógł prawidłowo korzystać z systemu.";
            }
            else
            {
                TempData["Error"] = $"Nie udało się usunąć roli: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(ManageRoles), new { id = userId });
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Administrator"))
            {
                var admins = await _userManager.GetUsersInRoleAsync("Administrator");
                if (admins.Count <= 1)
                {
                    TempData["Error"] = "Nie można usunąć ostatniego Administratora.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Użytkownik usunięty pomyślnie.";
            }
            else
            {
                TempData["Error"] = $"Nie udało się usunąć użytkownika: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Users/ManageGroups/5
        public async Task<IActionResult> ManageGroups(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Groups)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var allGroups = await _context.Groups
                .OrderBy(g => g.Name)
                .Select(g => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                })
                .ToListAsync();

            ViewData["AvailableGroups"] = allGroups;

            return View(user);
        }

        // POST: Users/ManageGroups/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageGroups(string id, List<int>? selectedGroups)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Groups)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            user.Groups.Clear();

            if (selectedGroups != null && selectedGroups.Any())
            {
                var groups = await _context.Groups
                    .Where(g => selectedGroups.Contains(g.Id))
                    .ToListAsync();

                foreach (var group in groups)
                {
                    user.Groups.Add(group);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Grupy użytkownika zaktualizowane pomyślnie.";

            return RedirectToAction(nameof(Details), new { id });
        }
    }

    public class UserWithRolesViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; }
    }

    public class ManageUserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public List<string> UserRoles { get; set; }
        public List<string> AllRoles { get; set; }
    }
}
