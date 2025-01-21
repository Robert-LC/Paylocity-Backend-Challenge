using Api.Models;

namespace Api.Interfaces.Repositories
{
    public interface IEmployeeRepository
    {
        Task<Employee> GetById(int id);
        Task<ICollection<Employee>> GetAll();
    }
}
