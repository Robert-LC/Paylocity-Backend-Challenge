using Api.Dtos.Employee;

namespace Api.Interfaces.Services
{
    public interface IEmployeeService
    {
        Task<GetEmployeeDto> Get(int id);
        Task<ICollection<GetEmployeeDto>> GetAll();
    }
}
