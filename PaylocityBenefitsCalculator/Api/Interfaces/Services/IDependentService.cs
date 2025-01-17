using Api.Dtos.Dependent;
using Api.Models;

namespace Api.Interfaces.Services
{
    public interface IDependentService
    {
        /* 
         * I like returning the whole Obj instead of only Id on Adds
         * because it gives the API flexibility to return obj to client for confirmation/displaying data if need arises
        */
        Task<Dependent> Add(AddDependentDto dependentDto, int employeeId);
        Task<Dependent> Get(int dependentId);
        Task<ICollection<Dependent>> GetAll();
        Task<ICollection<Dependent>> GetAllDependentsByEmployee(int employeeId);

        // Requirements specifically only mention view and inadvertadly create
        // Would also add Update & Delete in real-life setting
    }
}
