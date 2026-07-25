using CrmTaskManagement.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmTaskManagement.Data.Seed;

public static class DbInitializer
{
    public static async Task<bool> SeedAsync(AppDbContext context)
    {
        if (await context.Employees.AnyAsync())
        {
            return false;
        }

        var now = DateTime.UtcNow;

        var alice = new Employee
        {
            FullName = "Alice Johnson",
            Email = "alice.johnson@crm.local",
            Position = "Sales Manager",
            IsActive = true,
            CreatedAt = now.AddDays(-30)
        };

        var bob = new Employee
        {
            FullName = "Bob Smith",
            Email = "bob.smith@crm.local",
            Position = "Support Engineer",
            IsActive = true,
            CreatedAt = now.AddDays(-25)
        };

        var carol = new Employee
        {
            FullName = "Carol Davis",
            Email = "carol.davis@crm.local",
            Position = "Account Executive",
            IsActive = false,
            CreatedAt = now.AddDays(-20)
        };

        await context.Employees.AddRangeAsync(alice, bob, carol);
        await context.SaveChangesAsync();

        var newTask = new WorkTask
        {
            Title = "Follow up with prospective client",
            Description = "Reach out to the lead from the trade show and schedule a demo.",
            Status = Entities.TaskStatus.New,
            PlannedStartAt = now.AddDays(1),
            DueAt = now.AddDays(5),
            CompletedAt = null,
            CreatedByEmployeeId = alice.Id,
            AssignedToEmployeeId = bob.Id,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now.AddDays(-2)
        };

        var inProgressTask = new WorkTask
        {
            Title = "Resolve customer support ticket #482",
            Description = "Investigate reported login issue and provide a fix.",
            Status = Entities.TaskStatus.InProgress,
            PlannedStartAt = now.AddDays(-3),
            DueAt = now.AddDays(2),
            CompletedAt = null,
            CreatedByEmployeeId = alice.Id,
            AssignedToEmployeeId = bob.Id,
            CreatedAt = now.AddDays(-4),
            UpdatedAt = now.AddDays(-1)
        };

        var completedTask = new WorkTask
        {
            Title = "Prepare quarterly sales report",
            Description = "Compile Q2 sales figures for management review.",
            Status = Entities.TaskStatus.Completed,
            PlannedStartAt = now.AddDays(-10),
            DueAt = now.AddDays(-5),
            CompletedAt = now.AddDays(-6),
            CreatedByEmployeeId = alice.Id,
            AssignedToEmployeeId = alice.Id,
            CreatedAt = now.AddDays(-11),
            UpdatedAt = now.AddDays(-6)
        };

        await context.WorkTasks.AddRangeAsync(newTask, inProgressTask, completedTask);
        await context.SaveChangesAsync();

        return true;
    }
}
