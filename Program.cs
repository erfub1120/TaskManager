using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TaskManager.Data;
using TaskManager.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 10;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await context.Database.EnsureCreatedAsync();

    string[] roles = { "Administrator", "Manager", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = "admin@taskmanager.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "Administrator",
            Phone = "+48-111-111-111",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin@12345!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Administrator");
        }
    }

    var managerEmail = "manager@taskmanager.com";
    var managerUser = await userManager.FindByEmailAsync(managerEmail);
    
    if (managerUser == null)
    {
        managerUser = new ApplicationUser
        {
            UserName = managerEmail,
            Email = managerEmail,
            FirstName = "Jan",
            LastName = "Kowalski",
            Phone = "+48-222-222-222",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var result = await userManager.CreateAsync(managerUser, "Manager@123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(managerUser, "Manager");
        }
    }

    var userEmail = "user@taskmanager.com";
    var normalUser = await userManager.FindByEmailAsync(userEmail);
    
    if (normalUser == null)
    {
        normalUser = new ApplicationUser
        {
            UserName = userEmail,
            Email = userEmail,
            FirstName = "Anna",
            LastName = "Nowak",
            Phone = "+48-333-333-333",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var result = await userManager.CreateAsync(normalUser, "User@123456!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(normalUser, "User");
        }
    }

    if (!context.Groups.Any())
    {
        var group1 = new Group
        {
            Name = "Marketing",
            Description = "Marketing team tasks",
            CreatedDate = DateTime.UtcNow,
            ManagerId = managerUser.Id
        };

        var group2 = new Group
        {
            Name = "Development",
            Description = "Software development tasks",
            CreatedDate = DateTime.UtcNow,
            ManagerId = adminUser.Id
        };

        context.Groups.AddRange(group1, group2);
        await context.SaveChangesAsync();

        group1.Members.Add(normalUser);
        group1.Members.Add(managerUser);
        group2.Members.Add(adminUser);
        group2.Members.Add(normalUser);
        
        await context.SaveChangesAsync();

        var task1 = new TaskItem
        {
            Title = "Create marketing campaign",
            Description = "Develop Q1 marketing campaign strategy",
            Status = TaskManager.Models.TaskStatus.InProgress,
            Priority = TaskManager.Models.TaskPriority.High,
            CreatedDate = DateTime.UtcNow,
            DueDate = DateTime.Today.AddDays(14),
            GroupId = group1.Id,
            AssignedUserId = normalUser.Id,
            CreatedById = managerUser.Id
        };

        var task2 = new TaskItem
        {
            Title = "Fix login bug",
            Description = "Users reporting login issues on mobile devices",
            Status = TaskManager.Models.TaskStatus.ToDo,
            Priority = TaskManager.Models.TaskPriority.Critical,
            CreatedDate = DateTime.UtcNow,
            DueDate = DateTime.Today.AddDays(3),
            GroupId = group2.Id,
            AssignedUserId = adminUser.Id,
            CreatedById = adminUser.Id
        };

        var task3 = new TaskItem
        {
            Title = "Update documentation",
            Description = "Update user documentation for new features",
            Status = TaskManager.Models.TaskStatus.Done,
            Priority = TaskManager.Models.TaskPriority.Low,
            CreatedDate = DateTime.UtcNow.AddDays(-5),
            UpdatedDate = DateTime.UtcNow.AddDays(-1),
            DueDate = DateTime.Today.AddDays(-1),
            GroupId = group2.Id,
            AssignedUserId = normalUser.Id,
            CreatedById = adminUser.Id
        };

        context.TaskItems.AddRange(task1, task2, task3);
        await context.SaveChangesAsync();

        var log1 = new TaskLog
        {
            UserId = managerUser.Id,
            Timestamp = DateTime.UtcNow.AddHours(-2),
            Action = TaskManager.Models.LogAction.Created,
            Description = "Task created and assigned",
            TaskItemId = task1.Id,
            TaskTitle = task1.Title,
            TaskStatus = task1.Status,
            TaskPriority = task1.Priority,
            GroupId = group1.Id,
            GroupName = group1.Name,
            AssignedUserId = normalUser.Id,
            AssignedUserFirstName = normalUser.FirstName,
            AssignedUserLastName = normalUser.LastName
        };

        var log2 = new TaskLog
        {
            UserId = normalUser.Id,
            Timestamp = DateTime.UtcNow.AddHours(-1),
            Action = TaskManager.Models.LogAction.StatusChanged,
            Description = "Status changed from ToDo to InProgress",
            TaskItemId = task1.Id,
            TaskTitle = task1.Title,
            TaskStatus = TaskManager.Models.TaskStatus.InProgress,
            TaskPriority = task1.Priority,
            GroupId = group1.Id,
            GroupName = group1.Name,
            AssignedUserId = normalUser.Id,
            AssignedUserFirstName = normalUser.FirstName,
            AssignedUserLastName = normalUser.LastName
        };

        context.TaskLogs.AddRange(log1, log2);
        await context.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();