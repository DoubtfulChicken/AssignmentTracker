using System.Collections.ObjectModel;
using System.Linq;
using AssignmentTracker.Models;

namespace AssignmentTracker.ViewModels
{
    public class AssignmentViewModel
    {
        public ObservableCollection<Assignment> Assignments { get; set; } = new ObservableCollection<Assignment>();

        public void AddAssignment(Assignment assignment)
        {
            Assignments.Add(assignment);
        }

        public void RemoveAssignment(Assignment assignment)
        {
            Assignments.Remove(assignment);
        }

        public void UpdateAssignmentStatus(Assignment assignment, Assignment.AssignmentStatus status)
        {
            var existingAssignment = Assignments.FirstOrDefault(a => a == assignment);
            if (existingAssignment != null)
            {
                existingAssignment.Status = status;
            }
        }
    }
}
