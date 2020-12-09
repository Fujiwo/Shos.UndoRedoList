using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Shos.Collections
{
    public class UndoRedoList<TElement, TList> : IList<TElement> where TList : IList<TElement>, new()
    {
        abstract class Action
        {
            public IList<TElement> Container { get; set; } = null;
            public TElement Element { get; set; }
            public int Index { get; set; } = 0;

            public Action()
            { }
            public Action(IList<TElement> container, TElement element, int index) => (Container, Element, Index) = (container, element, index);

            public abstract void Undo();
            public abstract void Redo();
        }

        class AddAction : Action
        {
            public AddAction(IList<TElement> container, TElement element, int index) : base(container, element, index)
            {}

            public override void Undo() => Container.RemoveAt(Index);
            public override void Redo() => Container.Insert(Index, Element);
        }

        class RemoveAction : Action
        {
            public RemoveAction(IList<TElement> container, TElement element, int index) : base(container, element, index)
            {}

            public override void Undo() => Container.Insert(Index, Element);
            public override void Redo() => Container.RemoveAt(Index);
        }

        class ExchangeAction : Action
        {
            public ExchangeAction(IList<TElement> container, TElement oldElement, TElement newElement, int index) : base(container, newElement, index) => OldElement = oldElement;

            public TElement OldElement { get; set; }

            public override void Undo() => Container[Index] = OldElement;
            public override void Redo() => Container[Index] = Element;
        }

        class ActionCollection : Action, IEnumerable<Action>
        {
            List<Action> actions;

            public ActionCollection() => actions = new List<Action>();
            public ActionCollection(IEnumerable<Action> actions) => this.actions = actions.ToList();

            public void Add(Action action) => actions.Add(action);

            public override void Undo()
            {
                for (var index = actions.Count - 1; index >= 0; index--)
                    actions[index].Undo();
            }

            public override void Redo()
            {
                foreach (var action in actions)
                    action.Redo();
            }

            public IEnumerator<Action> GetEnumerator() => actions.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class ActionScope : IDisposable
        {
            readonly UndoRedoList<TElement, TList> list;

            public ActionScope(UndoRedoList<TElement, TList> list)
            {
                this.list = list;
                list.BeginAction();
            }

            public void Dispose() => list.EndAction();
        }


        public class DisabledUndoScope : IDisposable
        {
            readonly UndoRedoList<TElement, TList> list;
            readonly bool                          listUndoEnabled;

            public DisabledUndoScope(UndoRedoList<TElement, TList> list)
            {
                this.list = list;
                listUndoEnabled  = list.UndoEnabled;
                list.UndoEnabled = false;
            }

            public void Dispose() => list.UndoEnabled = listUndoEnabled;
        }

        public TList List { get; } = new TList();

        public int Count => List.Count;

        public bool IsReadOnly => List.IsReadOnly;

        public bool UndoEnabled { get; set; } = true;

        public TElement this[int index] {
            get => List[index];
            set {
                Add(new ExchangeAction(container: List, oldElement: List[index], newElement: value, index: index));
                List[index] = value;
            }
        }

        readonly UndoRedoRingBuffer<Action> undoBuffer;

        List<Action> actions = new List<Action>();
        bool HasBeganAction { get; set; } = false;

        public UndoRedoList(int maximumUndoTimes = ModuloArithmetic.DefaultDivisor)
            => undoBuffer = new UndoRedoRingBuffer<Action>(maximumUndoTimes);

        public bool CanUndo => undoBuffer.CanGoBackward;
        public bool CanRedo => undoBuffer.CanGoForward;

        public bool Undo()
        {
            if (CanUndo) {
                var action = undoBuffer.Current;
                action.Undo();
                undoBuffer.GoBackward();
                return true;
            }
            return false;
        }

        public bool Redo()
        {
            if (CanRedo) {
                undoBuffer.GoForward();
                var action = undoBuffer.Current;
                action.Redo();
                return true;
            }
            return false;
        }

        public void ClearUndo() => undoBuffer.Clear();

        public void Add(TElement element)
        {
            Add(new AddAction(container: List, element: element, index: List.Count));
            List.Add(element);
        }

        public void Clear()
        {
            var actionCollection = new ActionCollection { Container = List };
            for (var index = List.Count - 1; index >= 0; index--)
                actionCollection.Add(new RemoveAction(container: List, element: List[index], index: index));
            Add(actionCollection);
            List.Clear();
        }

        public bool Contains(TElement element) => List.Contains(element);

        public void CopyTo(TElement[] array, int arrayIndex) => List.CopyTo(array, arrayIndex);

        public IEnumerator<TElement> GetEnumerator() => List.GetEnumerator();

        public int IndexOf(TElement element) => List.IndexOf(element);

        public void Insert(int index, TElement element)
        {
            Add(new AddAction(container: List, element: element, index: index));
            List.Insert(index, element);
        }

        public bool Remove(TElement element)
        {
            var index = List.IndexOf(element);
            if (index < 0)
                return false;
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            Add(new RemoveAction(container: List, element: List[index], index: index));
            List.RemoveAt(index);
        }

        public void BeginAction()
        {
            if (HasBeganAction || actions.Count != 0)
                throw new InvalidOperationException();
            HasBeganAction = true;
        }

        public void EndAction()
        {
            if (!HasBeganAction)
                throw new InvalidOperationException();
            if (actions.Count == 1)
                undoBuffer.Add(actions[0]);
            else if (actions.Count > 1)
                undoBuffer.Add(new ActionCollection(actions));
            actions.Clear();
            HasBeganAction = false;
        }

        void Add(Action action)
        {
            if (!UndoEnabled)
                return;

            if (HasBeganAction)
                actions.Add(action);
            else
                undoBuffer.Add(action);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
