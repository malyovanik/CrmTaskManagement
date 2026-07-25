using CrmTaskManagement.Data.Entities;

namespace CrmTaskManagement.Data.Repositories;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id);
}
