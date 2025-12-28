using System.Drawing;
using System.Globalization;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Interfaces;
using Timeclock_WebApplication.Models;
using Timeclock_WebApplication.Repository;
using Timeclock_WebApplication.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Timeclock_WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IUserRepository _userRepository;

        // Constructor to initialize the database context and user repository
        public AdminController(ApplicationDbContext dbContext, IUserRepository userRepository)
        {
            _dbContext = dbContext;
            _userRepository = userRepository;
        }

        // GET: api/Admin
        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userRepository.GetAllUsers();
            return Ok(users);
        }

        // GET: api/Admin/ExportToExcel
        [HttpGet("ExportToExcel")]
        public async Task<IActionResult> ExportToExcel(string group, DateTime startDate, DateTime endDate)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var users = await _userRepository.GetUsersByGroup(group);

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
                    var taskEntriesForPeriod = _dbContext.TaskEntries
                        .Where(te => te.AppUserId == user.Id && te.Date >= startDate && te.Date <= endDate)
                        .Include(te => te.Job)
                        .OrderBy(te => te.Date)
                        .ToList();

                    var leaveEntriesForPeriod = _dbContext.LeaveEntries
                        .Where(le => le.AppUserId == user.Id && le.Date >= startDate && le.Date <= endDate)
                        .OrderBy(le => le.Date)
                        .ToList();

                    var allEntriesForPeriod = taskEntriesForPeriod.Cast<dynamic>()
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

                        if (entry is TaskEntry taskEntry)
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
                            worksheet.Cells[row, 2].Value = taskEntry.Date.HasValue ? taskEntry.Date.Value.ToString("MM/dd/yy") : string.Empty;
                            worksheet.Cells[row, 3].Value = taskEntry.Job?.JobNumber;
                            worksheet.Cells[row, 4].Value = taskEntry.Duration;
                            worksheet.Cells[row, 5].Value = taskEntry.TaskName;
                            worksheet.Cells[row, 6].Value = taskEntry.Comment;
                            worksheet.Cells[row, 7].Value = "TBA";
                            dayTotal += taskEntry.Duration ?? 0.0;
                            lastDate = taskEntry.Date;
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

                    var totalHours = CalculateTotalHours(taskEntriesForPeriod, leaveEntriesForPeriod);

                    worksheet.Cells[row, 1].Value = "Summary for " + user.FirstName + " " + user.LastName;
                    worksheet.Cells[row, 1, row, 2].Merge = true;
                    worksheet.Cells[row, 1, row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1, row, 4].Style.Fill.BackgroundColor.SetColor(Color.LightGoldenrodYellow);
                    worksheet.Cells[row, 4].Value = totalHours;
                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);

                stream.Position = 0;
                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TaskEntries.xlsx");
            }
        }

        // GET: api/Admin/ExportJobDetailsToExcel
        [HttpGet("ExportJobDetailsToExcel")]
        public async Task<IActionResult> ExportJobDetailsToExcel(string searchTerm)
        {
            var taskEntries = await _dbContext.TaskEntries
                .Include(te => te.AppUser)
                .Include(te => te.Job)
                .Where(te => te.Job.JobNumber == searchTerm)
                .Select(te => new
                {
                    te.AppUser.FirstName,
                    te.AppUser.LastName,
                    te.Date,
                    te.TaskName,
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

                var groupedEntries = taskEntries.GroupBy(te => new { te.FirstName, te.LastName });

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
                        worksheet.Cells[row, 6].Value = entry.TaskName;
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

                stream.Position = 0;
                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "JobDetails.xlsx");
            }
        }

        // Helper method to calculate total hours from time and leave entries
        private double CalculateTotalHours(IEnumerable<TaskEntry> taskEntries, IEnumerable<LeaveEntry> leaveEntries)
        {
            var taskEntriesTotal = taskEntries.Sum(te => te.Duration.GetValueOrDefault());
            var leaveEntriesTotal = leaveEntries.Where(le => le.LeaveType != "UPTO").Sum(le => le.LeaveDuration.GetValueOrDefault());
            return taskEntriesTotal + leaveEntriesTotal;
        }

        // GET: api/Admin/JobDetails
        [HttpGet("JobDetails")]
        public async Task<IActionResult> JobDetails(string searchTerm)
        {
             var taskEntries = await _dbContext.TaskEntries
                .Include(te => te.AppUser)
                .Include(te => te.Job)
                .Where(te => te.Job.JobNumber == searchTerm)
                .Select(te => new
                {
                    te.AppUser.FirstName,
                    te.AppUser.LastName,
                    te.Date,
                    te.TaskName,
                    te.Duration,
                    te.Job.JobNumber,
                    te.Job.JobName
                }).ToListAsync();

            return Ok(taskEntries);
        }
    }
}
