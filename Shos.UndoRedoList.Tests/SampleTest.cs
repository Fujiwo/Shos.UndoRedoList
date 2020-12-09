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
            var list = new UndoRedoList<int, List<int>>();

            Assert.IsFalse(list.CanUndo);
            Assert.IsFalse(list.CanRedo);

            // Modify list
            list.Add(100);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(100, list[0]);
            Assert.IsTrue (list.CanUndo);
            Assert.IsFalse(list.CanRedo);

            // Undo
            Assert.IsTrue(list.Undo());

            Assert.AreEqual(0, list.Count);
            Assert.IsFalse(list.CanUndo);
            Assert.IsTrue (list.CanRedo);

            // Redo
            Assert.IsTrue(list.Redo());

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(100, list[0]);
            Assert.IsTrue (list.CanUndo);
            Assert.IsFalse(list.CanRedo);
        }

        [TestMethod]
        public void ActionScopeTest()
        {
            // list which support undo/redo.
            var list = new UndoRedoList<int, List<int>>();
            Assert.IsFalse(list.CanUndo);
            Assert.IsFalse(list.CanRedo);

            // ActionScope
            using (var scope = new UndoRedoList<int, List<int>>.ActionScope(list)) {
                // Modify list in ActionScope
                list.Add(100);
                list.Add(200);
                list.Add(300);
            }

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(100, list[0]);
            Assert.AreEqual(200, list[1]);
            Assert.AreEqual(300, list[2]);
            Assert.IsTrue(list.CanUndo);

            // Undo
            Assert.IsTrue(list.Undo());
            // The 3 actions in ActionScope can undo in one time.
            Assert.AreEqual(0, list.Count);
            Assert.IsFalse(list.CanUndo);
        }

        [TestMethod]
        public void DisabledUndoScopeTest()
        {
            // list which support undo/redo.
            var list = new UndoRedoList<int, List<int>>();
            Assert.IsFalse(list.CanUndo);
            Assert.IsFalse(list.CanRedo);

            // DisabledUndoScope
            using (var scope = new UndoRedoList<int, List<int>>.DisabledUndoScope(list)) {
                // Modify list in DisabledUndoScope
                list.Add(100);
            }

            // You can't undo actions in DisabledUndoScope.
            Assert.IsFalse(list.CanUndo);
            Assert.IsFalse(list.CanRedo);
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
