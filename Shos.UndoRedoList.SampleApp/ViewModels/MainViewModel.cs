using Prism.Commands;
using Prism.Mvvm;

namespace Shos.UndoRedoList.SampleApp.ViewModels
{
    using Models;

    public class MainViewModel : BindableBase
    {
        public StaffList StaffList { get; } = new StaffList();

        string newStaffName = "";

        public string NewStaffName {
            get => newStaffName;
            set => SetProperty(ref newStaffName, value);
        }

        public DelegateCommand Add { get; private set; }
        public DelegateCommand Clear { get; private set; }
        public DelegateCommand Undo { get; private set; }
        public DelegateCommand Redo { get; private set; }

        public MainViewModel()
        {
            Add   = new DelegateCommand(() => StaffList.Add(new Staff { Name = NewStaffName }));
            Clear = new DelegateCommand(() => StaffList.Clear(), () => StaffList.Count > 0);
            Undo  = new DelegateCommand(() => StaffList.Undo (), () => StaffList.CanUndo);
            Redo  = new DelegateCommand(() => StaffList.Redo (), () => StaffList.CanRedo);
        }
    }
}
