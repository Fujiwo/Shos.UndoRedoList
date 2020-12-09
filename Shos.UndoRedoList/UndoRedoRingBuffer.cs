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

        public UndoRedoRingBuffer(int size = defaultSize) : base(size) => CurrentIndex = new ModuloArithmetic(size, false);

        public override void Add(TElement element)
        {
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
            CurrentIndex++;
            return true;
        }
    }
}
