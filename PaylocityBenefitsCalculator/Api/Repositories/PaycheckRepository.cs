
using Api.Interfaces.Repositories;
using Api.Models;

namespace Api.Repositories
{
    public class PaycheckRepository : IPaycheckRepository
    {
        /// <summary>
        /// Adds the paycheck to the db
        /// </summary>
        /// <param name="paycheck"></param>
        /// <returns>The paycheck that was added</returns>
        public Task<Paycheck> Add(Paycheck paycheck)
        {
            throw new NotImplementedException();
            // assuming we used Entity Framework ORM
            // await _context.Paychecks.AddAsync(paycheck)
            // await _context.SaveChangesAsync()
        }

        public Task<ICollection<Paycheck>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Paycheck>> GetAllByEmployeeId(int employeeId)
        {
            throw new NotImplementedException();
        }

        public Task<Paycheck> GetById(int paycheckId)
        {
            throw new NotImplementedException();
        }
    }
}
