// These tests require Docker to be running locally (or in CI): Testcontainers spins up a
// real, disposable PostgreSQL container so the actual database-level check constraints and
// unique indexes are exercised. EF Core's InMemory provider silently ignores check
// constraints, which would make tests like these meaningless.

using CrmTaskManagement.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CrmTaskManagement.Data.Tests.Integration;

public class DatabaseConstraintsTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgresContainer = null!;

    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder("postgres:16")
            .WithDatabase("crm_task_management_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgresContainer.StartAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }

    private AppDbContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(_postgresContainer.GetConnectionString()).UseSnakeCaseNamingConvention();
        return new AppDbContext(optionsBuilder.Options);
    }

    private static async Task<(Employee Creator, Employee Assignee)> SeedEmployeePairAsync(AppDbContext context)
    {
        var creator = new Employee
        {
            FullName = "Creator Employee",
            Email = $"creator-{Guid.NewGuid():N}@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var assignee = new Employee
        {
            FullName = "Assignee Employee",
            Email = $"assignee-{Guid.NewGuid():N}@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Employees.AddRange(creator, assignee);
        await context.SaveChangesAsync();

        return (creator, assignee);
    }

    [Fact]
    public async Task SaveChangesAsync_DueAtBeforePlannedStartAt_ThrowsDbUpdateException()
    {
        await using var context = CreateContext();
        var (creator, assignee) = await SeedEmployeePairAsync(context);

        var now = DateTime.UtcNow;
        context.WorkTasks.Add(new WorkTask
        {
            Title = "Invalid due date task",
            Status = WorkTaskStatus.New,
            PlannedStartAt = now,
            DueAt = now.AddDays(-1),
            CreatedByEmployeeId = creator.Id,
            AssignedToEmployeeId = assignee.Id,
            CreatedAt = now,
            UpdatedAt = now
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_CompletedStatusWithoutCompletedAt_ThrowsDbUpdateException()
    {
        await using var context = CreateContext();
        var (creator, assignee) = await SeedEmployeePairAsync(context);

        var now = DateTime.UtcNow;
        context.WorkTasks.Add(new WorkTask
        {
            Title = "Completed task missing CompletedAt",
            Status = WorkTaskStatus.Completed,
            PlannedStartAt = now.AddDays(-2),
            DueAt = now.AddDays(-1),
            CompletedAt = null,
            CreatedByEmployeeId = creator.Id,
            AssignedToEmployeeId = assignee.Id,
            CreatedAt = now,
            UpdatedAt = now
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_NonCompletedStatusWithCompletedAtSet_ThrowsDbUpdateException()
    {
        await using var context = CreateContext();
        var (creator, assignee) = await SeedEmployeePairAsync(context);

        var now = DateTime.UtcNow;
        context.WorkTasks.Add(new WorkTask
        {
            Title = "New task with CompletedAt set",
            Status = WorkTaskStatus.New,
            PlannedStartAt = now,
            DueAt = now.AddDays(1),
            CompletedAt = now,
            CreatedByEmployeeId = creator.Id,
            AssignedToEmployeeId = assignee.Id,
            CreatedAt = now,
            UpdatedAt = now
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_ParentTaskIdEqualsOwnId_ThrowsDbUpdateException()
    {
        await using var context = CreateContext();
        var (creator, assignee) = await SeedEmployeePairAsync(context);

        var now = DateTime.UtcNow;
        var task = new WorkTask
        {
            Title = "Task that will reference itself",
            Status = WorkTaskStatus.New,
            PlannedStartAt = now,
            DueAt = now.AddDays(1),
            CreatedByEmployeeId = creator.Id,
            AssignedToEmployeeId = assignee.Id,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.WorkTasks.Add(task);
        await context.SaveChangesAsync();

        task.ParentTaskId = task.Id;

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_DuplicateEmployeeEmail_ThrowsDbUpdateException()
    {
        await using var context = CreateContext();
        var email = $"duplicate-{Guid.NewGuid():N}@example.com";

        context.Employees.Add(new Employee
        {
            FullName = "First Employee",
            Email = email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        context.Employees.Add(new Employee
        {
            FullName = "Second Employee",
            Email = email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_ValidWorkTask_SavesSuccessfully()
    {
        await using var context = CreateContext();
        var (creator, assignee) = await SeedEmployeePairAsync(context);

        var now = DateTime.UtcNow;
        var task = new WorkTask
        {
            Title = "Valid task",
            Status = WorkTaskStatus.New,
            PlannedStartAt = now,
            DueAt = now.AddDays(1),
            CompletedAt = null,
            CreatedByEmployeeId = creator.Id,
            AssignedToEmployeeId = assignee.Id,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.WorkTasks.Add(task);

        await context.SaveChangesAsync();

        Assert.True(task.Id > 0);
    }
}
