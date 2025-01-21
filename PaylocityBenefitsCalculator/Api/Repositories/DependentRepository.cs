
using Api.Interfaces.Repositories;
using Api.Models;

namespace Api.Repositories
{
    public class DependentRepository : IDependentRepository
    {
        /*
         * In this case I returned the object back because for calls like Add, an ORM like EF would auto generate the new ID for me
         * and you want the whole object back so that can be displayed or processed
         * 
         * If I was using stored procedures instead I might change these repo-db calls to return a bool
         */

        public Task<Dependent> Add(Dependent dependent)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Dependent>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<Dependent> GetById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
