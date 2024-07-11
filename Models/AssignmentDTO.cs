using System;

namespace AssignmentTracker.Models
{
    public class AssignmentDTO
    {
        public string UnitCode { get; set; }
        public string TaskName { get; set; }
        public string TaskGrade { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Notes { get; set; }
        public DateTime? ReminderDate { get; set; }
        public uint UnitCodeColor { get; set; }
        public uint TaskGradeColor { get; set; }
    }
}