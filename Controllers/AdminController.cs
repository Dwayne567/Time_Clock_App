using System.Drawing;
using System.Globalization;
using FT_TTMS_WebApplication.Data;
using FT_TTMS_WebApplication.Interfaces;
using FT_TTMS_WebApplication.Models;
using FT_TTMS_WebApplication.Repository;
using FT_TTMS_WebApplication.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace FT_TTMS_WebApplication.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IUserRepository _userRepository;

        // Constructor to initialize the database context and user repository
        public AdminController(ApplicationDbContext dbContext, IUserRepository userRepository)
        {
            _dbContext = dbContext;
            _userRepository = userRepository;
        }

        // Action method to display the admin index page
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllUsers();
            var viewModel = new AdminViewModel { Users = users };
            return View(viewModel);
        }

        // Action method to export user data to an Excel file
        public async Task<IActionResult> ExportToExcel(string group, DateTime startDate, DateTime endDate)
        {
            var model = new DashboardViewModel();

            // Set the license context for EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Get users by group
            var users = await _userRepository.GetUsersByGroup(group);

            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                // Add title to the worksheet
                worksheet.Cells[1, 1].Value = "All Employees Time by Date Range";
                worksheet.Cells[1, 1, 1, 9].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 20;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Set column widths
                worksheet.Column(1).Width = 50;
                worksheet.Column(2).Width = 10;
                worksheet.Column(3).Width = 35;
                worksheet.Column(4).Width = 10;
                worksheet.Column(5).Width = 40;
                worksheet.Column(6).Width = 40;
                worksheet.Column(7).Width = 10;
                worksheet.Column(8).Width = 10;

                // Add header row
                worksheet.Cells[2, 1].Value = "Employee";
                worksheet.Cells[2, 2].Value = "Date";
                worksheet.Cells[2, 3].Value = "Job No";
                worksheet.Cells[2, 4].Value = "Hours";
                worksheet.Cells[2, 5].Value = "Task";
                worksheet.Cells[2, 6].Value = "Description";
                worksheet.Cells[2, 7].Value = "Per Diem";
                worksheet.Cells[2, 8].Value = "Day Total";

                // Style the header row
                using (var range = worksheet.Cells[2, 1, 2, 9])
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.Font.Bold = true;
                }

                // Add data rows
                int row = 3;
                foreach (var user in users)
                {
                    var timeEntriesForPeriod = _dbContext.TimeEntries
                        .Where(te => te.AppUserId == user.Id && te.Date >= startDate && te.Date <= endDate)
                        .Include(te => te.Job)
                        .OrderBy(te => te.Date)
                        .ToList();

                    var leaveEntriesForPeriod = _dbContext.LeaveEntries
                        .Where(le => le.AppUserId == user.Id && le.Date >= startDate && le.Date <= endDate)
                        .OrderBy(le => le.Date)
                        .ToList();

                    var allEntriesForPeriod = timeEntriesForPeriod.Cast<dynamic>()
                        .Concat(leaveEntriesForPeriod)
                        .OrderBy(entry => entry.Date)
                        .ToList();

                    DateTime? lastDate = null;
                    var dayTotal = 0.0;
                    var isFirstEntry = true;

                    foreach (var entry in allEntriesForPeriod)
                    {
                        if (lastDate.HasValue && entry.Date != lastDate && row > 3)
                        {
                            worksheet.Cells[row - 1, 8].Value = dayTotal;
                            dayTotal = 0.0;
                        }

                        if (entry is TimeEntry timeEntry)
                        {
                            if (isFirstEntry)
                            {
                                worksheet.Cells[row, 1].Value = user.FirstName + " " + user.LastName + " - " + user.EmployeeNumber;
                                isFirstEntry = false;
                            }
                            else
                            {
                                worksheet.Cells[row, 1].Value = "";
                            }
                            worksheet.Cells[row, 2].Value = timeEntry.Date.HasValue ? timeEntry.Date.Value.ToString("MM/dd/yy") : string.Empty;
                            worksheet.Cells[row, 3].Value = timeEntry.Job?.JobNumber;
                            worksheet.Cells[row, 4].Value = timeEntry.Duration;
                            worksheet.Cells[row, 5].Value = timeEntry.Duty;
                            worksheet.Cells[row, 6].Value = timeEntry.Comment;
                            worksheet.Cells[row, 7].Value = "TBA";
                            dayTotal += timeEntry.Duration ?? 0.0;
                            lastDate = timeEntry.Date;
                        }
                        else if (entry is LeaveEntry leaveEntry)
                        {
                            if (isFirstEntry)
                            {
                                worksheet.Cells[row, 1].Value = user.FirstName + " " + user.LastName + " - " + user.EmployeeNumber;
                                isFirstEntry = false;
                            }
                            else
                            {
                                worksheet.Cells[row, 1].Value = "";
                            }
                            worksheet.Cells[row, 2].Value = leaveEntry.Date.HasValue ? leaveEntry.Date.Value.ToString("MM/dd/yy") : string.Empty;
                            worksheet.Cells[row, 3].Value = leaveEntry.LeaveType;
                            worksheet.Cells[row, 4].Value = leaveEntry.LeaveDuration;
                            worksheet.Cells[row, 5].Value = leaveEntry.LeaveType;
                            worksheet.Cells[row, 6].Value = leaveEntry.Status;
                            worksheet.Cells[row, 7].Value = "N/A";
                            dayTotal += leaveEntry.LeaveDuration ?? 0.0;
                            lastDate = leaveEntry.Date;
                        }
                        row++;
                    }

                    if (lastDate.HasValue && row > 3)
                    {
                        worksheet.Cells[row - 1, 8].Value = dayTotal;
                    }

                    var totalHours = calculateTotalHours(timeEntriesForPeriod, leaveEntriesForPeriod);

                    worksheet.Cells[row, 1].Value = "Summary for " + user.FirstName + " " + user.LastName;
                    worksheet.Cells[row, 1, row, 2].Merge = true;
                    worksheet.Cells[row, 1, row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1, row, 4].Style.Fill.BackgroundColor.SetColor(Color.LightGoldenrodYellow);
                    worksheet.Cells[row, 4].Value = totalHours;
                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);

                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TimeEntries.xlsx");
            }
        }

        // Action method to export job details to an Excel file
        public async Task<IActionResult> ExportJobDetailsToExcel(string searchTerm)
        {
            var timeEntries = await _dbContext.TimeEntries
                .Include(te => te.AppUser)
                .Include(te => te.Job)
                .Where(te => te.Job.JobNumber == searchTerm)
                .Select(te => new
                {
                    te.AppUser.FirstName,
                    te.AppUser.LastName,
                    te.Date,
                    te.Duty,
                    te.Duration,
                    te.Job.JobNumber,
                    te.Job.JobName
                }).ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("JobDetails");

                worksheet.Cells[1, 1].Value = "Job Details";
                worksheet.Cells[1, 1, 1, 7].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 20;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Column(1).Width = 15;
                worksheet.Column(2).Width = 15;
                worksheet.Column(3).Width = 20;
                worksheet.Column(4).Width = 50;
                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Width = 45;
                worksheet.Column(7).Width = 10;

                worksheet.Cells[2, 1].Value = "First Name";
                worksheet.Cells[2, 2].Value = "Last Name";
                worksheet.Cells[2, 3].Value = "Job Number";
                worksheet.Cells[2, 4].Value = "Job Name";
                worksheet.Cells[2, 5].Value = "Date";
                worksheet.Cells[2, 6].Value = "Task";
                worksheet.Cells[2, 7].Value = "Duration";

                using (var range = worksheet.Cells[2, 1, 2, 7])
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.Font.Bold = true;
                }

                var groupedEntries = timeEntries.GroupBy(te => new { te.FirstName, te.LastName });

                int row = 3;
                double grandTotalDuration = 0;

                foreach (var group in groupedEntries)
                {
                    double userTotalDuration = 0;

                    foreach (var entry in group)
                    {
                        worksheet.Cells[row, 1].Value = entry.FirstName;
                        worksheet.Cells[row, 2].Value = entry.LastName;
                        worksheet.Cells[row, 3].Value = entry.JobNumber;
                        worksheet.Cells[row, 4].Value = entry.JobName;
                        worksheet.Cells[row, 5].Value = entry.Date?.ToString("MM/dd/yyyy");
                        worksheet.Cells[row, 6].Value = entry.Duty;
                        worksheet.Cells[row, 7].Value = entry.Duration;
                        userTotalDuration += (double)entry.Duration;
                        row++;
                    }

                    worksheet.Cells[row, 6].Value = "Total";
                    worksheet.Cells[row, 7].Value = userTotalDuration;
                    worksheet.Cells[row, 6, row, 7].Style.Font.Bold = true;
                    row++;

                    grandTotalDuration += userTotalDuration;
                }

                worksheet.Cells[row, 6].Value = "Grand Total";
                worksheet.Cells[row, 7].Value = grandTotalDuration;
                worksheet.Cells[row, 6, row, 7].Style.Font.Bold = true;

                var stream = new MemoryStream();
                package.SaveAs(stream);

                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "JobDetails.xlsx");
            }
        }

        // Helper method to calculate total hours from time and leave entries
        private double calculateTotalHours(IEnumerable<TimeEntry> timeEntries, IEnumerable<LeaveEntry> leaveEntries)
        {
            var timeEntriesTotal = timeEntries.Sum(te => te.Duration.GetValueOrDefault());
            var leaveEntriesTotal = leaveEntries.Where(le => le.LeaveType != "UPTO").Sum(le => le.LeaveDuration.GetValueOrDefault());
            return timeEntriesTotal + leaveEntriesTotal;
        }

        // Action method to display job details
        public async Task<IActionResult> JobDetails(string searchTerm)
        {
            var timeEntries = _dbContext.TimeEntries
                .Include(te => te.AppUser)
                .Include(te => te.Job)
                .Where(te => te.Job.JobNumber == searchTerm)
                .Select(te => new
                {
                    te.AppUser.FirstName,
                    te.AppUser.LastName,
                    te.Date,
                    te.Duty,
                    te.Duration,
                    te.Job.JobNumber,
                    te.Job.JobName
                });

            return View(await timeEntries.ToListAsync());
        }
    }
}