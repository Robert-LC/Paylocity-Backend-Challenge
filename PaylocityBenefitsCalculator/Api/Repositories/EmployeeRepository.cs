
using Api.Interfaces.Repositories;
using Api.Models;

namespace Api.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        public Task<ICollection<Employee>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<Employee> GetById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
