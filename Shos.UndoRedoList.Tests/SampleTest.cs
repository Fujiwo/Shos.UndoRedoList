using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shos.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Shos.UndoRedoList.Tests
{
    [TestClass]
    public class SampleTest
    {
        [TestMethod]
        public void UndoRedoTest()
        {
            // list which support undo/redo.
            var target = new UndoRedoList<int, List<int>>();

            Assert.IsFalse(target.CanUndo);
            Assert.IsFalse(target.CanRedo);

            // Modify target
            target.Add(100);
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(100, target[0]);
            Assert.IsTrue (target.CanUndo);
            Assert.IsFalse(target.CanRedo);

            // Undo
            Assert.IsTrue(target.Undo());

            Assert.AreEqual(0, target.Count);
            Assert.IsFalse(target.CanUndo);
            Assert.IsTrue (target.CanRedo);

            // Redo
            Assert.IsTrue(target.Redo());

            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(100, target[0]);
            Assert.IsTrue (target.CanUndo);
            Assert.IsFalse(target.CanRedo);
        }

        [TestMethod]
        public void ActionScopeTest()
        {
            // list which support undo/redo.
            var target = new UndoRedoList<int, List<int>>();
            Assert.IsFalse(target.CanUndo);
            Assert.IsFalse(target.CanRedo);

            // ActionScope
            using (var scope = new UndoRedoList<int, List<int>>.ActionScope(target)) {
                // Modify target in ActionScope
                target.Add(100);
                target.Add(200);
                target.Add(300);
            }

            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(100, target[0]);
            Assert.AreEqual(200, target[1]);
            Assert.AreEqual(300, target[2]);
            Assert.IsTrue(target.CanUndo);

            // Undo
            Assert.IsTrue(target.Undo());
            // The 3 actions in ActionScope can undo in one time.
            Assert.AreEqual(0, target.Count);
            Assert.IsFalse(target.CanUndo);
        }

        [TestMethod]
        public void DisabledUndoScopeTest()
        {
            // list which support undo/redo.
            var target = new UndoRedoList<int, List<int>>();
            Assert.IsFalse(target.CanUndo);
            Assert.IsFalse(target.CanRedo);

            // DisabledUndoScope
            using (var scope = new UndoRedoList<int, List<int>>.DisabledUndoScope(target)) {
                // Modify target in DisabledUndoScope
                target.Add(100);
            }

            // You can't undo actions in DisabledUndoScope.
            Assert.IsFalse(target.CanUndo);
            Assert.IsFalse(target.CanRedo);
        }

        [TestMethod]
        public void UndoRedoListTest()
        {
            // List which support undo/redo.
            var target = new UndoRedoList<int>();

            target.Add(100);

            // You can undo/redo also.
            Assert.IsTrue (target.CanUndo);
            Assert.IsFalse(target.CanRedo);

            Assert.IsTrue(target.Undo());
            Assert.IsTrue(target.Redo());
        }

        [TestMethod]
        public void UndoRedoObservableCollectionTest()
        {
            // ObservableCollection which support undo/redo.
            var observableCollection = new UndoRedoObservableCollection<int>();
            observableCollection.CollectionChanged += ObservableCollection_CollectionChanged;

            observableCollection.Add(100);

            // You can undo/redo also.
            Assert.IsTrue (observableCollection.CanUndo);
            Assert.IsFalse(observableCollection.CanRedo);

            Assert.IsTrue(observableCollection.Undo());
            Assert.IsTrue(observableCollection.Redo());

            // event handler
            static void ObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            { /* do domething */ }
        }
    }
}
