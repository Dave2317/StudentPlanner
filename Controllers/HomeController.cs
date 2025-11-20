using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StudentPlanner.Models;
using StudentPlanner.Data;
using StudentPlanner.Services;

namespace StudentPlanner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly SQLiteService _sqlite;
        private readonly EmailService _emailService;

        public HomeController(
            ILogger<HomeController> logger,
            AppDbContext context,
            SQLiteService sqlite,
            EmailService emailService)
        {
            _logger = logger;
            _context = context;
            _sqlite = sqlite;
            _emailService = emailService;
        }

        // Dashboard
        public IActionResult Index()
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-6);

            var tasksThisWeek = _context.DayEntries
                .Count(e => e.Date >= weekStart && e.Date <= today);

            var hoursLast7Days = (int)Math.Round(
                _context.DayEntries
                    .Where(e => e.Date >= weekStart && e.Date <= today)
                    .Sum(e => e.Hours)
            );

            var todayTasks = _context.DayEntries
                .Count(e => e.Date == today);

            var completedThisWeek = _context.DayEntries
                .Count(e => e.Date >= weekStart && e.Date <= today && e.Status == "Completed");

            double completionRate = 0;
            if (tasksThisWeek > 0)
            {
                completionRate = (double)completedThisWeek / tasksThisWeek * 100.0;
            }

            var todaysEntriesList = _context.DayEntries
                .Where(e => e.Date == today)
                .ToList();

            var model = new DashboardViewModel
            {
                TasksThisWeek = tasksThisWeek,
                HoursLast7Days = hoursLast7Days,
                TodayTasks = todayTasks,
                CompletionRate = completionRate,
                TodayEntries = todaysEntriesList
            };

            return View(model);
        }

        // Reports page
        public IActionResult Reports()
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-6);

            var courseSummaries = _context.DayEntries
                .GroupBy(e => e.Course)
                .Select(g => new CourseHoursSummary
                {
                    Course = g.Key,
                    TotalHours = (int)Math.Round(g.Sum(x => x.Hours)),
                    EntryCount = g.Count()
                })
                .OrderByDescending(x => x.TotalHours)
                .ToList();

            var totalHoursAllTime = _context.DayEntries.Any()
                ? (int)Math.Round(_context.DayEntries.Sum(e => e.Hours))
                : 0;

            var entriesLast7Days = _context.DayEntries
                .Count(e => e.Date >= weekStart && e.Date <= today);

            var tips = _sqlite.GetStudyTips();

            var model = new ReportsViewModel
            {
                CourseSummaries = courseSummaries,
                TotalHoursAllTime = totalHoursAllTime,
                EntriesLast7Days = entriesLast7Days,
                StudyTips = tips
            };

            return View(model);
        }

        // GET: Settings page
        [HttpGet]
        public IActionResult Settings()
        {
            var model = new SettingsViewModel();

            if (Request.Cookies.TryGetValue("UserName", out var name))
            {
                model.Name = name;
            }

            if (Request.Cookies.TryGetValue("theme", out var theme))
            {
                model.CurrentTheme = theme;
            }

            return View(model);
        }

        // POST: Save settings (name cookie) and keep theme label correct
        [HttpPost]
        public IActionResult SaveSettings(SettingsViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                var options = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    IsEssential = true
                };

                Response.Cookies.Append("UserName", model.Name, options);
                TempData["SettingsMessage"] = "Your name has been saved.";
            }
            else
            {
                TempData["SettingsMessage"] = "Name cannot be empty.";
            }

            // Re-read theme so the label shows the current value
            if (Request.Cookies.TryGetValue("theme", out var theme))
            {
                model.CurrentTheme = theme;
            }

            return View("Settings", model);
        }

        // Export all DayEntries as CSV
        public IActionResult ExportData()
        {
            var entries = _context.DayEntries.ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Date,Course,TaskDescription,Hours,Status,Notes");

            foreach (var e in entries)
            {
                var course = (e.Course ?? "").Replace(",", " ");
                var task = (e.TaskDescription ?? "").Replace(",", " ");
                var status = (e.Status ?? "").Replace(",", " ");
                var notes = (e.Notes ?? "").Replace(",", " ");

                sb.AppendLine($"{e.Date:yyyy-MM-dd},{course},{task},{e.Hours},{status},{notes}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"DailyEntries_{DateTime.Now:yyyyMMdd_HHmm}.csv";

            return File(bytes, "text/csv", fileName);
        }

        // POST: Clear name cookie
        [HttpPost]
        public IActionResult ClearSettings()
        {
            Response.Cookies.Delete("UserName");
            TempData["SettingsMessage"] = "Saved name has been cleared.";
            return RedirectToAction("Settings");
        }

        [HttpPost]
        public async Task<IActionResult> EmailTodaySummary()
        {
            var today = DateTime.Today;

            var todaysEntries = await _context.DayEntries
                .Where(x => x.Date == today)
                .ToListAsync();

            if (!todaysEntries.Any())
            {
                TempData["DashboardMessage"] = "No entries for today.";
                return RedirectToAction("Index");
            }

            string emailBody = "<h2>Today's Study Summary</h2>";

            foreach (var e in todaysEntries)
            {
                emailBody += $"<p><b>{e.Course}</b>: {e.TaskDescription} ({e.Hours} hours) - {e.Status}</p>";
            }

            await _emailService.SendEmailAsync(
                "harshildaveprojects@gmail.com",
                "Today's Study Summary",
                emailBody
            );

            TempData["DashboardMessage"] = "Today's summary has been emailed!";
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

