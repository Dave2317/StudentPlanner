using System.Collections.Generic;

namespace StudentPlanner.Models
{
    public class ReportsViewModel
    {
        public List<CourseHoursSummary> CourseSummaries { get; set; } = new();
        public int TotalHoursAllTime { get; set; }
        public int EntriesLast7Days { get; set; }


        public List<string> StudyTips { get; set; } = new();
    }
}
