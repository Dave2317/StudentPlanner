using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using StudentPlanner.Models;


namespace StudentPlanner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Dashboard
        public IActionResult Index()
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-6); // last 7 days including today

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

            var model = new DashboardViewModel
            {
                TasksThisWeek = tasksThisWeek,
                HoursLast7Days = hoursLast7Days,
                TodayTasks = todayTasks,
                CompletionRate = completionRate
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

            var model = new ReportsViewModel
            {
                CourseSummaries = courseSummaries,
                TotalHoursAllTime = totalHoursAllTime,
                EntriesLast7Days = entriesLast7Days
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
        // Export all DayEntries as CSV
    public IActionResult ExportData()
    {
        var entries = _context.DayEntries.ToList();

        var sb = new StringBuilder();
        sb.AppendLine("Date,Course,TaskDescription,Hours,Status,Notes");

        foreach (var e in entries)
        {
            // Replace commas to avoid breaking CSV columns
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


        // POST: Save settings (name cookie)
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

            return RedirectToAction("Settings");
        }

        // POST: Clear name cookie
        [HttpPost]
        public IActionResult ClearSettings()
        {
            Response.Cookies.Delete("UserName");
            TempData["SettingsMessage"] = "Saved name has been cleared.";
            return RedirectToAction("Settings");
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
