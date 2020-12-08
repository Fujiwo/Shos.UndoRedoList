using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shos.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Shos.UndoRedoList.Tests
{
    [TestClass]
    public class ModuloArithmeticTest
    {
        [TestMethod]
        public void Construct()
        {
            var target = new ModuloArithmetic(3);
            Assert.IsTrue(target.IsValid);
            Assert.AreEqual(3, target.Divisor);
            Assert.AreEqual(0, target.Value);
            Assert.AreEqual(new ModuloArithmetic(3), target);
            Assert.IsTrue(new ModuloArithmetic(5) == target);
            Assert.IsTrue(target == 0);
            Assert.IsTrue(0 == target);
            Assert.IsFalse(new ModuloArithmetic(3) != target);
            Assert.IsFalse(target != 0);
            Assert.IsFalse(0 != target);
            Assert.IsFalse(new ModuloArithmetic(6, false).IsValid);
        }

        [TestMethod]
        public void Next()
        {
            Assert.AreEqual(new ModuloArithmetic(3) { Value = 1 }, new ModuloArithmetic(3).Next);
        }

        [TestMethod]
        public void Previous()
        {
            Assert.AreEqual(new ModuloArithmetic(3) { Value = 2 }, new ModuloArithmetic(3).Previous);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InitializeWidthOne()
        {
            var target = new ModuloArithmetic(1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InitializeWidthZero()
        {
            var target = new ModuloArithmetic(0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InitializeWidthMinusValue()
        {
            var target = new ModuloArithmetic(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TooLargeValue()
        {
            var target = new ModuloArithmetic(3) { Value = 3 };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MinusValue()
        {
            var target = new ModuloArithmetic { Value = -1 };
        }

        [TestMethod]
        public void Increment()
        {
            var target = new ModuloArithmetic(3);
            Assert.AreEqual(1, (++target).Value);
            Assert.AreEqual(1, target.Value);
            Assert.AreEqual(1, (target++).Value);
            Assert.AreEqual(2, target.Value);
            target.MoveNext();
            Assert.AreEqual(0, target.Value);
        }

        [TestMethod]
        public void Decrement()
        {
            var target = new ModuloArithmetic(4) { Value = 2 };
            Assert.AreEqual(2, (target--).Value);
            Assert.AreEqual(0, (--target).Value);
            target.MovePrevious();
            Assert.AreEqual(3, target.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidMinus()
        {
            var result = new ModuloArithmetic(3) - new ModuloArithmetic(4);
        }

        [TestMethod]
        public void Minus()
        {
            Assert.AreEqual(0, new ModuloArithmetic(3) { Value = 2 } - new ModuloArithmetic(3) { Value = 2 });
            Assert.AreEqual(1, new ModuloArithmetic(3) { Value = 2 } - new ModuloArithmetic(3) { Value = 1 });
            Assert.AreEqual(2, new ModuloArithmetic(3) { Value = 1 } - new ModuloArithmetic(3) { Value = 2 });
        }
    }

    [TestClass]
    public class RingBufferTest
    {
        [TestMethod]
        public void Construct()
        {
            var target = new RingBuffer<int>();
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        public void Add()
        {
            var target = new RingBuffer<int>(3) { 100 };
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(100, target[new ModuloArithmetic(3)]);
            target.Add(300);
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual(100, target[new ModuloArithmetic(3)]);
            Assert.AreEqual(300, target[new ModuloArithmetic(3) { Value = 1 }]);
            target.Add(200);
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(200, target[new ModuloArithmetic(3) { Value = 2 }]);
            target.Add(-100);
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(200, target[new ModuloArithmetic(3) { Value = 2 }]);
            Assert.AreEqual(-100, target[new ModuloArithmetic(3) { Value = 0 }]);
            target.Add(-300);
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(-100, target[new ModuloArithmetic(3) { Value = 0 }]);
            Assert.AreEqual(-300, target[new ModuloArithmetic(3) { Value = 1 }]);
        }

        [TestMethod]
        public void ForLoop()
        {
            var target = new RingBuffer<int>(3) { 100, 300, 200, 400 };

            //var count = target.Count;
            var index = target.TopIndex;
            for (var count = target.Count; count >= 0; index++, count--) {
                switch (index) {
                    case ModuloArithmetic i when i == target.TopIndex:
                        Assert.AreEqual(1, i.Value);
                        Assert.AreEqual(300, target[i]);
                        break;
                    case ModuloArithmetic i when i == target.TopIndex.Next:
                        Assert.AreEqual(2, i.Value);
                        Assert.AreEqual(200, target[i]);
                        break;
                    case ModuloArithmetic i when i == target.TopIndex.Next.Next:
                        Assert.AreEqual(0, i.Value);
                        Assert.AreEqual(400, target[i]);
                        break;
                    default:
                        Assert.Fail();
                        break;
                }
            }
        }

        [TestMethod]
        public void ForEachLoop()
        {
            var target = new RingBuffer<int>(3) { 100, 300, 200, 400, 500 };
            var index = target.TopIndex;
            foreach (var element in target) {
                switch (index) {
                    case ModuloArithmetic i when i == target.TopIndex:
                        Assert.AreEqual(2, i.Value);
                        Assert.AreEqual(200, target[i]);
                        break;
                    case ModuloArithmetic i when i == target.TopIndex.Next:
                        Assert.AreEqual(0, i.Value);
                        Assert.AreEqual(400, target[i]);
                        break;
                    case ModuloArithmetic i when i == target.TopIndex.Next.Next:
                        Assert.AreEqual(1, i.Value);
                        Assert.AreEqual(500, target[i]);
                        break;
                    default:
                        Assert.Fail();
                        break;
                }
                index++;
            }
        }

        [TestMethod]
        public void Clear()
        {
            var target = new RingBuffer<int>(3) { -100, 300, -200, 400 };
            target.Clear();
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        public void Remove()
        {
            var target = new RingBuffer<int>(3) { -100, 300, -200, 400 };
            Assert.IsTrue(target.Remove());
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual(300, target[target.TopIndex]);
            Assert.AreEqual(-200, target[target.TopIndex.Next]);
            Assert.IsTrue(target.Remove());
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(300, target[target.TopIndex]);
            Assert.IsTrue(target.Remove());
            Assert.AreEqual(0, target.Count);
            Assert.IsFalse(target.Remove());
        }

        [TestMethod]
        public void RemoveAfter()
        {
            var target = new RingBuffer<int> { 10, 20, 30, 40 };
            target.RemoveAfter(target.BottomIndex);
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(10, target[target.TopIndex]);
            Assert.AreEqual(20, target[target.TopIndex.Next]);
            Assert.AreEqual(30, target[target.TopIndex.Next.Next]);
            target.RemoveAfter(target.BottomIndex.Previous);
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(10, target[target.TopIndex]);
            target.Add(100);
            target.Add(200);
            target.RemoveAfter(target.TopIndex);
            Assert.AreEqual(0, target.Count);
        }
    }

    [TestClass]
    public class UndoRedoRingBufferTest
    {
        [TestMethod]
        public void Test()
        {
            var target = new UndoRedoRingBuffer<int>(3);
            Assert.AreEqual(0, target.Count);
            Assert.IsFalse(target.CanGoBackward);
            Assert.IsFalse(target.CanGoForward);
            target.Add(100);
            Assert.AreEqual(1, target.Count);
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsFalse(target.CanGoForward);
            target.Add(200);
            target.Add(300);
            Assert.AreEqual(3, target.Count);
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsFalse(target.CanGoForward);
            Assert.IsTrue(target.GoBackward());
            Assert.AreEqual(200, target.Current);
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsTrue(target.CanGoForward);
            Assert.IsTrue(target.GoBackward());
            Assert.AreEqual(100, target.Current);
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsTrue(target.CanGoForward);
            Assert.IsTrue(target.GoBackward());
            Assert.IsFalse(target.CanGoBackward);
            Assert.IsTrue(target.GoForward());
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsTrue(target.CanGoForward);
            Assert.AreEqual(100, target.Current);
            Assert.IsTrue(target.GoForward());
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsTrue(target.CanGoForward);
            Assert.AreEqual(200, target.Current);
            Assert.IsTrue(target.GoForward());
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsFalse(target.CanGoForward);
            Assert.AreEqual(300, target.Current);
            Assert.IsTrue(target.GoBackward());
            Assert.IsTrue(target.GoBackward());
            Assert.IsTrue(target.GoBackward());
            target.Add(500);
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsFalse(target.CanGoForward);
            Assert.AreEqual(500, target.Current);
        }
    }

    [TestClass]
    public class UndoRedoListTest
    {
        [TestMethod]
        public void Construct()
        {
            var list = new UndoRedoList<int, List<int>>();
            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(0, list.List.Count);
            Assert.IsFalse(list.CanUndo);
            Assert.IsFalse(list.CanRedo);
        }

        [TestMethod]
        public void Undo()
        {
            var list = new UndoRedoList<int, List<int>>();
            Assert.IsFalse(list.Undo());

            list.Add(100);
            Assert.IsTrue(list.CanUndo);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(100, list[0]);

            list.Add(200);
            Assert.IsTrue(list.CanUndo);
            Assert.IsFalse(list.CanRedo);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(100, list[0]);
            Assert.AreEqual(200, list[1]);

            Assert.IsTrue(list.Undo());
            Assert.IsTrue(list.CanUndo);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(100, list[0]);

            Assert.IsTrue(list.Undo());
            Assert.IsFalse(list.CanUndo);
            Assert.AreEqual(0, list.Count);

            list.Add(300);
            Assert.IsTrue(list.CanUndo);
            list.Add(400);
            list.RemoveAt(0);
            Assert.IsTrue(list.CanUndo);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(400, list[0]);

            Assert.IsTrue(list.Undo());
            Assert.IsTrue(list.CanUndo);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(300, list[0]);
            Assert.AreEqual(400, list[1]); 

            list.Add(500);
            list[list.Count - 1] = 600;
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(600, list[list.Count - 1]);
            Assert.IsTrue(list.CanUndo);
            Assert.IsTrue(list.Undo());
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(500, list[list.Count - 1]);

            list.Clear();
            Assert.AreEqual(0, list.Count);
            Assert.IsTrue(list.CanUndo);
            Assert.IsTrue(list.Undo());
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(300, list[0]);
            Assert.AreEqual(400, list[1]);
            Assert.AreEqual(500, list[2]);
        }

        [TestMethod]
        public void Redo()
        {
            var list = new UndoRedoList<int, List<int>> { 100, 200 };
            Assert.IsFalse(list.CanRedo);
            Assert.IsFalse(list.Redo());

            list.Undo();
            Assert.IsTrue(list.CanRedo);

            Assert.IsTrue(list.Redo());
            Assert.IsFalse(list.CanRedo);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(100, list[0]);
            Assert.AreEqual(200, list[1]);

            list.Undo();
            Assert.IsTrue(list.CanRedo);
            list.Add(300);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(100, list[0]);
            Assert.AreEqual(300, list[1]);
            Assert.IsFalse(list.CanRedo);

            list.RemoveAt(0);
            list.Undo();
            Assert.IsTrue(list.CanRedo);

            Assert.IsTrue(list.Redo());
            Assert.IsFalse(list.CanRedo);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(300, list[0]);

            list.Add(500);
            list[list.Count - 1] = 600;
            Assert.AreEqual(2, list.Count);
            list.Undo();
            Assert.IsTrue(list.CanRedo);
            Assert.IsTrue(list.Redo());
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(600, list[list.Count - 1]);

            list.Clear();
            list.Undo();
            Assert.IsTrue(list.CanRedo);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(300, list[0]);
            Assert.AreEqual(600, list[1]);
            Assert.IsTrue(list.Redo());
            Assert.AreEqual(0, list.Count);
            Assert.IsTrue(list.CanUndo);
            Assert.IsFalse(list.CanRedo);
        }

        [TestMethod]
        public void Scope()
        {
            var list = new UndoRedoList<int, List<int>> { 100, 200 };
            using (var scope = new UndoRedoList<int, List<int>>.ActionScope(list)) {
                list.Add(300);
                list.RemoveAt(0);
                list.RemoveAt(1);
                list.Add(400);
                list.Add(500);
            }
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(200, list[0]);
            Assert.AreEqual(400, list[1]);
            Assert.AreEqual(500, list[2]);
            Assert.IsTrue(list.CanUndo);
            Assert.IsFalse(list.CanRedo);

            Assert.IsTrue(list.Undo());
            Assert.IsTrue(list.CanUndo);
            Assert.IsTrue(list.CanRedo);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(100, list[0]);
            Assert.AreEqual(200, list[1]);

            Assert.IsTrue(list.Redo());
            Assert.IsTrue(list.CanUndo);
            Assert.IsFalse(list.CanRedo);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(200, list[0]);
            Assert.AreEqual(400, list[1]);
            Assert.AreEqual(500, list[2]);
        }
    }

    [TestClass]
    public class UndoRedoObservableCollectionTest
    {
        UndoRedoObservableCollection<int>   target;
        List<NotifyCollectionChangedAction> notifyCollectionChangedActions;

        [TestInitialize]
        public void Initialize()
        {
            target = new UndoRedoObservableCollection<int>();
            target.CollectionChanged += OnCollectionChanged;
            notifyCollectionChangedActions = new List<NotifyCollectionChangedAction>();
        }

        [TestMethod]
        public void Test()
        {
            Assert.AreEqual(0, notifyCollectionChangedActions.Count);
            target.Add(100);
            Assert.AreEqual(1, notifyCollectionChangedActions.Count);
            target.RemoveAt(0);
            Assert.AreEqual(2, notifyCollectionChangedActions.Count);
            target.Undo();
            Assert.AreEqual(3, notifyCollectionChangedActions.Count);
            target.Undo();
            Assert.AreEqual(4, notifyCollectionChangedActions.Count);
            target.Redo();
            Assert.AreEqual(5, notifyCollectionChangedActions.Count);
            target.Redo();
            Assert.AreEqual(6, notifyCollectionChangedActions.Count);
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => notifyCollectionChangedActions.Add(e.Action);
    }
}
