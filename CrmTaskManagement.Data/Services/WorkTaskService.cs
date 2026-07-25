using CrmTaskManagement.Data.Entities;
using CrmTaskManagement.Data.Repositories;
using CrmTaskManagement.Data.Services.Exceptions;

namespace CrmTaskManagement.Data.Services;

public class WorkTaskService
{
    private readonly IWorkTaskRepository _workTaskRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public WorkTaskService(IWorkTaskRepository workTaskRepository, IEmployeeRepository employeeRepository)
    {
        _workTaskRepository = workTaskRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task CreateTaskAsync(WorkTask task)
    {
        var assignee = await _employeeRepository.GetByIdAsync(task.AssignedToEmployeeId);

        if (assignee is null)
        {
            throw new BusinessRuleViolationException("Assignee not found");
        }

        if (!assignee.IsActive)
        {
            throw new BusinessRuleViolationException("Cannot assign a task to an inactive employee");
        }

        var now = DateTime.UtcNow;
        task.Status = WorkTaskStatus.New;
        task.CreatedAt = now;
        task.UpdatedAt = now;

        await _workTaskRepository.AddAsync(task);
    }

    public async Task ChangeStatusAsync(int taskId, WorkTaskStatus newStatus)
    {
        var task = await _workTaskRepository.GetByIdAsync(taskId);

        if (task is null)
        {
            throw new BusinessRuleViolationException("Task not found");
        }

        if (task.Status == WorkTaskStatus.Completed || task.Status == WorkTaskStatus.Cancelled)
        {
            throw new BusinessRuleViolationException($"Cannot change status of a task that is already {task.Status}");
        }

        task.CompletedAt = newStatus == WorkTaskStatus.Completed ? DateTime.UtcNow : null;
        task.Status = newStatus;
        task.UpdatedAt = DateTime.UtcNow;

        await _workTaskRepository.UpdateAsync(task);
    }

    public Task<IEnumerable<WorkTask>> GetTasksByAssigneeAsync(int employeeId)
    {
        return _workTaskRepository.GetByAssigneeAsync(employeeId);
    }
}
