using Api.Models;

namespace Api.Interfaces.Services
{
    public interface IEmployeeService
    {
        Task<Employee> Get(int id);
        Task<ICollection<Employee>> GetAll();
    }
}
