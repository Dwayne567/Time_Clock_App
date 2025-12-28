using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Models;
using Timeclock_WebApplication.ViewModels;
using Timeclock_WebApplication.Interfaces;
using Timeclock_WebApplication.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace Timeclock_WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IDayEntryRepository _dayEntryRepository;
        private readonly ILeaveEntryRepository _leaveEntryRepository;
        private readonly ITaskEntryRepository _taskEntryRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ITaskRepository _taskRepository;

        public DashboardController(
            IUserRepository userRepository,
            IDayEntryRepository dayEntryRepository,
            ILeaveEntryRepository leaveEntryRepository,
            ITaskEntryRepository taskEntryRepository,
            IJobRepository jobRepository,
            ITaskRepository taskRepository)
        {
            _userRepository = userRepository;
            _dayEntryRepository = dayEntryRepository;
            _leaveEntryRepository = leaveEntryRepository;
            _taskEntryRepository = taskEntryRepository;
            _jobRepository = jobRepository;
            _taskRepository = taskRepository;
        }

        // GET: api/Dashboard/Index
        [HttpGet("Index")]
        public async Task<IActionResult> Index(DateTime? WeekSelect = null, string? userId = null)
        {
            try 
            {
                if (string.IsNullOrEmpty(userId))
                {
                    userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                }

                Console.WriteLine($"Dashboard Index Request. UserId: {userId ?? "NULL"}, IsAuth: {User.Identity?.IsAuthenticated ?? false}");

                var model = new DashboardViewModel();
                model.CurrentUserId = userId;
                model.IsAdmin = User.IsInRole(UserRoles.Admin);

                // Set the selected week in the model
                SetWeekSelect(model, WeekSelect);

                // Set the week status (previous, current, future) in the model
                SetWeekStatus(model);

                // Fetch all jobs and set them in the model
                model.Jobs = (await _jobRepository.GetAll()).ToList();

                if (!string.IsNullOrEmpty(userId))
                {
                    // Set the last task entry for the user in the model
                    await SetLastTaskEntry(model, userId);

                    // Set user details in the model
                    await SetUserDetails(model, userId);

                    // Set entries (day, time, leave) for the user in the model
                    await SetEntriesForUser(model, userId);
                }
                else 
                {
                     Console.WriteLine("UserId is null/empty. Skipping user-specific data.");
                }

                // Fetch all duties and set them in the model
                model.Tasks = await _taskRepository.GetAll();

                if (model.IsAdmin)
                {
                    var allUsers = await _userRepository.GetAllUsers();
                    model.Groups = allUsers
                        .Where(u => !string.IsNullOrWhiteSpace(u.Group))
                        .Select(u => u.Group!)
                        .Distinct()
                        .OrderBy(g => g)
                        .ToList();
                }

                // Calculate separate totals for weekly summary
                var startOfWeek = model.WeekSelect.Date;
                var endOfWeek = startOfWeek.AddDays(7);

                // Day Entry Total - calculate from start/end times
                model.DayEntryTotalHours = model.DayEntries
                    .Where(e => e.Date.HasValue && e.Date.Value >= startOfWeek && e.Date.Value < endOfWeek)
                    .Sum(e => {
                        if (e.DayStartTime.HasValue && e.DayEndTime.HasValue)
                        {
                            return (e.DayEndTime.Value - e.DayStartTime.Value).TotalHours;
                        }
                        return e.WorkDuration ?? 0;
                    });

                // Task Entry Total - sum of Duration for all task entries this week
                model.TaskEntryTotalHours = model.TaskEntries
                    .Where(e => e.Date.HasValue && e.Date.Value >= startOfWeek && e.Date.Value < endOfWeek)
                    .Sum(e => e.Duration ?? 0);

                // Leave Entry Total - sum of LeaveDuration for all leave entries this week
                model.LeaveEntryTotalHours = model.LeaveEntries
                    .Where(e => e.Date.HasValue && e.Date.Value >= startOfWeek && e.Date.Value < endOfWeek)
                    .Sum(e => e.LeaveDuration ?? 0);

                // Total hours is sum of all
                model.TotalHours = model.DayEntryTotalHours + model.TaskEntryTotalHours + model.LeaveEntryTotalHours;

                // Return the populated model as JSON
                return Ok(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in Dashboard Index: {ex.Message} \n {ex.StackTrace}");
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
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
                model.WeekSelect = today.StartOfWeek(DayOfWeek.Sunday);
            }

            // If WeekSelect has a value, set it to the start of the selected week
            if (WeekSelect.HasValue)
            {
                model.WeekSelect = WeekSelect.Value.StartOfWeek(DayOfWeek.Sunday);
                // Console.WriteLine("WeekSelect: " + model.WeekSelect);
            }
        }

        // Sets the week status in the dashboard model
        private void SetWeekStatus(DashboardViewModel model)
        {
            var today = DateTime.Today;
            var startOfCurrentWeek = today.StartOfWeek(DayOfWeek.Sunday);

            model.IsPrevWeek = model.WeekSelect < startOfCurrentWeek;
            model.IsCurrentWeek = model.WeekSelect >= startOfCurrentWeek && model.WeekSelect < startOfCurrentWeek.AddDays(7);
            model.IsFutureWeek = model.WeekSelect >= startOfCurrentWeek.AddDays(7);
        }

        // Sets the last task entry for a user in the dashboard model
        private async Task SetLastTaskEntry(DashboardViewModel model, string userId)
        {
            var lastUserTaskEntry = await _taskEntryRepository.GetLastTaskEntryByUserAsync(userId);
            model.LastTaskEntry = lastUserTaskEntry;
        }

        // Sets user details in the dashboard model
        private async Task SetUserDetails(DashboardViewModel model, string userId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user != null)
            {
                Console.WriteLine($"SetUserDetails found user: {user.UserName}. FirstName: '{user.FirstName}', LastName: '{user.LastName}'");
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
            }
            else 
            {
                Console.WriteLine($"SetUserDetails: User not found for ID {userId}");
            }
        }

        // Sets entries for a user in the dashboard model based on their role
        private async Task SetEntriesForUser(DashboardViewModel model, string userId)
        {
            // Logging removed for brevity unless needed
            
            model.DayEntries = await _dayEntryRepository.fetchDayEntriesByUser(userId, model.WeekSelect);
            model.TaskEntries = await _taskEntryRepository.FetchTaskEntriesByUser(userId, model.WeekSelect);
            model.LeaveEntries = await _leaveEntryRepository.FetchLeaveEntriesByUser(userId, model.WeekSelect);
        }

        [HttpPost("ClockInOut")]
        public async Task<IActionResult> ClockInOut([FromBody] DashboardViewModel model)
        {
            if (model.DayEntry == null)
            {
                return BadRequest("DayEntry is required.");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(model.DayEntry.AppUserId) && !string.IsNullOrEmpty(currentUserId))
            {
                model.DayEntry.AppUserId = currentUserId;
            }

            // Normalize the provided date information so the week filtering logic can find the entry again.
            var entryDate = model.DayEntry.Date?.Date ?? DateTime.Today;
            model.DayEntry.Date = entryDate;
            model.DayEntry.DayName ??= entryDate.DayOfWeek.ToString();

            if (model.DayEntry.WeekOf.HasValue)
            {
                model.DayEntry.WeekOf = model.DayEntry.WeekOf.Value.StartOfWeek(DayOfWeek.Sunday);
            }
            else
            {
                model.DayEntry.WeekOf = entryDate.StartOfWeek(DayOfWeek.Sunday);
            }

            // Calculate durations if start and end times are provided
            if (model.DayEntry.DayStartTime.HasValue && model.DayEntry.DayEndTime.HasValue)
            {
                var dayDuration = (model.DayEntry.DayEndTime.Value - model.DayEntry.DayStartTime.Value).TotalHours;
                model.DayEntry.DayDuration = dayDuration;

                // Calculate lunch duration if lunch times are provided
                double lunchDuration = 0;
                if (model.DayEntry.LunchStartTime.HasValue && model.DayEntry.LunchEndTime.HasValue)
                {
                    lunchDuration = (model.DayEntry.LunchEndTime.Value - model.DayEntry.LunchStartTime.Value).TotalHours;
                    model.DayEntry.LunchDuration = lunchDuration;
                }

                // Work duration = Day duration - Lunch duration
                model.DayEntry.WorkDuration = dayDuration - lunchDuration;
            }

            if (model.DayEntry.Id > 0)
            {
                var existingEntry = await _dayEntryRepository.GetByIdAsync(model.DayEntry.Id);
                if (existingEntry == null)
                {
                    return NotFound("Day entry not found.");
                }

                existingEntry.AppUserId = model.DayEntry.AppUserId;
                existingEntry.WeekOf = model.DayEntry.WeekOf;
                existingEntry.Date = model.DayEntry.Date;
                existingEntry.DayName = model.DayEntry.DayName;
                existingEntry.DayStartTime = model.DayEntry.DayStartTime;
                existingEntry.DayEndTime = model.DayEntry.DayEndTime;
                existingEntry.LunchStartTime = model.DayEntry.LunchStartTime;
                existingEntry.LunchEndTime = model.DayEntry.LunchEndTime;
                existingEntry.DayDuration = model.DayEntry.DayDuration;
                existingEntry.LunchDuration = model.DayEntry.LunchDuration;
                existingEntry.WorkDuration = model.DayEntry.WorkDuration;
                existingEntry.Status = model.DayEntry.Status;

                await _dayEntryRepository.UpdateAsync(existingEntry);
            }
            else
            {
                await _dayEntryRepository.CreateAsync(model.DayEntry);
            }

            return Ok(new { message = "Clock In/Out successful" });
        }

        [HttpPost("AddTaskEntry")]
        public async Task<IActionResult> AddTaskEntry([FromBody] DashboardViewModel model)
        {
            if (model.TaskEntry != null)
            {
                if (model.TaskEntry.Id > 0)
                {
                    await _taskEntryRepository.UpdateAsync(model.TaskEntry);
                }
                else
                {
                    await _taskEntryRepository.CreateAsync(model.TaskEntry);
                }

                return Ok("Task entry added/updated successfully");
            }

            return BadRequest("TaskEntry is required.");
        }

        [HttpDelete("DeleteTaskEntry/{id}")]
        public async Task<IActionResult> DeleteTaskEntry(int id)
        {
            var taskEntry = await _taskEntryRepository.GetByIdAsync(id);
            if (taskEntry != null)
            {
                await _taskEntryRepository.DeleteAsync(taskEntry.Id);
                return Ok("Task entry deleted successfully.");
            }
            return NotFound("Task entry not found.");
        }

        [HttpDelete("DeleteDay/{id}")]
        public async Task<IActionResult> DeleteDay(int id)
        {
            var dayEntry = await _dayEntryRepository.GetByIdAsync(id);
            if (dayEntry != null)
            {
                await _dayEntryRepository.DeleteAsync(dayEntry.Id);
                return Ok("Day entry deleted successfully.");
            }
            return NotFound("Day entry not found.");
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost("AddJob")]
        public async Task<IActionResult> AddJob([FromBody] DashboardViewModel model)
        {
            if (model.JobModel != null && !string.IsNullOrWhiteSpace(model.JobModel.JobNumber))
            {
                var existingJob = await _jobRepository.FindByJobNumberAsync(model.JobModel.JobNumber);
                if (existingJob != null)
                {
                    return BadRequest("A job with the same job number already exists.");
                }

                var newJob = new Job
                {
                    JobNumber = model.JobModel.JobNumber,
                    JobName = model.JobModel.JobName,
                    JobNumberAndJobName = model.JobModel.JobNumberAndJobName
                };

                await _jobRepository.CreateAsync(newJob);
                return Ok("Job created successfully.");
            }

            return BadRequest("Job name and number are required.");
        }

        [HttpGet("ExportTimeSheet")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> ExportTimeSheet([FromQuery] string group, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            if (string.IsNullOrWhiteSpace(group))
            {
                return BadRequest("Group is required.");
            }

            var entries = await _taskEntryRepository.FetchByGroupAndDateRangeAsync(group, fromDate, toDate);
            var sb = new StringBuilder();
            sb.AppendLine("Employee,Email,Group,Date,Job,Task,Duration,Comment");

            foreach (var entry in entries)
            {
                var employeeName = entry.AppUser != null
                    ? $"{entry.AppUser.FirstName} {entry.AppUser.LastName}".Trim()
                    : string.Empty;
                var email = entry.AppUser?.Email ?? string.Empty;
                var groupValue = entry.AppUser?.Group ?? string.Empty;
                var date = entry.Date?.ToString("yyyy-MM-dd") ?? string.Empty;
                var job = entry.Job?.JobNumberAndJobName ?? string.Empty;
                var task = entry.TaskName ?? string.Empty;
                var duration = entry.Duration?.ToString("0.##") ?? string.Empty;
                var comment = entry.Comment ?? string.Empty;

                sb.AppendLine(string.Join(",", new[]
                {
                    EscapeForCsv(employeeName),
                    EscapeForCsv(email),
                    EscapeForCsv(groupValue),
                    EscapeForCsv(date),
                    EscapeForCsv(job),
                    EscapeForCsv(task),
                    EscapeForCsv(duration),
                    EscapeForCsv(comment)
                }));
            }

            var fileName = $"timesheet_{group}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", fileName);
        }

        [HttpPost("AddLeave")]
        public async Task<IActionResult> AddLeave([FromBody] DashboardViewModel model)
        {
            if (model.LeaveEntry != null)
            {
                if (model.LeaveEntry.Id > 0)
                {
                    await _leaveEntryRepository.UpdateAsync(model.LeaveEntry);
                }
                else
                {
                    await _leaveEntryRepository.CreateAsync(model.LeaveEntry);
                }

                return Ok("Leave entry added/updated successfully");
            }

            return BadRequest("LeaveEntry is required.");
        }

        [HttpDelete("DeleteLeave/{id}")]
        public async Task<IActionResult> DeleteLeave(int id)
        {
            var leaveEntry = await _leaveEntryRepository.GetByIdAsync(id);
            if (leaveEntry != null)
            {
                await _leaveEntryRepository.DeleteAsync(leaveEntry.Id);
                return Ok("Leave entry deleted successfully.");
            }
            return NotFound("Leave entry not found.");
        }

        private static string EscapeForCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.Contains('"'))
            {
                value = value.Replace("\"", "\"\"");
            }

            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            {
                return $"\"{value}\"";
            }

            return value;
        }
    }
}