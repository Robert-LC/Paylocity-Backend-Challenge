using Api.Models;

namespace Api.Interfaces.Repositories
{
    public interface IDependentRepository
    {
        Task<Dependent> Add(Dependent dependent);
        Task<Dependent> GetById(int id);
        Task<ICollection<Dependent>> GetAll();
    }
}
