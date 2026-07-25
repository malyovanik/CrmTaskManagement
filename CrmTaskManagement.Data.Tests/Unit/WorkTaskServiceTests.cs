using CrmTaskManagement.Data.Entities;
using CrmTaskManagement.Data.Repositories;
using CrmTaskManagement.Data.Services;
using CrmTaskManagement.Data.Services.Exceptions;
using NSubstitute;

namespace CrmTaskManagement.Data.Tests.Unit;

public class WorkTaskServiceTests
{
    private readonly IWorkTaskRepository _workTaskRepository = Substitute.For<IWorkTaskRepository>();
    private readonly IEmployeeRepository _employeeRepository = Substitute.For<IEmployeeRepository>();
    private readonly WorkTaskService _sut;

    public WorkTaskServiceTests()
    {
        _sut = new WorkTaskService(_workTaskRepository, _employeeRepository);
    }

    [Fact]
    public async Task CreateTaskAsync_ActiveAssignee_SetsStatusNewAndPersistsTask()
    {
        var assignee = new Employee { Id = 2, FullName = "Bob Smith", Email = "bob@example.com", IsActive = true };
        _employeeRepository.GetByIdAsync(2).Returns(assignee);

        var task = new WorkTask { Title = "Follow up with client", AssignedToEmployeeId = 2 };

        await _sut.CreateTaskAsync(task);

        Assert.Equal(WorkTaskStatus.New, task.Status);
        Assert.NotEqual(default, task.CreatedAt);
        Assert.NotEqual(default, task.UpdatedAt);
        await _workTaskRepository.Received(1).AddAsync(task);
    }

    [Fact]
    public async Task CreateTaskAsync_InactiveAssignee_ThrowsBusinessRuleViolationException()
    {
        var assignee = new Employee { Id = 3, FullName = "Carol Davis", Email = "carol@example.com", IsActive = false };
        _employeeRepository.GetByIdAsync(3).Returns(assignee);

        var task = new WorkTask { Title = "Task for inactive employee", AssignedToEmployeeId = 3 };

        var exception = await Assert.ThrowsAsync<BusinessRuleViolationException>(() => _sut.CreateTaskAsync(task));

        Assert.Equal("Cannot assign a task to an inactive employee", exception.Message);
        await _workTaskRepository.DidNotReceive().AddAsync(Arg.Any<WorkTask>());
    }

    [Fact]
    public async Task CreateTaskAsync_AssigneeNotFound_ThrowsBusinessRuleViolationException()
    {
        _employeeRepository.GetByIdAsync(99).Returns((Employee?)null);

        var task = new WorkTask { Title = "Task for unknown employee", AssignedToEmployeeId = 99 };

        var exception = await Assert.ThrowsAsync<BusinessRuleViolationException>(() => _sut.CreateTaskAsync(task));

        Assert.Equal("Assignee not found", exception.Message);
        await _workTaskRepository.DidNotReceive().AddAsync(Arg.Any<WorkTask>());
    }

    [Fact]
    public async Task ChangeStatusAsync_NewToInProgress_UpdatesStatusAndPersists()
    {
        var task = new WorkTask { Id = 1, Status = WorkTaskStatus.New };
        _workTaskRepository.GetByIdAsync(1).Returns(task);

        await _sut.ChangeStatusAsync(1, WorkTaskStatus.InProgress);

        Assert.Equal(WorkTaskStatus.InProgress, task.Status);
        Assert.Null(task.CompletedAt);
        await _workTaskRepository.Received(1).UpdateAsync(task);
    }

    [Fact]
    public async Task ChangeStatusAsync_TransitionToCompleted_SetsCompletedAt()
    {
        var task = new WorkTask { Id = 1, Status = WorkTaskStatus.InProgress };
        _workTaskRepository.GetByIdAsync(1).Returns(task);

        await _sut.ChangeStatusAsync(1, WorkTaskStatus.Completed);

        Assert.Equal(WorkTaskStatus.Completed, task.Status);
        Assert.NotNull(task.CompletedAt);
    }

    [Fact]
    public async Task ChangeStatusAsync_CurrentStatusCompleted_ThrowsBusinessRuleViolationException()
    {
        var task = new WorkTask { Id = 1, Status = WorkTaskStatus.Completed };
        _workTaskRepository.GetByIdAsync(1).Returns(task);

        var exception = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => _sut.ChangeStatusAsync(1, WorkTaskStatus.InProgress));

        Assert.Equal("Cannot change status of a task that is already Completed", exception.Message);
        await _workTaskRepository.DidNotReceive().UpdateAsync(Arg.Any<WorkTask>());
    }

    [Fact]
    public async Task ChangeStatusAsync_CurrentStatusCancelled_ThrowsBusinessRuleViolationException()
    {
        var task = new WorkTask { Id = 1, Status = WorkTaskStatus.Cancelled };
        _workTaskRepository.GetByIdAsync(1).Returns(task);

        var exception = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => _sut.ChangeStatusAsync(1, WorkTaskStatus.InProgress));

        Assert.Equal("Cannot change status of a task that is already Cancelled", exception.Message);
        await _workTaskRepository.DidNotReceive().UpdateAsync(Arg.Any<WorkTask>());
    }

    [Fact]
    public async Task ChangeStatusAsync_TaskNotFound_ThrowsBusinessRuleViolationException()
    {
        _workTaskRepository.GetByIdAsync(404).Returns((WorkTask?)null);

        var exception = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => _sut.ChangeStatusAsync(404, WorkTaskStatus.InProgress));

        Assert.Equal("Task not found", exception.Message);
    }

    [Fact]
    public async Task GetTasksByAssigneeAsync_ReturnsWhatRepositoryReturns()
    {
        var expectedTasks = new List<WorkTask>
        {
            new() { Id = 1, Title = "Task 1", AssignedToEmployeeId = 5 },
            new() { Id = 2, Title = "Task 2", AssignedToEmployeeId = 5 }
        };
        _workTaskRepository.GetByAssigneeAsync(5).Returns(expectedTasks);

        var result = await _sut.GetTasksByAssigneeAsync(5);

        Assert.Same(expectedTasks, result);
    }
}
