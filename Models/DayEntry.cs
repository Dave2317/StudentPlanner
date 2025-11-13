using System;
using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Models
{
    public class DayEntry
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(100)]
        public string Course { get; set; }

        [Display(Name = "Task Description")]
        [StringLength(500)]
        public string TaskDescription { get; set; }

        [Range(0, 24)]
        public double Hours { get; set; }

        [StringLength(50)]
        public string Status { get; set; }   // e.g. Planned / In Progress / Done

        [StringLength(500)]
        public string Notes { get; set; }
    }
}
