using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using AssignmentTracker.ViewModels;
using AssignmentTracker.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;

namespace AssignmentTracker
{
    public partial class DashboardUserControl : UserControl
    {
        private readonly AssignmentViewModel viewModel;
        private DateTime selectedWeekStart;
        private ToolTip taskToolTip;
        private Dictionary<string, Brush> unitCodeColors;

        public DashboardUserControl()
        {
            InitializeComponent();
        }

        public DashboardUserControl(AssignmentViewModel viewModel, Dictionary<string, Brush> unitCodeColors) : this()
        {
            this.viewModel = viewModel;
            this.unitCodeColors = unitCodeColors;
            DataContext = this.viewModel;
            InitializeWeekComboBox();
            selectedWeekStart = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            LoadData();
            this.SizeChanged += DashboardUserControl_SizeChanged;

            viewModel.PropertyChanged += ViewModel_PropertyChanged; // Add this line
        }
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(viewModel.FilteredAssignments))
            {
                RefreshData();
            }
        }

        private void DashboardUserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawGanttChart();
        }

        public void RefreshData()
        {
            LoadData();
        }

        private void InitializeWeekComboBox()
        {
            var startDate = DateTime.Now.StartOfWeek(DayOfWeek.Monday).AddMonths(-3);
            var endDate = DateTime.Now.StartOfWeek(DayOfWeek.Monday).AddMonths(3);

            var weeks = new List<DateTime>();
            while (startDate <= endDate)
            {
                weeks.Add(startDate);
                startDate = startDate.AddDays(7);
            }

            WeekComboBox.ItemsSource = weeks;
            WeekComboBox.SelectedItem = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
        }

        private void LoadData()
        {
            if (viewModel == null || viewModel.FilteredAssignments == null)
            {
                Console.WriteLine("viewModel or FilteredAssignments is null");
                return;
            }

            var startOfWeek = selectedWeekStart;
            var endOfWeek = startOfWeek.AddDays(7);

            WeekDateRangeTextBlock.Text = $"{startOfWeek:dd/MM/yyyy} - {endOfWeek:dd/MM/yyyy}";

            var assignmentsDueThisWeek = viewModel.FilteredAssignments
                .Cast<Assignment>()
                .Where(a => a.DueDate >= startOfWeek && a.DueDate < endOfWeek)
                .GroupBy(a => a.TaskGrade)
                .Select(g => new GradeCount { Grade = g.Key, Count = g.Count() })
                .OrderBy(g => g.Grade)
                .ToList();

            if (!assignmentsDueThisWeek.Any())
            {
                assignmentsDueThisWeek.Add(new GradeCount { Grade = "None", Count = 0 });
            }

            GradeCountListView.ItemsSource = assignmentsDueThisWeek;
            DrawGanttChart();
            UpdateUnitCodeLegend();
        }

        private void GanttScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.HorizontalChange != 0)
            {
                DateLabelsScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        }

        private void DrawGanttChart()
        {
            GanttCanvas.Children.Clear();
            DateLabelsCanvas.Children.Clear();

            if (!viewModel.FilteredAssignments.Cast<Assignment>().Any())
                return;

            var sortedAssignments = viewModel.FilteredAssignments.Cast<Assignment>().OrderBy(a => a.StartDate).ToList();

            DateTime minDate = sortedAssignments.Min(a => a.StartDate);
            DateTime maxDate = sortedAssignments.Max(a => a.DueDate);

            double totalDays = (maxDate - minDate).TotalDays + 1;
            double canvasWidth = Math.Max(GanttCanvas.ActualWidth, totalDays * 10);
            double dayWidth = canvasWidth / totalDays;
            double taskHeight = 10;
            double taskSpacing = 5;

            GanttCanvas.Width = canvasWidth;
            GanttCanvas.Height = sortedAssignments.Count * (taskHeight + taskSpacing);

            DateLabelsCanvas.Width = canvasWidth;

            DateTime startWeek = minDate.StartOfWeek(DayOfWeek.Monday);
            while (startWeek <= maxDate)
            {
                double separatorLeft = (startWeek - minDate).TotalDays * dayWidth;
                var line = new Line
                {
                    X1 = separatorLeft,
                    Y1 = 0,
                    X2 = separatorLeft,
                    Y2 = GanttCanvas.Height,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 1
                };

                GanttCanvas.Children.Add(line);

                var weekText = new TextBlock
                {
                    Text = startWeek.ToString("dd/MM/yyyy"),
                    Foreground = Brushes.Gray
                };

                Canvas.SetLeft(weekText, separatorLeft + 5);
                Canvas.SetTop(weekText, 0); // Position at the top of DateLabelsCanvas
                Canvas.SetZIndex(weekText, 1); // Ensure the text is on top
                DateLabelsCanvas.Children.Add(weekText);

                startWeek = startWeek.AddDays(7);
            }

            DateTime weekStart = selectedWeekStart;
            double weekLeft = (weekStart - minDate).TotalDays * dayWidth;
            double weekWidth = 7 * dayWidth;

            var weekRect = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)),
                Stroke = Brushes.Red,
                Width = weekWidth,
                Height = GanttCanvas.Height
            };

            Canvas.SetLeft(weekRect, weekLeft);
            Canvas.SetTop(weekRect, 0);
            GanttCanvas.Children.Insert(0, weekRect);

            int taskIndex = 0;
            foreach (var assignment in sortedAssignments)
            {
                double left = (assignment.StartDate - minDate).TotalDays * dayWidth;
                double width = (assignment.DueDate - assignment.StartDate).TotalDays * dayWidth;
                double top = taskIndex * (taskHeight + taskSpacing);

                var rect = new Rectangle
                {
                    Fill = unitCodeColors.ContainsKey(assignment.UnitCode) ? unitCodeColors[assignment.UnitCode] : Brushes.LightGray,
                    Stroke = Brushes.Black,
                    Width = width,
                    Height = taskHeight
                };

                rect.MouseEnter += (s, e) => ShowToolTip(s, e, assignment);
                rect.MouseLeave += (s, e) => HideToolTip();

                Canvas.SetLeft(rect, left);
                Canvas.SetTop(rect, top);
                GanttCanvas.Children.Add(rect);

                taskIndex++;
            }
        }

        private void UpdateUnitCodeLegend()
        {
            UnitCodeLegend.Children.Clear();
            UnitCodeLegend.Children.Add(new TextBlock { Text = "Unit Code Key", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 5) });
            foreach (var entry in unitCodeColors)
            {
                var legendItem = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 5) };
                var colorRect = new Rectangle { Width = 20, Height = 20, Fill = entry.Value, Margin = new Thickness(0, 0, 5, 0) };
                var label = new TextBlock { Text = entry.Key };
                legendItem.Children.Add(colorRect);
                legendItem.Children.Add(label);
                UnitCodeLegend.Children.Add(legendItem);
            }
        }

        private void ShowToolTip(object sender, System.Windows.Input.MouseEventArgs e, Assignment assignment)
        {
            if (taskToolTip == null)
            {
                taskToolTip = new ToolTip();
            }

            taskToolTip.Content = $"{assignment.TaskName}\nStart: {assignment.StartDate:dd/MM/yyyy}\nDue: {assignment.DueDate:dd/MM/yyyy}";
            var rect = sender as Rectangle;
            if (rect != null)
            {
                rect.ToolTip = taskToolTip;
                taskToolTip.IsOpen = true;
            }
        }

        private void HideToolTip()
        {
            if (taskToolTip != null)
            {
                taskToolTip.IsOpen = false;
            }
        }

        private void WeekComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WeekComboBox.SelectedItem != null)
            {
                selectedWeekStart = (DateTime)WeekComboBox.SelectedItem;
                RefreshData(); // Ensure the selected week is highlighted dynamically
            }
        }

        private void ViewWeekButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void PreviousWeekButton_Click(object sender, RoutedEventArgs e)
        {
            selectedWeekStart = selectedWeekStart.AddDays(-7);
            RefreshData();
        }

        private void NextWeekButton_Click(object sender, RoutedEventArgs e)
        {
            selectedWeekStart = selectedWeekStart.AddDays(7);
            RefreshData();
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
