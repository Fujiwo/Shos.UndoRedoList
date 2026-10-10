using System.Collections;

namespace Shos.Collections;

/// <summary>Thread-safe IList implemented collection which supports undo/redo.</summary>
/// <remarks>
/// Every member is synchronized with <see cref="SyncRoot"/>.
/// Values such as <see cref="Count"/> and <see cref="CanUndo"/> may be out of date as soon as they are returned,
/// so use the return value of <see cref="Undo"/>/<see cref="Redo"/> or <see cref="ExecuteAsOneAction{TResult}(Func{IList{TElement}, TResult})"/>
/// instead of "check then act".
/// </remarks>
/// <typeparam name="TElement">type of elements</typeparam>
/// <typeparam name="TList">type of IList implemented collection</typeparam>
/// <param name="maximumUndoTimes">maximum undo times</param>
/// <exception cref="ArgumentOutOfRangeException">Thrown when maximumUndoTimes is 1 or less.</exception>
public class SynchronizedUndoRedoList<TElement, TList>(int maximumUndoTimes = ModuloArithmetic.DefaultDivisor) : IList<TElement> where TList : IList<TElement>, new()
{
    readonly UndoRedoList<TElement, TList> list     = new(maximumUndoTimes);
    readonly object                        syncRoot = new();

    /// <summary>Object used to synchronize access. Lock it to do several operations atomically.</summary>
    public object SyncRoot => syncRoot;

    public bool CanUndo {
        get {
            lock (syncRoot)
                return list.CanUndo;
        }
    }

    public bool CanRedo {
        get {
            lock (syncRoot)
                return list.CanRedo;
        }
    }

    /// <summary>You can't undo actions while UndoEnabled is false. Use <see cref="ExecuteWithoutUndo"/> to change it.</summary>
    public bool UndoEnabled {
        get {
            lock (syncRoot)
                return list.UndoEnabled;
        }
    }

    public bool Undo()
    {
        lock (syncRoot)
            return list.Undo();
    }

    public bool Redo()
    {
        lock (syncRoot)
            return list.Redo();
    }

    public void ClearUndo()
    {
        lock (syncRoot)
            list.ClearUndo();
    }

    /// <summary>
    /// Executes the action atomically. Changes in the action can undo in one time.
    /// </summary>
    /// <remarks>
    /// The action runs while holding the lock: don't keep the passed list after the action returns,
    /// don't call <see cref="ExecuteAsOneAction(Action{IList{TElement}})"/> from the action, and keep it short.
    /// If an exception is thrown in the action, the changes made before the exception are recorded as one undo step.
    /// </remarks>
    public void ExecuteAsOneAction(Action<IList<TElement>> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ExecuteAsOneAction(list => { action(list); return true; });
    }

    /// <summary>
    /// Executes the function atomically and returns its result. Changes in the function can undo in one time.
    /// </summary>
    /// <remarks>See <see cref="ExecuteAsOneAction(Action{IList{TElement}})"/>.</remarks>
    public TResult ExecuteAsOneAction<TResult>(Func<IList<TElement>, TResult> function)
    {
        ArgumentNullException.ThrowIfNull(function);
        lock (syncRoot) {
            using var scope = new UndoRedoList<TElement, TList>.ActionScope(list);
            return function(list);
        }
    }

    /// <summary>
    /// Executes the action atomically. Changes in the action are not recorded (you can't undo them).
    /// </summary>
    /// <remarks>See <see cref="ExecuteAsOneAction(Action{IList{TElement}})"/>.</remarks>
    public void ExecuteWithoutUndo(Action<IList<TElement>> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        lock (syncRoot) {
            using var scope = new UndoRedoList<TElement, TList>.DisabledUndoScope(list);
            action(list);
        }
    }

    /// <summary>Returns a copy of the elements.</summary>
    public TElement[] ToArray()
    {
        lock (syncRoot) {
            var array = new TElement[list.Count];
            list.CopyTo(array, 0);
            return array;
        }
    }

    #region IList<T> implementation
    public int Count {
        get {
            lock (syncRoot)
                return list.Count;
        }
    }

    public bool IsReadOnly {
        get {
            lock (syncRoot)
                return list.IsReadOnly;
        }
    }

    public TElement this[int index] {
        get {
            lock (syncRoot)
                return list[index];
        }
        set {
            lock (syncRoot)
                list[index] = value;
        }
    }

    public void Add(TElement element)
    {
        lock (syncRoot)
            list.Add(element);
    }

    public void Clear()
    {
        lock (syncRoot)
            list.Clear();
    }

    public bool Contains(TElement element)
    {
        lock (syncRoot)
            return list.Contains(element);
    }

    public void CopyTo(TElement[] array, int arrayIndex)
    {
        lock (syncRoot)
            list.CopyTo(array, arrayIndex);
    }

    public int IndexOf(TElement element)
    {
        lock (syncRoot)
            return list.IndexOf(element);
    }

    public void Insert(int index, TElement element)
    {
        lock (syncRoot)
            list.Insert(index, element);
    }

    public bool Remove(TElement element)
    {
        lock (syncRoot)
            return list.Remove(element);
    }

    public void RemoveAt(int index)
    {
        lock (syncRoot)
            list.RemoveAt(index);
    }

    /// <summary>Enumerates a snapshot of the elements (changes after this call are not reflected).</summary>
    public IEnumerator<TElement> GetEnumerator() => ((IEnumerable<TElement>)ToArray()).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}

/// <summary>Thread-safe list which supports undo/redo.</summary>
/// <typeparam name="TElement">type of elements</typeparam>
/// <param name="maximumUndoTimes">maximum undo times</param>
/// <exception cref="ArgumentOutOfRangeException">Thrown when maximumUndoTimes is 1 or less.</exception>
public class SynchronizedUndoRedoList<TElement>(int maximumUndoTimes = ModuloArithmetic.DefaultDivisor) : SynchronizedUndoRedoList<TElement, List<TElement>>(maximumUndoTimes);
