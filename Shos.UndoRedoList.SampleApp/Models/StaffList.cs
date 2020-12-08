using Shos.Collections;

namespace Shos.UndoRedoList.SampleApp.Models
{
    public class StaffList : UndoRedoObservableCollection<Staff>
    {
        public new void Add(Staff staff)
        {
            if (string.IsNullOrWhiteSpace(staff.Name))
                return;

            staff.Id = Count + 1;
            base.Add(staff);
        }
    }
}
