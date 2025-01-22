﻿using FT_TTMS_WebApplication.Models;

namespace FT_TTMS_WebApplication.Interfaces
{
    // Interface for Job repository
    public interface IJobRepository
    {
        Task<IEnumerable<Job>> GetAll();
        Task<Job> CreateAsync(Job job);

        bool Add(Job job);

        bool Update(Job job);

        bool Delete(Job job);

        bool Save();

        Task<Job> FindByJobNumberAsync(string jobNumber);
    }
}