using System.Collections.ObjectModel;
using System.Linq;
using AssignmentTracker.Models;
using System.ComponentModel;
using System.Windows.Data;

namespace AssignmentTracker.ViewModels
{
    public class AssignmentViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Assignment> Assignments { get; set; } = new ObservableCollection<Assignment>();
        public ICollectionView FilteredAssignments { get; private set; }

        private string searchText;
        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilteredAssignments.Refresh();
            }
        }

        private Assignment.AssignmentStatus? statusFilter;
        public Assignment.AssignmentStatus? StatusFilter
        {
            get => statusFilter;
            set
            {
                statusFilter = value;
                OnPropertyChanged(nameof(StatusFilter));
                FilteredAssignments.Refresh();
            }
        }

        private string unitCodeFilter;
        public string UnitCodeFilter
        {
            get => unitCodeFilter;
            set
            {
                unitCodeFilter = value;
                OnPropertyChanged(nameof(UnitCodeFilter));
                FilteredAssignments.Refresh();
            }
        }

        public AssignmentViewModel()
        {
            FilteredAssignments = CollectionViewSource.GetDefaultView(Assignments);
            FilteredAssignments.Filter = FilterAssignments;
        }

        private bool FilterAssignments(object obj)
        {
            if (obj is Assignment assignment)
            {
                bool matchesSearchText = string.IsNullOrEmpty(SearchText) ||
                                         assignment.TaskName.IndexOf(SearchText, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         assignment.UnitCode.IndexOf(SearchText, System.StringComparison.OrdinalIgnoreCase) >= 0;

                bool matchesStatus = !StatusFilter.HasValue || assignment.Status == StatusFilter.Value;
                bool matchesUnitCode = string.IsNullOrEmpty(UnitCodeFilter) || assignment.UnitCode == UnitCodeFilter;

                return matchesSearchText && matchesStatus && matchesUnitCode;
            }
            return false;
        }

        public void AddAssignment(Assignment assignment)
        {
            Assignments.Add(assignment);
            FilteredAssignments.Refresh();
        }

        public void RemoveAssignment(Assignment assignment)
        {
            Assignments.Remove(assignment);
            FilteredAssignments.Refresh();
        }

        public void UpdateAssignmentStatus(Assignment assignment, Assignment.AssignmentStatus status)
        {
            var existingAssignment = Assignments.FirstOrDefault(a => a == assignment);
            if (existingAssignment != null)
            {
                existingAssignment.Status = status;
                FilteredAssignments.Refresh();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
