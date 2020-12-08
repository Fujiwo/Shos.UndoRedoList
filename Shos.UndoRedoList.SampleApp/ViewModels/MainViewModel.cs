using Prism.Commands;
using Prism.Mvvm;
//using System;
//using System.IO;
//using System.Xml.Serialization;

namespace Shos.UndoRedoList.SampleApp.ViewModels
{
    using Models;

    public class MainViewModel : BindableBase //, IDisposable
    {
        public StaffList StaffList { get; private set; } = new StaffList();

        //const string applicationName = "Shos.UndoRedoList.SampleApp";
        //const string dataFileName    = "staffList.xml";
        //string applicationFolder => $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\{applicationName}";
        //string dataFilePath      => $@"{applicationFolder}\{dataFileName}";

        string newStaffName = "";

        public string NewStaffName {
            get => newStaffName;
            set => SetProperty(ref newStaffName, value);
        }

        public DelegateCommand        Add    { get; private set; }
        public DelegateCommand<Staff> Remove { get; private set; }
        public DelegateCommand        Clear  { get; private set; }
        public DelegateCommand        Undo   { get; private set; }
        public DelegateCommand        Redo   { get; private set; }

        public MainViewModel()
        {
            //Load();

            Add = new DelegateCommand(() => StaffList.Add(new Staff { Name = NewStaffName }));
            Remove = new DelegateCommand<Staff>(staff => StaffList.Remove(staff));
            Clear  = new DelegateCommand(() => StaffList.Clear(), () => StaffList.Count > 0);
            Undo   = new DelegateCommand(() => StaffList.Undo (), () => StaffList.CanUndo);
            Redo   = new DelegateCommand(() => StaffList.Redo (), () => StaffList.CanRedo);

            StaffList.CollectionChanged += (_, __) => {
                Clear.RaiseCanExecuteChanged();
                Undo .RaiseCanExecuteChanged();
                Redo .RaiseCanExecuteChanged();
            };
        }

        //public void Dispose() => Save();

        //void Load()
        //{
        //    if (!File.Exists(dataFilePath))
        //        return;

        //    var serializer = new XmlSerializer(typeof(StaffList));
        //    using (var reader = new StreamReader(dataFilePath)) {
        //        var staffList = serializer.Deserialize(reader) as StaffList;
        //        if (!(staffList is null)) {
        //            staffList.ClearUndo();
        //            StaffList = staffList;
        //        }
        //    }
        //}

        //void Save()
        //{
        //    if (!Directory.Exists(applicationFolder))
        //        Directory.CreateDirectory(applicationFolder);

        //    var serializer = new XmlSerializer(typeof(StaffList));
        //    using (var writer = new StreamWriter(dataFilePath))
        //        serializer.Serialize(writer, StaffList);
        //}
    }
}
