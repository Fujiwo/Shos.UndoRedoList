namespace Shos.Collections;

/// <summary>Specialized RingBuffer for undo/redo.</summary>
/// <typeparam name="TElement">Type of elements</typeparam>
/// <param name="size">size of the buffer</param>
/// <exception cref="ArgumentOutOfRangeException">Thrown when size is 1 or less.</exception>
public class UndoRedoRingBuffer<TElement>(int size = RingBuffer<TElement>.defaultSize) : RingBuffer<TElement>(size)
{
    /// <summary>Index of current element.</summary>
    public ModuloArithmetic CurrentIndex { get; private set; } = new(size, false);
    /// <summary>Current element.</summary>
    public TElement Current => this[CurrentIndex];
    public bool CanGoBackward => CurrentIndex.IsValid;
    public bool CanGoForward => CurrentIndex != BottomIndex;

    public override void Add(TElement element)
    {
        // Remove elements after the current element (they can't be redone any more).
        if (!CurrentIndex.IsValid)
            Clear();
        else if (CurrentIndex != BottomIndex)
            RemoveAfter(CurrentIndex.Next);
        base.Add(element);
        CurrentIndex = BottomIndex;
    }

    public override void Clear()
    {
        base.Clear();
        CurrentIndex = CurrentIndex.InvalidItem;
    }

    public bool GoBackward()
    {
        if (!CanGoBackward)
            return false;
        if (CurrentIndex == TopIndex)
            CurrentIndex = CurrentIndex.InvalidItem;
        else
            CurrentIndex--;
        return true;
    }

    public bool GoForward()
    {
        if (!CanGoForward)
            return false;
        // Invalid CurrentIndex means "before TopIndex".
        CurrentIndex = CurrentIndex.IsValid ? CurrentIndex.Next : TopIndex;
        return true;
    }
}
