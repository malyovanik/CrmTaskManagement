using CrmTaskManagement.Data.Entities;
using CrmTaskManagement.Data.Repositories;
using CrmTaskManagement.Data.Services;
using CrmTaskManagement.Data.Services.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CrmTaskManagement.Console.Demo;

public class WorkTaskDemoRunner
{
    // Ids match the insertion order of DbInitializer.SeedAsync (Alice, Bob, Carol).
    private const int AliceId = 1;
    private const int BobId = 2;
    private const int CarolId = 3;

    private readonly WorkTaskService _workTaskService;
    private readonly IWorkTaskRepository _workTaskRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public WorkTaskDemoRunner(WorkTaskService workTaskService, IWorkTaskRepository workTaskRepository, IEmployeeRepository employeeRepository)
    {
        _workTaskService = workTaskService;
        _workTaskRepository = workTaskRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task RunAsync()
    {
        System.Console.WriteLine();
        System.Console.WriteLine("--- WorkTaskService demo ---");

        try
        {
            var alice = await _employeeRepository.GetByIdAsync(AliceId)
                ?? throw new InvalidOperationException($"Seeded employee Alice (Id={AliceId}) not found.");
            var bob = await _employeeRepository.GetByIdAsync(BobId)
                ?? throw new InvalidOperationException($"Seeded employee Bob (Id={BobId}) not found.");
            var carol = await _employeeRepository.GetByIdAsync(CarolId)
                ?? throw new InvalidOperationException($"Seeded employee Carol (Id={CarolId}) not found.");

            var demoTask = await DemoCreateTaskAsync(bob, alice);
            await DemoCreateTaskWithInactiveAssigneeAsync(alice, carol);
            await DemoChangeStatusAsync(demoTask);
            await DemoChangeStatusViolationAsync(demoTask);
            await DemoGetTasksByAssigneeAsync(alice);
            await DemoParentTaskSelfReferenceViolationAsync(demoTask);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Unexpected error during demo: {ex}");
        }
    }

    private async Task<WorkTask> DemoCreateTaskAsync(Employee creator, Employee assignee)
    {
        var demoTask = new WorkTask
        {
            Title = "Demo task for end-to-end verification",
            Description = "Created by the Program.cs demo scenario.",
            PlannedStartAt = DateTime.UtcNow,
            DueAt = DateTime.UtcNow.AddDays(3),
            CreatedByEmployeeId = creator.Id,
            AssignedToEmployeeId = assignee.Id
        };

        await _workTaskService.CreateTaskAsync(demoTask);
        System.Console.WriteLine($"[CreateTask] Created task Id={demoTask.Id}, Status={demoTask.Status}");

        return demoTask;
    }

    private async Task DemoCreateTaskWithInactiveAssigneeAsync(Employee creator, Employee inactiveAssignee)
    {
        try
        {
            var invalidTask = new WorkTask
            {
                Title = "Task assigned to inactive employee",
                PlannedStartAt = DateTime.UtcNow,
                DueAt = DateTime.UtcNow.AddDays(1),
                CreatedByEmployeeId = creator.Id,
                AssignedToEmployeeId = inactiveAssignee.Id
            };
            await _workTaskService.CreateTaskAsync(invalidTask);
        }
        catch (BusinessRuleViolationException ex)
        {
            System.Console.WriteLine($"[CreateTask - expected failure] {ex.Message}");
        }
    }

    private async Task DemoChangeStatusAsync(WorkTask task)
    {
        await _workTaskService.ChangeStatusAsync(task.Id, WorkTaskStatus.InProgress);
        System.Console.WriteLine($"[ChangeStatus] Status={task.Status}, CompletedAt={task.CompletedAt}");

        await _workTaskService.ChangeStatusAsync(task.Id, WorkTaskStatus.Completed);
        System.Console.WriteLine($"[ChangeStatus] Status={task.Status}, CompletedAt={task.CompletedAt}");
    }

    private async Task DemoChangeStatusViolationAsync(WorkTask task)
    {
        try
        {
            await _workTaskService.ChangeStatusAsync(task.Id, WorkTaskStatus.InProgress);
        }
        catch (BusinessRuleViolationException ex)
        {
            System.Console.WriteLine($"[ChangeStatus - expected failure] {ex.Message}");
        }
    }

    private async Task DemoGetTasksByAssigneeAsync(Employee assignee)
    {
        var tasks = await _workTaskService.GetTasksByAssigneeAsync(assignee.Id);
        System.Console.WriteLine("[GetTasksByAssignee] Tasks assigned to Alice:");
        foreach (var task in tasks)
        {
            System.Console.WriteLine($"  Id={task.Id}, Title={task.Title}, Status={task.Status}");
        }
    }

    private async Task DemoParentTaskSelfReferenceViolationAsync(WorkTask task)
    {
        // Bypasses WorkTaskService on purpose - this is a DB-level check constraint
        // (CK_work_tasks_parent_task_not_self), not a service-layer business rule.
        task.ParentTaskId = task.Id;

        try
        {
            await _workTaskRepository.UpdateAsync(task);
            System.Console.WriteLine("[ParentTaskId self-reference] Unexpectedly succeeded - constraint did not fire.");
        }
        catch (DbUpdateException ex)
        {
            System.Console.WriteLine($"[ParentTaskId self-reference - expected failure] {ex.InnerException?.Message ?? ex.Message}");
        }
        finally
        {
            task.ParentTaskId = null;
        }
    }
}
