using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using AssignmentTracker.Models;

namespace AssignmentTracker
{
    public class NotificationManager
    {
        private readonly DispatcherTimer timer;
        private readonly List<Assignment> assignments;

        public NotificationManager(List<Assignment> assignments)
        {
            this.assignments = assignments;
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)  // Check every minute
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CheckReminders();
        }

        private void CheckReminders()
        {
            foreach (var assignment in assignments)
            {
                if (assignment.ReminderDate.HasValue && assignment.ReminderDate.Value <= DateTime.Now)
                {
                    ShowNotification(assignment, "Reminder for your task!");
                    assignment.ReminderDate = null;  // Clear manual reminder after showing notification
                }

                if (DateTime.Now >= assignment.ReminderOnStart && DateTime.Now < assignment.ReminderOnStart.AddMinutes(1))
                {
                    ShowNotification(assignment, "Start your task!");
                }

                if (DateTime.Now >= assignment.Reminder5DaysBeforeDue && DateTime.Now < assignment.Reminder5DaysBeforeDue.AddMinutes(1))
                {
                    ShowNotification(assignment, "5 days until the due date!");
                }

                if (DateTime.Now >= assignment.Reminder10DaysBeforeDue && DateTime.Now < assignment.Reminder10DaysBeforeDue.AddMinutes(1))
                {
                    ShowNotification(assignment, "10 days until the due date!");
                }
            }
        }

        public void ShowNotification(Assignment assignment, string message)
        {
            MessageBox.Show($"{message}: {assignment.TaskName} is due on {assignment.DueDate:dd/MM/yyyy}!", "Assignment Reminder", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
