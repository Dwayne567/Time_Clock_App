using FT_TTMS_WebApplication.Models;

namespace FT_TTMS_WebApplication.ViewModels
{
    // ViewModel for Dashboard
    public class DashboardViewModel
    {
        // First Name Last Name
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        // Week Select
        public DateTime WeekSelect { get; set; }
        public bool IsCurrentWeek { get; set; }
        public bool IsPrevWeek { get; set; }
        public bool IsFutureWeek { get; set; }

        // Day Entry
        public DayEntry? DayEntry { get; set; }
        public IEnumerable<DayEntry> DayEntries { get; set; } = new List<DayEntry>();
        public IEnumerable<DayEntry> UserDayEntries { get; set; } = new List<DayEntry>();

        // Time Entry
        public TimeEntry? TimeEntry { get; set; }
        public IEnumerable<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
        public IEnumerable<TimeEntry> UserTimeEntries { get; set; } = new List<TimeEntry>();
        public TimeEntry LastTimeEntry { get; set; }

        // Leave Entry
        public LeaveEntry? LeaveEntry { get; set; }
        public IEnumerable<LeaveEntry> LeaveEntries { get; set; } = new List<LeaveEntry>();
        public IEnumerable<LeaveEntry> UserLeaveEntries { get; set; } = new List<LeaveEntry>();

        // Job
        public string? Job { get; set; }
        public Job? JobModel { get; set; }
        public IEnumerable<Job> Jobs { get; set; } = new List<Job>();

        // Imported Job
        public string? ImportedJob { get; set; }
        public ImportedJob? ImportedJobModel { get; set; }
        public IEnumerable<ImportedJob> ImportedJobs { get; set; } = new List<ImportedJob>();

        // Created Job
        public string? CreatedJob { get; set; }
        public CreatedJob? CreatedJobModel { get; set; }
        public IEnumerable<CreatedJob> CreatedJobs { get; set; } = new List<CreatedJob>();

        // Duty
        public string? Duty { get; set; }
        public IEnumerable<Duty> Duties { get; set; } = new List<Duty>();

        public DashboardViewModel()
        {
            Jobs = new List<Job>();
            Duties = new List<Duty>();
            WeekSelect = DateTime.Now; // Default to current date
            //SetWeekStatus();
        }

        // Get the start of the week for a given date
        private DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        // Set the status of the week (current, previous, future)
        private void SetWeekStatus()
        {
            DateTime startOfCurrentWeek = StartOfWeek(DateTime.Now, DayOfWeek.Monday);
            DateTime startOfSelectedWeek = StartOfWeek(WeekSelect, DayOfWeek.Monday);

            IsCurrentWeek = startOfSelectedWeek == startOfCurrentWeek;
            IsPrevWeek = startOfSelectedWeek == startOfCurrentWeek.AddDays(-7);
            IsFutureWeek = startOfSelectedWeek > startOfCurrentWeek;
        }

        // Calculate the total duration of a day
        public double CalculateDayDuration(DateTime day)
        {
            if (DayEntries == null) return 0.0;

            var dayStart = day.Date;
            var dayEnd = dayStart.AddDays(1);

            var totalDuration = DayEntries
                .Where(entry => entry.Date >= dayStart && entry.Date < dayEnd)
                .Aggregate(TimeSpan.Zero, (sum, entry) =>
                {
                    var start = entry.DayStartTime ?? TimeSpan.Zero;
                    var end = entry.DayEndTime ?? TimeSpan.Zero;
                    // Check if the end time is before the start time, indicating an overnight span
                    if (end < start)
                    {
                        end = end.Add(TimeSpan.FromDays(1));
                    }
                    return sum.Add(end - start);
                });

            var totalMinutes = (double)totalDuration.TotalMinutes;
            var roundedMinutes = Math.Round(totalMinutes / 15) * 15;
            return roundedMinutes / 60;
        }

        // Calculate the total lunch duration of a day
        public double CalculateLunchDuration(DateTime day)
        {
            if (DayEntries == null) return 0.0;

            var dayStart = day.Date;
            var dayEnd = dayStart.AddDays(1);

            var totalLunchDuration = DayEntries
                .Where(entry => entry.Date >= dayStart && entry.Date < dayEnd)
                .Aggregate(TimeSpan.Zero, (sum, entry) =>
                {
                    var start = entry.LunchStartTime ?? TimeSpan.Zero;
                    var end = entry.LunchEndTime ?? TimeSpan.Zero;
                    // Check if the lunch end time is before the lunch start time, indicating an overnight span
                    if (end < start)
                    {
                        end = end.Add(TimeSpan.FromDays(1));
                    }
                    return sum.Add(end - start);
                });

            var totalMinutes = (double)totalLunchDuration.TotalMinutes;
            var roundedMinutes = Math.Round(totalMinutes / 15) * 15;
            return roundedMinutes / 60;
        }

        // Calculate the total work duration of a day
        public double CalculateWorkDuration(DateTime day)
        {
            // Calculate total day duration
            double dayDuration = CalculateDayDuration(day);
            // Calculate lunch duration
            double lunchDuration = CalculateLunchDuration(day);

            // Subtract lunch duration from day duration to get work duration
            double workDuration = dayDuration - lunchDuration;

            return workDuration;
        }

        // Calculate the total task duration of a day
        public double CalculateTaskDuration(DateTime day)
        {
            var totalDuration = 0.0;

            if (TimeEntries != null && TimeEntries.Any())
            {
                var dayStart = day.Date;
                var dayEnd = dayStart.AddDays(1);

                // Adjusted to handle nullable Duration values
                totalDuration = TimeEntries
                    .Where(entry => entry.Date.HasValue && entry.Date.Value >= dayStart && entry.Date.Value < dayEnd)
                    .Sum(entry => entry.Duration.HasValue ? entry.Duration.Value : 0.0);

                Console.WriteLine($"Total duration from TimeEntries: {totalDuration}");
            }
            else
            {
                Console.WriteLine("No TimeEntries found.");
            }

            // Include LeaveEntries with LeaveType == "PTO"
            if (LeaveEntries != null && LeaveEntries.Any())
            {
                var dayStart = day.Date;
                var dayEnd = dayStart.AddDays(1);

                var ptoDuration = LeaveEntries
                    .Where(entry => entry.Date.HasValue && entry.Date.Value >= dayStart && entry.Date < dayEnd && entry.LeaveType == "PTO")
                    .Sum(entry => entry.LeaveDuration.HasValue ? entry.LeaveDuration.Value : 0.0);

                Console.WriteLine($"PTO duration from LeaveEntries: {ptoDuration}");

                totalDuration += ptoDuration;
            }
            else
            {
                Console.WriteLine("No LeaveEntries found or no PTO entries found.");
            }

            Console.WriteLine($"Total calculated task duration: {totalDuration}");
            return totalDuration;
        }

        // Calculate the total leave duration of a day
        public double CalculateLeaveDuration(DateTime day)
        {
            if (LeaveEntries == null || !LeaveEntries.Any()) return 0.0;

            var dayStart = day.Date;
            var dayEnd = dayStart.AddDays(1);

            // Adjusted to handle nullable Duration values
            var totalDuration = LeaveEntries
                .Where(entry => entry.Date.HasValue && entry.Date.Value >= dayStart && entry.Date < dayEnd)
                .Sum(entry => entry.LeaveDuration.HasValue ? entry.LeaveDuration.Value : 0.0);

            return totalDuration;
        }

        // Calculate the total hours for a week
        public double CalculateTotalHoursForWeek(DateTime weekOf)
        {
            var startOfWeek = weekOf.Date.AddDays(-(int)weekOf.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            // Calculate hours from time entries
            var totalHours = TimeEntries
                .Where(entry => entry.Date.HasValue && entry.Date.Value >= startOfWeek && entry.Date < endOfWeek)
                .Sum(entry => entry.Duration.HasValue ? entry.Duration.Value : 0.0);

            // Assuming LeaveEntries is a similar collection to TimeEntries
            // Calculate leave hours within the same week, excluding "UPTO" leave type
            var leaveHours = LeaveEntries
                .Where(entry => entry.Date.HasValue && entry.Date.Value >= startOfWeek && entry.Date < endOfWeek && entry.LeaveType != "UPTO")
                .Sum(entry => entry.LeaveDuration.HasValue ? entry.LeaveDuration.Value : 0.0);

            // Add leave hours to total hours
            totalHours += leaveHours;

            return totalHours;
        }
    }
}