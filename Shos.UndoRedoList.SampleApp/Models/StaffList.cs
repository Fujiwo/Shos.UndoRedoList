using Shos.Collections;

namespace Shos.UndoRedoList.SampleApp.Models
{
    public class StaffList : UndoRedoObservableCollection<Staff>
    {
        public new void Add(Staff staff)
        {
            staff.Id = Count + 1;
            base.Add(staff);
        }
    }
}
