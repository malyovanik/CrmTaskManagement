using CrmTaskManagement.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmTaskManagement.Data.Repositories;

public class WorkTaskRepository : IWorkTaskRepository
{
    private readonly AppDbContext _context;

    public WorkTaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(WorkTask task)
    {
        await _context.WorkTasks.AddAsync(task);
        await _context.SaveChangesAsync();
    }

    public async Task<WorkTask?> GetByIdAsync(int id)
    {
        return await _context.WorkTasks.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<WorkTask>> GetByAssigneeAsync(int employeeId)
    {
        return await _context.WorkTasks
            .Where(t => t.AssignedToEmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task UpdateAsync(WorkTask task)
    {
        _context.WorkTasks.Update(task);
        await _context.SaveChangesAsync();
    }
}
