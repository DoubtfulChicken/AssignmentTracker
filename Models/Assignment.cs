using System;
using System.Windows.Media;
using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Reflection;

namespace AssignmentTracker.Models
{
    public class Assignment : ICloneable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string UnitCode { get; set; }
        public string TaskName { get; set; }
        public string TaskGrade { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Notes { get; set; }
        public DateTime? ReminderDate { get; set; }  // Optional manual reminder date

        public enum AssignmentStatus
        {
            [Description("Not Started")]
            NotStarted,
            [Description("In Progress")]
            InProgress,
            [Description("Completed")]
            Completed
        }

        public static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();

            return attribute == null ? value.ToString() : attribute.Description;
        }

        private AssignmentStatus status = AssignmentStatus.NotStarted;
        public AssignmentStatus Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public object Clone()
        {
            return new Assignment
            {
                UnitCode = this.UnitCode,
                TaskName = this.TaskName,
                TaskGrade = this.TaskGrade,
                StartDate = this.StartDate,
                DueDate = this.DueDate,
                Notes = this.Notes,
                ReminderDate = this.ReminderDate,
                UnitCodeColor = this.UnitCodeColor,
                TaskGradeColor = this.TaskGradeColor,
                Status = this.Status
            };
        }

        // Automatic reminder dates
        public DateTime ReminderOnStart => StartDate;
        public DateTime Reminder5DaysBeforeDue => DueDate.AddDays(-5);
        public DateTime Reminder10DaysBeforeDue => DueDate.AddDays(-10);

        // Calculated properties for binding
        [JsonIgnore]
        public int DaysUntilStart => (StartDate - DateTime.Now).Days;
        [JsonIgnore]
        public int DaysUntilDue => (DueDate - DateTime.Now).Days;

        // Properties for cell colors
        [JsonIgnore]
        public Brush UnitCodeColor { get; set; }
        [JsonIgnore]
        public Brush TaskGradeColor { get; set; }

        public AssignmentDTO ToDTO()
        {
            return new AssignmentDTO
            {
                UnitCode = this.UnitCode,
                TaskName = this.TaskName,
                TaskGrade = this.TaskGrade,
                StartDate = this.StartDate,
                DueDate = this.DueDate,
                Notes = this.Notes,
                ReminderDate = this.ReminderDate,
                UnitCodeColor = ((SolidColorBrush)UnitCodeColor).Color.ToUint(),
                TaskGradeColor = ((SolidColorBrush)TaskGradeColor).Color.ToUint()
            };
        }

        public static Assignment FromDTO(AssignmentDTO dto)
        {
            return new Assignment
            {
                UnitCode = dto.UnitCode,
                TaskName = dto.TaskName,
                TaskGrade = dto.TaskGrade,
                StartDate = dto.StartDate,
                DueDate = dto.DueDate,
                Notes = dto.Notes,
                ReminderDate = dto.ReminderDate,
                UnitCodeColor = new SolidColorBrush(dto.UnitCodeColor.ToColor()),
                TaskGradeColor = new SolidColorBrush(dto.TaskGradeColor.ToColor())
            };
        }
    }

    public static class ColorExtensions
    {
        public static uint ToUint(this Color color)
        {
            return ((uint)color.A << 24) | ((uint)color.R << 16) | ((uint)color.G << 8) | color.B;
        }

        public static Color ToColor(this uint argb)
        {
            return Color.FromArgb(
                (byte)((argb >> 24) & 0xFF),
                (byte)((argb >> 16) & 0xFF),
                (byte)((argb >> 8) & 0xFF),
                (byte)(argb & 0xFF));
        }
    }
}
