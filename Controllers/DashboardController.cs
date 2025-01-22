using FT_TTMS_WebApplication.Data;
using FT_TTMS_WebApplication.Models;
using FT_TTMS_WebApplication.ViewModels;
using FT_TTMS_WebApplication.Interfaces;
using FT_TTMS_WebApplication.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FT_TTMS_WebApplication.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IDayEntryRepository _dayEntryRepository;
        private readonly ILeaveEntryRepository _leaveEntryRepository;
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IJobRepository _jobRepository;
        private readonly IImportedJobRepository _importedJobRepository;
        private readonly ICreatedJobRepository _createdJobRepository;
        private readonly IDutyRepository _dutyRepository;

        public DashboardController(ApplicationDbContext context)
        {
            _userRepository = new UserRepository(context);
            _dayEntryRepository = new DayEntryRepository(context);
            _leaveEntryRepository = new LeaveEntryRepository(context);
            _timeEntryRepository = new TimeEntryRepository(context);
            _jobRepository = new JobRepository(context);
            _importedJobRepository = new ImportedJobRepository(context);
            _createdJobRepository = new CreatedJobRepository(context);
            _dutyRepository = new DutyRepository(context);
        }

        // Handles the GET request for the Dashboard index page
        [HttpGet]
        [Route("Dashboard/Index/{userId?}")]
        public async Task<IActionResult> Index(DateTime? WeekSelect = null, string userId = null)
        {
            var model = new DashboardViewModel();

            // Set the selected week in the model
            SetWeekSelect(model, WeekSelect);

            // Set the week status (previous, current, future) in the model
            SetWeekStatus(model);

            // Synchronize jobs from different repositories
            await SyncJobs();

            // Fetch all jobs and set them in the model
            model.Jobs = (await _jobRepository.GetAll()).ToList();

            // Set the last time entry for the user in the model
            await SetLastTimeEntry(model, userId);

            // Set user details in the model
            await SetUserDetails(model, userId);

            // Set entries (day, time, leave) for the user in the model
            await SetEntriesForUser(model, userId);

            // Fetch all duties and set them in the model
            model.Duties = await _dutyRepository.GetAll();

            // Return the view with the populated model
            return View(model);
        }

        // Sets the selected week in the dashboard model
        private void SetWeekSelect(DashboardViewModel model, DateTime? WeekSelect)
        {
            var today = DateTime.Today;
            var currentDayOfWeek = (int)today.DayOfWeek;

            // Calculate the previous Sunday
            var previousSunday = today.AddDays(-(currentDayOfWeek + 7) % 7 - 7);

            // If today is Monday or Tuesday, set WeekSelect to the previous Sunday
            if (currentDayOfWeek == 1 || currentDayOfWeek == 2)
            {
                model.WeekSelect = previousSunday;
            }
            else
            {
                // Otherwise, set WeekSelect to the current week's Sunday
                model.WeekSelect = StartOfWeek(today);
            }

            // If WeekSelect has a value, set it to the start of the selected week
            if (WeekSelect.HasValue)
            {
                model.WeekSelect = StartOfWeek(WeekSelect.Value);
                Console.WriteLine("WeekSelect: " + model.WeekSelect);
            }
        }

        // Sets the week status in the dashboard model
        private void SetWeekStatus(DashboardViewModel model)
        {
            model.IsPrevWeek = IsPreviousWeek(model.WeekSelect);
            model.IsCurrentWeek = IsCurrentWeek(model.WeekSelect);
            model.IsFutureWeek = IsFutureWeek(model.WeekSelect);
        }

        // Returns the start of the week (Sunday) for a given date
        public DateTime StartOfWeek(DateTime date)
        {
            int delta = DayOfWeek.Sunday - date.DayOfWeek;
            DateTime sunday = date.AddDays(delta);
            return sunday;
        }

        // Checks if the given date is in the previous week
        private bool IsPreviousWeek(DateTime date)
        {
            var today = DateTime.Today;
            var startOfWeek = StartOfWeek(today);

            return date < startOfWeek;
        }

        // Checks if the given date is in the current week
        private bool IsCurrentWeek(DateTime date)
        {
            var today = DateTime.Today;
            var startOfWeek = StartOfWeek(today);

            return date >= startOfWeek && date < startOfWeek.AddDays(7);
        }

        // Checks if the given date is in a future week
        private bool IsFutureWeek(DateTime date)
        {
            var today = DateTime.Today;
            var startOfWeek = StartOfWeek(today);

            return date >= startOfWeek.AddDays(7);
        }

        // Synchronizes jobs from imported and created job repositories with the main job repository
        private async Task SyncJobs()
        {
            // Fetch all jobs from the repositories
            var allJobs = await _jobRepository.GetAll();
            var importedJobs = await _importedJobRepository.GetAll();
            var createdJobs = await _createdJobRepository.GetAll();

            // Identify missing imported jobs
            var missingImportedJobs = importedJobs.Where(importedJob => 
                !allJobs.Any(job => job.JobNumber == importedJob.JobNumber));

            // Identify missing created jobs
            var missingCreatedJobs = createdJobs.Where(createdJob => 
                !allJobs.Any(job => job.JobNumber == createdJob.JobNumber));

            // Add missing imported jobs to the main job repository
            foreach (var job in missingImportedJobs)
            {
                await _jobRepository.CreateAsync(new Job
                {
                    JobNumber = job.JobNumber,
                    JobName = job.JobName,
                    JobNumberAndJobName = job.JobNumberAndJobName
                });
            }

            // Add missing created jobs to the main job repository
            foreach (var job in missingCreatedJobs)
            {
                await _jobRepository.CreateAsync(new Job
                {
                    JobNumber = job.JobNumber,
                    JobName = job.JobName,
                    JobNumberAndJobName = job.JobNumberAndJobName
                });
            }
        }

        // Sets the last time entry for a user in the dashboard model
        private async Task SetLastTimeEntry(DashboardViewModel model, string userId)
        {
            var lastUserTimeEntry = await _timeEntryRepository.GetLastTimeEntryByUserAsync(userId);
            model.LastTimeEntry = lastUserTimeEntry;
        }

        // Sets user details in the dashboard model
        private async Task SetUserDetails(DashboardViewModel model, string userId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user != null)
            {
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
            }
        }

        // Sets entries for a user in the dashboard model based on their role
        private async Task SetEntriesForUser(DashboardViewModel model, string userId)
        {
            bool isAdmin = User.IsInRole("admin");
            if (isAdmin && !string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("Admin user detected.");
            }
            else
            {
                Console.WriteLine("Non-admin user detected.");
            }

            model.DayEntries = await _dayEntryRepository.fetchDayEntriesByUser(userId, model.WeekSelect);
            model.TimeEntries = await _timeEntryRepository.fetchTimeEntriesByUser(userId, model.WeekSelect);
            model.LeaveEntries = await _leaveEntryRepository.FetchLeaveEntriesByUser(userId, model.WeekSelect);
        }

        [HttpPost]
        public async Task<IActionResult> ClockInOut(DashboardViewModel model)
        {
            if (model.DayEntry != null)
            {
                if (model.DayEntry.Id > 0) // Assuming Id is a non-zero value for existing entries
                {
                    // Fetch the existing entry from the database
                    var existingEntry = await _dayEntryRepository.GetByIdAsync(model.DayEntry.Id);
                    if (existingEntry != null)
                    {
                        // Update the existing entry's fields
                        existingEntry.AppUserId = model.DayEntry.AppUserId;
                        existingEntry.WeekOf = model.DayEntry.WeekOf;
                        existingEntry.Date = model.DayEntry.Date;
                        existingEntry.DayName = model.DayEntry.DayName;
                        existingEntry.DayStartTime = model.DayEntry.DayStartTime;
                        existingEntry.DayEndTime = model.DayEntry.DayEndTime;
                        existingEntry.LunchStartTime = model.DayEntry.LunchStartTime;
                        existingEntry.LunchEndTime = model.DayEntry.LunchEndTime;
                        // Add any other fields that need to be updated

                        // Save the updated entry to the database
                        await _dayEntryRepository.UpdateAsync(existingEntry);
                    }
                }
                else
                {
                    // Directly save the new DayEntry from the ViewModel to the database
                    await _dayEntryRepository.CreateAsync(model.DayEntry);
                }

                // After operations, redirect to the Index view with the correct week
                DateTime weekOfPost = StartOfWeek(model.DayEntry.Date ?? DateTime.Now);
                return RedirectToAction("Index", new { WeekSelect = weekOfPost, userId = model.DayEntry.AppUserId});
            }

            // Handle the case where DayEntry is null, possibly by returning an error message
            ModelState.AddModelError("", "DayEntry is required.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddTime(DashboardViewModel model)
        {
            if (model.TimeEntry != null)
            {
                // Check if the TimeEntry has an ID indicating it's an existing entry
                if (model.TimeEntry.Id > 0)
                {
                    // Update existing time entry
                    await _timeEntryRepository.UpdateAsync(model.TimeEntry);
                }
                else
                {
                    // Create new time entry
                    await _timeEntryRepository.CreateAsync(model.TimeEntry);
                }

                // After operations, redirect to the Index view with the correct week
                DateTime weekOfPost = StartOfWeek(model.TimeEntry.Date ?? DateTime.Now);
                return RedirectToAction("Index", new { WeekSelect = weekOfPost, userId = model.TimeEntry.AppUserId});
            }

            // Use TempData to indicate that there was a validation error
            TempData["ValidationError"] = "Please correct the errors and try again.";

            // Redirect to the Index action to ensure the model is properly prepared
            return RedirectToAction("Index");
        }

        // Deletes a time entry and redirects to the index with the appropriate week and user
        [HttpPost]
        public async Task<IActionResult> DeleteTime(int id)
        {
            var timeEntry = await _timeEntryRepository.GetByIdAsync(id);
            DateTime weekOfPost = StartOfWeek(timeEntry.Date ?? DateTime.Now);

            if (timeEntry != null)
            {
                await _timeEntryRepository.DeleteAsync(timeEntry.Id);
                TempData["SuccessMessage"] = "Time entry deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Time entry not found.";
            }

            return RedirectToAction("Index", new { WeekSelect = weekOfPost, userId = timeEntry.AppUserId });
        }

        // Deletes a day entry and redirects to the index with the appropriate week and user
        [HttpPost]
        public async Task<IActionResult> DeleteDay(int id)
        {
            var dayEntry = await _dayEntryRepository.GetByIdAsync(id);
            DateTime weekOfPost = StartOfWeek(dayEntry.Date ?? DateTime.Now);

            if (dayEntry != null)
            {
                await _dayEntryRepository.DeleteAsync(dayEntry.Id);
                TempData["SuccessMessage"] = "Day entry deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Day entry not found.";
            }

            return RedirectToAction("Index", new { WeekSelect = weekOfPost, userId = dayEntry.AppUserId });
        }


        // Adds a new job and redirects to the index with the appropriate week and user
        [HttpPost]
        public async Task<IActionResult> AddJob(DashboardViewModel model)
        {
            if (model.JobModel != null)
            {
                // Check if a job with the same job number already exists
                var existingJob = await _jobRepository.FindByJobNumberAsync(model.JobModel.JobNumber);
                if (existingJob != null)
                {
                    TempData["ErrorMessage"] = "A job with the same job number already exists.";
                    return RedirectToAction("Index", new { WeekSelect = model.WeekSelect, userId = User.FindFirstValue(ClaimTypes.NameIdentifier) });
                }

                var newJob = new CreatedJob
                {
                    JobNumber = model.JobModel.JobNumber,
                    JobName = model.JobModel.JobName,
                    JobNumberAndJobName = model.JobModel.JobNumberAndJobName
                };

                await _createdJobRepository.CreateAsync(newJob);
                TempData["SuccessMessage"] = "Job created successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Job name and number are required.";
            }

            var userIdFromIdentity = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return RedirectToAction("Index", new { WeekSelect = model.WeekSelect, userId = userIdFromIdentity });
        }

        [HttpPost]
        public async Task<IActionResult> AddLeave(DashboardViewModel model)
        {
            if (model.LeaveEntry != null)
            {

                // Check if the LeaveEntry has an ID indicating it's an existing entry
                if (model.LeaveEntry.Id > 0)
                {
                    // Update existing leave entry
                    await _leaveEntryRepository.UpdateAsync(model.LeaveEntry);
                }
                else
                {
                    // Create new leave entry
                    await _leaveEntryRepository.CreateAsync(model.LeaveEntry);
                }

                // After operations, redirect to the Index view with the correct week
                DateTime weekOfPost = StartOfWeek(model.LeaveEntry.Date ?? DateTime.Now);
                return RedirectToAction("Index", new { WeekSelect = weekOfPost, userId = model.LeaveEntry.AppUserId});
            }

            // Use TempData to indicate that there was a validation error
            TempData["ValidationError"] = "Please correct the errors and try again.";

            // Redirect to the Index action to ensure the model is properly prepared
            return RedirectToAction("Index");
        }

        // Deletes a leave entry and redirects to the index with the appropriate week and user
        [HttpPost]
        public async Task<IActionResult> DeleteLeave(int id)
        {
            var leaveEntry = await _leaveEntryRepository.GetByIdAsync(id);
            DateTime weekOfPost = StartOfWeek(leaveEntry.Date ?? DateTime.Now);

            if (leaveEntry != null)
            {
                await _leaveEntryRepository.DeleteAsync(leaveEntry.Id);
                TempData["SuccessMessage"] = "Leave entry deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Leave entry not found.";
            }

            return RedirectToAction("Index", new { WeekSelect = weekOfPost, userId = leaveEntry.AppUserId });
        }

    }
}