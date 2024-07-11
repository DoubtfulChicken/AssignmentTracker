using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AssignmentTracker.Models;
using AssignmentTracker.ViewModels;
using AssignmentTracker.Views;
using System.Windows.Forms;
using DrawingColor = System.Drawing.Color; // Alias to differentiate System.Drawing.Color
using MediaBrush = System.Windows.Media.Brush; // Alias to differentiate System.Windows.Media.Brush
using MediaColor = System.Windows.Media.Color; // Alias to differentiate System.Windows.Media.Color
using System.Linq;
using System.IO;
using OfficeOpenXml;
using System.Text.Json;
using System.Collections;
using static AssignmentTracker.MainWindow;

namespace AssignmentTracker
{
    public partial class MainWindow : Window
    {
        private readonly AssignmentViewModel viewModel;
        private readonly NotificationManager notificationManager;
        private List<string> unitCodes = new List<string>();
        private Dictionary<string, MediaBrush> unitCodeColors = new Dictionary<string, MediaBrush>();
        private Dictionary<string, MediaBrush> gradeColors = new Dictionary<string, MediaBrush>
        {
            { "Pass", Brushes.LightSeaGreen },
            { "Credit", Brushes.DodgerBlue },
            { "Distinction", Brushes.MediumOrchid },
            { "High Distinction", Brushes.Salmon }
        };

        private const string SaveFilePath = "assignments.json";

        private GridViewColumnHeader _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;
        private System.Windows.Controls.TabControl MainTabControl;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new AssignmentViewModel();
            this.DataContext = viewModel;

            var dashboard = new DashboardUserControl(viewModel, unitCodeColors);
            DashboardUserControl.Content = dashboard;

            notificationManager = new NotificationManager(viewModel.Assignments.ToList());

            LoadAssignments();
            CheckForUpcomingAssignments();

            AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(GridViewColumnHeader_Click));

            MainTabControl = this.FindName("MainTabControl") as System.Windows.Controls.TabControl;
        }

        private void CheckForUpcomingAssignments()
        {
            var upcomingAssignments = viewModel.Assignments
                .Where(a => a.DueDate <= DateTime.Now.AddDays(10) && a.DueDate >= DateTime.Now)
                .OrderBy(a => a.DueDate)
                .ToList();

            foreach (var assignment in upcomingAssignments)
            {
                if (assignment.DaysUntilDue <= 5)
                {
                    notificationManager.ShowNotification(assignment, "Due in less than 5 days");
                }
                else if (assignment.DaysUntilDue <= 10)
                {
                    notificationManager.ShowNotification(assignment, "Due in less than 10 days");
                }
            }
        }

        private void LoadAssignments()
        {
            if (File.Exists(SaveFilePath))
            {
                var jsonData = File.ReadAllText(SaveFilePath);
                var assignmentDTOs = JsonSerializer.Deserialize<List<AssignmentDTO>>(jsonData);

                if (assignmentDTOs != null)
                {
                    foreach (var dto in assignmentDTOs)
                    {
                        var assignment = Assignment.FromDTO(dto);
                        viewModel.AddAssignment(assignment);

                        if (!unitCodes.Contains(assignment.UnitCode))
                        {
                            unitCodes.Add(assignment.UnitCode);
                        }

                        if (!unitCodeColors.ContainsKey(assignment.UnitCode) && assignment.UnitCodeColor is SolidColorBrush brush)
                        {
                            unitCodeColors[assignment.UnitCode] = brush;
                        }
                    }
                }
            }

            ApplyCustomColors();
            RefreshDashboardData();
        }

        private void SaveAssignments()
        {
            var assignmentDTOs = viewModel.Assignments.Select(a => a.ToDTO()).ToList();
            var jsonData = JsonSerializer.Serialize(assignmentDTOs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SaveFilePath, jsonData);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveAssignments();
            base.OnClosing(e);
        }


        private void AddAssignmentButton_Click(object sender, RoutedEventArgs e)
        {
            AddAssignmentWindow addAssignmentWindow = new AddAssignmentWindow(unitCodes);
            if (addAssignmentWindow.ShowDialog() == true)
            {
                Assignment newAssignment = addAssignmentWindow.Assignment;
                viewModel.AddAssignment(newAssignment);

                if (!unitCodes.Contains(newAssignment.UnitCode))
                {
                    unitCodes.Add(newAssignment.UnitCode);
                    AssignColorToUnitCode(newAssignment.UnitCode);
                }

                ApplyCustomColors();
                RefreshDashboardData();
            }
        }

        private void AssignColorToUnitCode(string unitCode)
        {
            // Assign a default color if not already assigned
            if (!unitCodeColors.ContainsKey(unitCode))
            {
                unitCodeColors[unitCode] = Brushes.LightBlue;
            }
        }

        private void ApplyCustomColors()
        {
            foreach (var assignment in viewModel.Assignments)
            {
                if (assignment != null)
                {
                    if (unitCodeColors != null && unitCodeColors.ContainsKey(assignment.UnitCode))
                    {
                        assignment.UnitCodeColor = unitCodeColors[assignment.UnitCode];
                    }

                    if (gradeColors != null && gradeColors.ContainsKey(assignment.TaskGrade))
                    {
                        assignment.TaskGradeColor = gradeColors[assignment.TaskGrade];
                    }
                }
            }

            CollectionViewSource.GetDefaultView(AssignmentListView.ItemsSource)?.Refresh();
            RefreshDashboardData();
        }

        private void SetUnitCodeColour(string unitCode)
        {
            var colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = colorDialog.Color;
                unitCodeColors[unitCode] = new SolidColorBrush(MediaColor.FromArgb(color.A, color.R, color.G, color.B));
                ApplyCustomColors();
                RefreshDashboardData(); // Ensure data is refreshed
            }
        }

        private void SetTaskGradeColour(string taskGrade)
        {
            var colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = colorDialog.Color;
                gradeColors[taskGrade] = new SolidColorBrush(MediaColor.FromArgb(color.A, color.R, color.G, color.B));
                ApplyCustomColors();
                RefreshDashboardData(); // Ensure data is refreshed
            }
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        direction = _lastDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                    }

                    var sortBy = headerClicked.Column.Header as string;

                    switch (sortBy)
                    {
                        case "Unit Code":
                            SortUnitCode(direction);
                            break;
                        case "Task Grade":
                            SortTaskGrade(direction);
                            break;
                        case "Task Name":
                            SortTaskName(direction);
                            break;
                        case "Start Date":
                            SortDate(nameof(Assignment.StartDate), direction);
                            break;
                        case "Due Date":
                            SortDate(nameof(Assignment.DueDate), direction);
                            break;
                        case "Days Until Start":
                            SortNumeric(nameof(Assignment.DaysUntilStart), direction);
                            break;
                        case "Days Until Due":
                            SortNumeric(nameof(Assignment.DaysUntilDue), direction);
                            break;
                        case "Status":
                            SortStatus(direction);
                            break;
                        default:
                            Sort(sortBy, direction);
                            break;
                    }

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void SortUnitCode(ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(AssignmentListView.ItemsSource);

            dataView.SortDescriptions.Clear();
            var primarySortDescription = new SortDescription(nameof(Assignment.UnitCode), direction);
            var secondarySortDescription = new SortDescription(nameof(Assignment.DueDate), ListSortDirection.Ascending);

            dataView.SortDescriptions.Add(primarySortDescription);
            dataView.SortDescriptions.Add(secondarySortDescription);
            dataView.Refresh();
            RefreshDashboardData(); // Ensure data is refreshed
        }

        private void SortTaskGrade(ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(AssignmentListView.ItemsSource);

            dataView.SortDescriptions.Clear();

            if (dataView is ListCollectionView listCollectionView)
            {
                listCollectionView.CustomSort = new TaskGradeComparer(direction, gradeOrder);
            }

            dataView.Refresh();
            RefreshDashboardData(); // Ensure data is refreshed
        }

        public class NaturalSortComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                string sx = x?.ToString();
                string sy = y?.ToString();

                if (sx == null || sy == null)
                {
                    return Comparer.DefaultInvariant.Compare(x, y);
                }

                int i = 0, j = 0;
                while (i < sx.Length && j < sy.Length)
                {
                    if (char.IsDigit(sx[i]) && char.IsDigit(sy[j]))
                    {
                        var x1 = i;
                        var y1 = j;

                        while (i < sx.Length && char.IsDigit(sx[i])) i++;
                        while (j < sy.Length && char.IsDigit(sy[j])) j++;

                        var n1 = int.Parse(sx.Substring(x1, i - x1));
                        var n2 = int.Parse(sy.Substring(y1, j - y1));

                        int diff = n1 - n2;
                        if (diff != 0) return diff;
                    }
                    else
                    {
                        int diff = sx[i] - sy[j];
                        if (diff != 0) return diff;

                        i++;
                        j++;
                    }
                }

                return sx.Length - sy.Length;
            }
        }

        private void SortTaskName(ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(AssignmentListView.ItemsSource);
            dataView.SortDescriptions.Clear();

            IComparer comparer = new NaturalSortComparer();
            if (direction == ListSortDirection.Descending)
            {
                comparer = new ReverseComparer(comparer);
            }

            if (dataView is ListCollectionView listCollectionView)
            {
                listCollectionView.CustomSort = comparer;
            }

            dataView.Refresh();
            RefreshDashboardData(); // Ensure data is refreshed
        }

        public class ReverseComparer : IComparer
        {
            private readonly IComparer _originalComparer;

            public ReverseComparer(IComparer originalComparer)
            {
                _originalComparer = originalComparer;
            }

            public int Compare(object x, object y)
            {
                return _originalComparer.Compare(y, x);
            }
        }

        private void SortDate(string propertyName, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(AssignmentListView.ItemsSource);

            dataView.SortDescriptions.Clear();

            var sortDescription = new SortDescription(propertyName, direction);
            dataView.SortDescriptions.Add(sortDescription);

            dataView.Refresh();
            RefreshDashboardData(); // Ensure data is refreshed
        }

        private void SortNumeric(string propertyName, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(AssignmentListView.ItemsSource);

            dataView.SortDescriptions.Clear();

            var sortDescription = new SortDescription(propertyName, direction);
            dataView.SortDescriptions.Add(sortDescription);

            dataView.Refresh();
            RefreshDashboardData(); // Ensure data is refreshed
        }

        private void SortStatus(ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(AssignmentListView.ItemsSource);

            dataView.SortDescriptions.Clear();

            if (dataView is ListCollectionView listCollectionView)
            {
                listCollectionView.CustomSort = new StatusComparer(direction);
            }

            dataView.Refresh();
        }

        public class StatusComparer : IComparer
        {
            private readonly ListSortDirection _direction;

            public StatusComparer(ListSortDirection direction)
            {
                _direction = direction;
            }

            public int Compare(object x, object y)
            {
                if (x is Assignment a1 && y is Assignment a2)
                {
                    int result = a1.Status.CompareTo(a2.Status);
                    return _direction == ListSortDirection.Ascending ? result : -result;
                }
                return 0;
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(AssignmentListView.ItemsSource);

            dataView.SortDescriptions.Clear();

            switch (sortBy)
            {
                case "TaskName":
                    dataView.SortDescriptions.Add(new SortDescription(nameof(Assignment.TaskName), direction));
                    break;
                case "StartDate":
                    dataView.SortDescriptions.Add(new SortDescription(nameof(Assignment.StartDate), direction));
                    break;
                case "DueDate":
                    dataView.SortDescriptions.Add(new SortDescription(nameof(Assignment.DueDate), direction));
                    break;
                case "DaysUntilStart":
                    dataView.SortDescriptions.Add(new SortDescription(nameof(Assignment.DaysUntilStart), direction));
                    break;
                case "DaysUntilDue":
                    dataView.SortDescriptions.Add(new SortDescription(nameof(Assignment.DaysUntilDue), direction));
                    break;
                case "Unit Code":
                    dataView.SortDescriptions.Add(new SortDescription(nameof(Assignment.UnitCode), direction));
                    break;
                default:
                    if (dataView is ListCollectionView listCollectionView)
                    {
                        listCollectionView.CustomSort = new TaskGradeComparer(direction, gradeOrder);
                    }
                    break;
            }
        }

        private Dictionary<string, int> gradeOrder = new Dictionary<string, int>
        {
            { "Pass", 1 },
            { "Credit", 2 },
            { "Distinction", 3 },
            { "High Distinction", 4 }
        };

        private void AssignmentListView_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (AssignmentListView.SelectedItem is Assignment selectedAssignment)
            {
                var contextMenu = new System.Windows.Controls.ContextMenu();

                var copyMenuItem = new System.Windows.Controls.MenuItem { Header = "Copy" };
                copyMenuItem.Click += (s, ev) => CopyAssignment(selectedAssignment);

                var editMenuItem = new System.Windows.Controls.MenuItem { Header = "Edit" };
                editMenuItem.Click += (s, ev) => EditAssignment(selectedAssignment);

                var deleteMenuItem = new System.Windows.Controls.MenuItem { Header = "Delete" };
                deleteMenuItem.Click += (s, ev) => DeleteAssignment(selectedAssignment);

                var setUnitCodeColourMenuItem = new System.Windows.Controls.MenuItem { Header = "Set Unit Code Colour" };
                setUnitCodeColourMenuItem.Click += (s, ev) => SetUnitCodeColour(selectedAssignment.UnitCode);

                var setTaskGradeColourMenuItem = new System.Windows.Controls.MenuItem { Header = "Set Task Grade Colour" };
                setTaskGradeColourMenuItem.Click += (s, ev) => SetTaskGradeColour(selectedAssignment.TaskGrade);

                var statusSubMenu = new System.Windows.Controls.MenuItem { Header = "Change Status" };
                var notStartedMenuItem = new System.Windows.Controls.MenuItem { Header = "Not Started" };
                notStartedMenuItem.Click += (s, ev) => UpdateAssignmentStatus(selectedAssignment, Assignment.AssignmentStatus.NotStarted);
                var inProgressMenuItem = new System.Windows.Controls.MenuItem { Header = "In Progress" };
                inProgressMenuItem.Click += (s, ev) => UpdateAssignmentStatus(selectedAssignment, Assignment.AssignmentStatus.InProgress);
                var completedMenuItem = new System.Windows.Controls.MenuItem { Header = "Completed" };
                completedMenuItem.Click += (s, ev) => UpdateAssignmentStatus(selectedAssignment, Assignment.AssignmentStatus.Completed);

                statusSubMenu.Items.Add(notStartedMenuItem);
                statusSubMenu.Items.Add(inProgressMenuItem);
                statusSubMenu.Items.Add(completedMenuItem);

                contextMenu.Items.Add(copyMenuItem);
                contextMenu.Items.Add(editMenuItem);
                contextMenu.Items.Add(deleteMenuItem);
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(setUnitCodeColourMenuItem);
                contextMenu.Items.Add(setTaskGradeColourMenuItem);
                contextMenu.Items.Add(new Separator());
                contextMenu.Items.Add(statusSubMenu);

                contextMenu.IsOpen = true;
            }
        }

        private void UpdateAssignmentStatus(Assignment assignment, Assignment.AssignmentStatus status)
        {
            viewModel.UpdateAssignmentStatus(assignment, status);
            CollectionViewSource.GetDefaultView(AssignmentListView.ItemsSource)?.Refresh();
            ApplyCustomColors();
            RefreshDashboardData();
        }

        private void CopyAssignment(Assignment assignment)
        {
            var copiedAssignment = new Assignment
            {
                UnitCode = assignment.UnitCode,
                TaskName = assignment.TaskName,
                TaskGrade = assignment.TaskGrade,
                StartDate = assignment.StartDate,
                DueDate = assignment.DueDate,
                Notes = assignment.Notes
            };
            viewModel.AddAssignment(copiedAssignment);
            ApplyCustomColors();
            RefreshDashboardData();
        }

        private void EditAssignment(Assignment assignment)
        {
            var editWindow = new AddAssignmentWindow(unitCodes)
            {
                Assignment = assignment
            };
            editWindow.InitializeFields();
            if (editWindow.ShowDialog() == true)
            {
                assignment.UnitCode = editWindow.Assignment.UnitCode;
                assignment.TaskName = editWindow.Assignment.TaskName;
                assignment.TaskGrade = editWindow.Assignment.TaskGrade;
                assignment.StartDate = editWindow.Assignment.StartDate;
                assignment.DueDate = editWindow.Assignment.DueDate;
                assignment.Notes = editWindow.Assignment.Notes;
                assignment.ReminderDate = editWindow.Assignment.ReminderDate;

                ApplyCustomColors();
                RefreshDashboardData();
            }
        }

        private void DeleteAssignment(Assignment assignment)
        {
            viewModel.Assignments.Remove(assignment);
            RefreshDashboardData(); // Ensure data is refreshed
        }

        private void ImportDataButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx",
                Title = "Select an Excel File"
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                ImportDataFromExcel(filePath);
            }
        }

        private void ImportDataFromExcel(string filePath)
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial; // Set the license context

            FileInfo fileInfo = new FileInfo(filePath);
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    string unitCode = worksheet.Cells[row, 1].Text;
                    string taskName = worksheet.Cells[row, 2].Text;
                    string taskGrade = worksheet.Cells[row, 3].Text;
                    DateTime startDate = DateTime.Parse(worksheet.Cells[row, 4].Text);
                    DateTime dueDate = DateTime.Parse(worksheet.Cells[row, 5].Text);
                    string notes = worksheet.Cells[row, 6].Text;

                    var newAssignment = new Assignment
                    {
                        UnitCode = unitCode,
                        TaskName = taskName,
                        TaskGrade = taskGrade,
                        StartDate = startDate,
                        DueDate = dueDate,
                        Notes = notes
                    };

                    viewModel.AddAssignment(newAssignment);

                    if (!unitCodes.Contains(unitCode))
                    {
                        unitCodes.Add(unitCode);
                        AssignColorToUnitCode(unitCode);
                    }
                }

                ApplyCustomColors();
                RefreshDashboardData(); // Ensure data is refreshed
            }
        }

        private void RefreshDashboardData()
        {
            if (DashboardUserControl != null && DashboardUserControl.Content is DashboardUserControl dashboard)
            {
                dashboard.RefreshData();
            }
        }
        public class TaskGradeComparer : IComparer
        {
            private readonly ListSortDirection _direction;
            private readonly Dictionary<string, int> _gradeOrder;

            public TaskGradeComparer(ListSortDirection direction, Dictionary<string, int> gradeOrder)
            {
                _direction = direction;
                _gradeOrder = gradeOrder;
            }

            public int Compare(object x, object y)
            {
                if (x is Assignment a1 && y is Assignment a2)
                {
                    int result = _gradeOrder[a1.TaskGrade].CompareTo(_gradeOrder[a2.TaskGrade]);
                    return _direction == ListSortDirection.Ascending ? result : -result;
                }
                return 0;
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete || e.Key == System.Windows.Input.Key.Back)
            {
                DeleteSelectedAssignments();
            }
            else if ((e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case System.Windows.Input.Key.C:
                        CopySelectedAssignments();
                        break;
                    case System.Windows.Input.Key.X:
                        CutSelectedAssignments();
                        break;
                    case System.Windows.Input.Key.V:
                        PasteAssignments();
                        break;
                    case System.Windows.Input.Key.N:
                        AddNewAssignment();
                        break;
                }
            }
            else if (e.Key == System.Windows.Input.Key.Tab)
            {
                MoveToNextTab();
            }
        }

        private void DeleteSelectedAssignments()
        {
            var selectedAssignments = AssignmentListView.SelectedItems.Cast<Assignment>().ToList();
            foreach (var assignment in selectedAssignments)
            {
                DeleteAssignment(assignment);
            }
        }

        private List<Assignment> clipboardAssignments = new List<Assignment>();

        private void CopySelectedAssignments()
        {
            clipboardAssignments = AssignmentListView.SelectedItems.Cast<Assignment>().Select(a => (Assignment)a.Clone()).ToList();
        }

        private void CutSelectedAssignments()
        {
            CopySelectedAssignments();
            DeleteSelectedAssignments();
        }

        private void PasteAssignments()
        {
            foreach (var assignment in clipboardAssignments)
            {
                viewModel.AddAssignment((Assignment)assignment.Clone());
            }
            clipboardAssignments.Clear();
            ApplyCustomColors();
            RefreshDashboardData();
        }

        private void AddNewAssignment()
        {
            AddAssignmentButton_Click(null, null);
        }

        private void MoveToNextTab()
        {
            var currentIndex = MainTabControl.SelectedIndex;
            var nextIndex = (currentIndex + 1) % MainTabControl.Items.Count;
            MainTabControl.SelectedIndex = nextIndex;
        }

    }
}

