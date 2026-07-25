namespace CrmTaskManagement.Data.Entities;

public class WorkTask
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public WorkTaskStatus Status { get; set; }
    public DateTime PlannedStartAt { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int CreatedByEmployeeId { get; set; }
    public int AssignedToEmployeeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? ParentTaskId { get; set; }

    public Employee CreatedBy { get; set; } = null!;
    public Employee AssignedTo { get; set; } = null!;
    public WorkTask? ParentTask { get; set; }
    public ICollection<WorkTask> SubTasks { get; set; } = new List<WorkTask>();
}
