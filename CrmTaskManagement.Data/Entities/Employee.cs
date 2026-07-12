namespace CrmTaskManagement.Data.Entities;

public class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Position { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<WorkTask> AssignedTasks { get; set; } = [];
    public ICollection<WorkTask> CreatedTasks { get; set; } = [];
}
