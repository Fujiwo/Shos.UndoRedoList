using Prism.Mvvm;

namespace Shos.UndoRedoList.SampleApp.Models
{
    public class Staff : BindableBase
    {
        int id = 0;

        public int Id {
            get => id;
            set => SetProperty(ref id, value);
        }

        string name = "";

        public string Name {
            get => name;
            set => SetProperty(ref name, value);
        }
    }
}
