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
            public IList<TElement> Container { get; set; }

            public abstract void Undo();
            public abstract void Redo();
        }

        class AddAction : Action
        {
            public TElement NewElement { get; set; }

            public override void Undo() => Container.Remove(NewElement);
            public override void Redo() => Container.Add(NewElement);
        }

        class RemoveAction : Action
        {
            public TElement OldElement { get; set; }

            public override void Undo() => Container.Add(OldElement);
            public override void Redo() => Container.Remove(OldElement);
        }

        class ExchangeAction : Action
        {
            public TElement OldElement { get; set; }
            public TElement NewElement { get; set; }

            public override void Undo() => Container[Container.IndexOf(NewElement)] = OldElement;
            public override void Redo() => Container[Container.IndexOf(OldElement)] = NewElement;
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

            public void Dispose()
            {
                list.EndAction();
            }
        }

        public TList List { get; } = new TList();

        public int Count => List.Count;

        public bool IsReadOnly => List.IsReadOnly;

        public TElement this[int index] {
            get => List[index];
            set {
                Add(new ExchangeAction { Container = List, OldElement = List[index], NewElement = value });
                List[index] = value;
            }
        }

        readonly UndoRedoRingBuffer<Action> undoBuffer;

        List<Action> actions = new List<Action>();
        bool HasBeganAction { get; set; } = false;

        public UndoRedoList(int bufferSize = ModuloArithmetic.DefaultDivisor)
            => undoBuffer = new UndoRedoRingBuffer<Action>(bufferSize);

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

        public void Add(TElement element)
        {
            Add(new AddAction { Container = List, NewElement = element });
            List.Add(element);
        }

        public void Clear()
        {
            var actionCollection = new ActionCollection { Container = List };
            for (var index = List.Count - 1; index >= 0; index--)
                actionCollection.Add(new RemoveAction { Container = List, OldElement = List[index] });
            Add(actionCollection);
            List.Clear();
        }

        public bool Contains(TElement element) => List.Contains(element);

        public void CopyTo(TElement[] array, int arrayIndex) => List.CopyTo(array, arrayIndex);

        public IEnumerator<TElement> GetEnumerator() => List.GetEnumerator();

        public int IndexOf(TElement element) => List.IndexOf(element);

        public void Insert(int index, TElement element)
        {
            Add(new AddAction { Container = List, NewElement = element });
            List.Insert(index, element);
        }

        public bool Remove(TElement element)
        {
            Add(new RemoveAction { Container = List, OldElement = element });
            return List.Remove(element);
        }

        public void RemoveAt(int index) => Remove(List[index]);

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
            if (HasBeganAction)
                actions.Add(action);
            else
                undoBuffer.Add(action);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
