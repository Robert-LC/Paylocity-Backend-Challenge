using Api.Dtos.Employee;
using Api.Interfaces.Services;

namespace Api.Services
{
    public class EmployeeService : IEmployeeService
    {
        public Task<GetEmployeeDto> Get(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<GetEmployeeDto>> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
