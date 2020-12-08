using Shos.Collections;
using System.Linq;

namespace Shos.UndoRedoList.SampleApp.Models
{
    public class StaffList : UndoRedoObservableCollection<Staff>
    {
        static int staffId = 0;

        public new void Add(Staff staff)
        {
            if (string.IsNullOrWhiteSpace(staff.Name))
                return;

            staffId = staffId == 0 ? (Count == 0 ? 1 : this.Select(staff => staff.Id).Max() + 1)
                                   : staffId + 1;
            staff.Id = staffId;
            base.Add(staff);
        }
    }
}
