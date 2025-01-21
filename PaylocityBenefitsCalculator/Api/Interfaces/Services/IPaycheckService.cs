using Api.Dtos.Paycheck;
using Api.Models;

namespace Api.Interfaces.Services
{
    public interface IPaycheckService
    {
        Task<ApiResponse<AddPaycheckDto>> Create(int employeeId);
        Task<ApiResponse<GetPaycheckDto>> Get(int paycheckId);
        Task<ApiResponse<ICollection<GetPaycheckDto>>> GetPaychecksByEmployee(int employeeId);
    }
}
