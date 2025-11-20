namespace StudentPlanner.Models
{
    public class DashboardViewModel
    {
        public int TasksThisWeek { get; set; }
        public int HoursLast7Days { get; set; }
        public int TodayTasks { get; set; }
        public double CompletionRate { get; set; }

       
        public int HoursThisWeek => HoursLast7Days;
        public int TodaysTasks => TodayTasks;

       
        public List<DayEntry>? TodayEntries { get; set; }
    }
}
