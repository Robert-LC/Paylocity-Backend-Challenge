using Api.Models;

namespace Api.Interfaces.Services
{
    public interface IPaycheckService
    {
        Task<Paycheck> Create();
        Task<Paycheck> Get(int paycheckId);
        Task<ICollection<Paycheck>> GetPaychecksByEmployee(int employeeId);
        
        // I might put these in a PaycheckHelper/PaycheckCreator class instead once I see how implementation works together
        Task<decimal> CalcGrossPay();
        Task<decimal> CalcDeductions();
    }
}
