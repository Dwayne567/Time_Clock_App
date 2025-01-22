using FT_TTMS_WebApplication.Models;

namespace FT_TTMS_WebApplication.Interfaces
{
    // Interface for Duty repository
    public interface IDutyRepository
    {
        Task<IEnumerable<Duty>> GetAll();

        Task<Duty> GetByIdAsync(int id);

        bool Add(Duty duty);

        bool Update(Duty duty);

        bool Delete(Duty duty);

        bool Save();
    }
}