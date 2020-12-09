using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Shos.Collections
{
    public class UndoRedoObservableCollection<TElement> : UndoRedoList<TElement, ObservableCollection<TElement>>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public UndoRedoObservableCollection(int maximumUndoTimes = ModuloArithmetic.DefaultDivisor) : base(maximumUndoTimes)
        {}

        public UndoRedoObservableCollection() => List.CollectionChanged += (_, e) => CollectionChanged?.Invoke(this, e);
    }
}
