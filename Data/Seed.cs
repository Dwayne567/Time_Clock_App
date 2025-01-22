using FT_TTMS_WebApplication.Models;
using Microsoft.AspNetCore.Identity;
using FT_TTMS_WebApplication.Utils;
using CsvHelper;
using System.Globalization;

namespace FT_TTMS_WebApplication.Data
{
    public class Seed
    {
        // Seeds jobs from a CSV file into the database
        public static async Task SeedJobsAsync(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var jobs = ReadCsvFile("FiberTrakJobTrackerPBCISS_08_22_24.csv");

            foreach (var job in jobs)
            {
                context.ImportedJobs.Add(job);
            }

            await context.SaveChangesAsync();
        }

        // Reads jobs from a CSV file and returns a list of ImportedJob objects
        private static List<ImportedJob> ReadCsvFile(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            var jobs = new List<ImportedJob>();

            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            while (csv.Read())
            {
                var jobNumber = csv.GetField(0);
                var jobName = csv.GetField(1);
                var jobNumberAndJobName = $"{jobNumber} - {jobName}";

                var job = new ImportedJob
                {
                    JobNumber = jobNumber,
                    JobName = jobName,
                    JobNumberAndJobName = jobNumberAndJobName
                };

                jobs.Add(job);
            }

            return jobs;
        }

        // Seeds users and roles into the database
        public static async void SeedUsersAndRolesAsync(IApplicationBuilder applicationBuilder)
        {
            using var serviceScope = applicationBuilder.ApplicationServices.CreateScope();

            // Roles
            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await roleManager.RoleExistsAsync(UserRoles.User))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            // Users
            var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            await CreateUserAsync(userManager, "admin1@test.com", "AdminFirstName1", "AdminLastName1", 1234, "admin1", UserRoles.Admin);
            await CreateUserAsync(userManager, "admin2@test.com", "AdminFirstName2", "AdminLastName2", 5678, "admin2", UserRoles.Admin);
            await CreateUserAsync(userManager, "user1@test.com", "UserFirstName1", "UserLastName1", 9876, "user1", UserRoles.User);
            await CreateUserAsync(userManager, "user2@test.com", "UserFirstName2", "UserLastName2", 5432, "user2", UserRoles.User);
            await CreateUserAsync(userManager, "user3@test.com", "UserFirstName3", "UserLastName3", 3456, "user3", UserRoles.User);
            await CreateUserAsync(userManager, "user4@test.com", "UserFirstName4", "UserLastName4", 6789, "user4", UserRoles.User);
            await CreateUserAsync(userManager, "user5@test.com", "UserFirstName5", "UserLastName5", 2345, "user5", UserRoles.User);
            await CreateUserAsync(userManager, "user6@test.com", "UserFirstName6", "UserLastName6", 4567, "user6", UserRoles.User);
        }

        // Helper method to create a user and assign a role
        private static async Task CreateUserAsync(UserManager<AppUser> userManager, string email, string firstName, string lastName, int employeeNumber, string userName, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                var newUser = new AppUser
                {
                    FirstName = firstName,
                    LastName = lastName,
                    EmployeeNumber = employeeNumber,
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true,
                    Group = role == UserRoles.Admin ? "Admin" : "FiberTrak"
                };

                var result = await userManager.CreateAsync(newUser, "Coding@1234?");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, role);
                }
            }
        }
    }
}