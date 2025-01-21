using Api.Models;

namespace Api.Interfaces.Repositories
{
    public interface IPaycheckRepository
    {
        Task<Paycheck> Add(Paycheck paycheck);
        Task<Paycheck> GetById(int paycheckId);
        Task<ICollection<Paycheck>> GetAll();
        Task<ICollection<Paycheck>> GetAllByEmployeeId(int employeeId);
    }
}
