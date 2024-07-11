using System.Windows;
using AssignmentTracker.Models;
using System.Collections.Generic;
using System.Windows.Controls;

namespace AssignmentTracker.Views
{
    public partial class AddAssignmentWindow : Window
    {
        public Assignment Assignment { get; set; }
        public List<string> UnitCodes { get; set; }

        public AddAssignmentWindow(List<string> unitCodes)
        {
            InitializeComponent();
            UnitCodes = unitCodes;
            UnitCodeComboBox.ItemsSource = UnitCodes;
        }

        public void InitializeFields()
        {
            if (Assignment != null)
            {
                UnitCodeComboBox.Text = Assignment.UnitCode;
                TaskNameTextBox.Text = Assignment.TaskName;
                TaskGradeComboBox.SelectedItem = Assignment.TaskGrade;
                StartDatePicker.SelectedDate = Assignment.StartDate;
                DueDatePicker.SelectedDate = Assignment.DueDate;
                NotesTextBox.Text = Assignment.Notes;
                ReminderDatePicker.SelectedDate = Assignment.ReminderDate;  // Initialize reminder date
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UnitCodeComboBox.Text) || string.IsNullOrEmpty(TaskNameTextBox.Text) || !StartDatePicker.SelectedDate.HasValue || !DueDatePicker.SelectedDate.HasValue)
            {
                System.Windows.MessageBox.Show("Please fill in the required fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Assignment = new Assignment
            {
                UnitCode = UnitCodeComboBox.Text,
                TaskName = TaskNameTextBox.Text,
                TaskGrade = (TaskGradeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                StartDate = StartDatePicker.SelectedDate.Value,
                DueDate = DueDatePicker.SelectedDate.Value,
                Notes = NotesTextBox.Text,
                ReminderDate = ReminderDatePicker.SelectedDate  // Set reminder date
            };

            if (!UnitCodes.Contains(UnitCodeComboBox.Text))
            {
                UnitCodes.Add(UnitCodeComboBox.Text);
            }

            DialogResult = true;
            Close();
        }
    }
}
