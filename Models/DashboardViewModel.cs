namespace StudentPlanner.Models
{
    public class DashboardViewModel
    {
        public int TasksThisWeek { get; set; }
        public double HoursLast7Days { get; set; }
        public int TodayTasks { get; set; }
        public double CompletionRate { get; set; }
    }
}
