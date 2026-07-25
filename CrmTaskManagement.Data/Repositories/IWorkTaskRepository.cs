using CrmTaskManagement.Data.Entities;

namespace CrmTaskManagement.Data.Repositories;

public interface IWorkTaskRepository
{
    Task AddAsync(WorkTask task);
    Task<WorkTask?> GetByIdAsync(int id);
    Task<IEnumerable<WorkTask>> GetByAssigneeAsync(int employeeId);
    Task UpdateAsync(WorkTask task);
}
