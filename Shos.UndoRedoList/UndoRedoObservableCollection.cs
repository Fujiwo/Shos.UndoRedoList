using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Shos.Collections;

/// <summary>ObservableCollection which supports undo/redo.</summary>
/// <typeparam name="TElement">type of elements</typeparam>
public class UndoRedoObservableCollection<TElement> : UndoRedoList<TElement, ObservableCollection<TElement>>, INotifyCollectionChanged
{
    #region INotifyCollectionChanged implementation
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    #endregion

    /// <summary>Initializes a new instance with the maximum undo times.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maximumUndoTimes is 1 or less.</exception>
    public UndoRedoObservableCollection(int maximumUndoTimes = ModuloArithmetic.DefaultDivisor) : base(maximumUndoTimes) => List.CollectionChanged += (_, e) => CollectionChanged?.Invoke(this, e);
}
