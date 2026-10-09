namespace Shos.Collections
{
    /// <summary>Specialized RingBuffer for undo/redo.</summary>
    /// <typeparam name="TElement">Type of elements</typeparam>
    public class UndoRedoRingBuffer<TElement> : RingBuffer<TElement>
    {
        /// <summary>Index of current element.</summary>
        public ModuloArithmetic CurrentIndex { get; private set; }
        /// <summary>Current element.</summary>
        public TElement Current => this[CurrentIndex];
        public bool CanGoBackward => CurrentIndex.IsValid;
        public bool CanGoForward => CurrentIndex != BottomIndex;

        /// <summary>Initializes a new instance with the size.</summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when size is 1 or less.</exception>
        public UndoRedoRingBuffer(int size = defaultSize) : base(size) => CurrentIndex = new ModuloArithmetic(size, false);

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
}
