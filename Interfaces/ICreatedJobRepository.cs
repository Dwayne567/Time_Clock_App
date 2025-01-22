using FT_TTMS_WebApplication.Models;

namespace FT_TTMS_WebApplication.Interfaces
{
    // Interface for CreatedJob repository
    public interface ICreatedJobRepository
    {
        Task<IEnumerable<CreatedJob>> GetAll();
        Task<CreatedJob> CreateAsync(CreatedJob createdJob);

        bool Add(CreatedJob createdJob);

        bool Update(CreatedJob createdJob);

        bool Delete(CreatedJob createdJob);

        bool Save();

        Task<bool> JobExists(string jobNumber);
    }
}