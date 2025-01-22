using FT_TTMS_WebApplication.Models;

namespace FT_TTMS_WebApplication.Interfaces
{
    // Interface for ImportedJob repository
    public interface IImportedJobRepository
    {
        Task<IEnumerable<ImportedJob>> GetAll();
        Task<ImportedJob> CreateAsync(ImportedJob job);

        bool Add(ImportedJob job);

        bool Update(ImportedJob job);

        bool Delete(ImportedJob job);

        bool Save();
    }
}