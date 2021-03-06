using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shos.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Shos.UndoRedoList.Tests
{
    [TestClass]
    public class UndoRedoObservableCollectionTest
    {
        UndoRedoObservableCollection<int>   target;
        List<NotifyCollectionChangedAction> notifyCollectionChangedActions;

        [TestInitialize]
        public void Initialize()
        {
            target = new UndoRedoObservableCollection<int>(maximumUndoTimes: 2);
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
            Assert.IsTrue(target.Undo());
            Assert.AreEqual(3, notifyCollectionChangedActions.Count);
            Assert.IsTrue(target.Undo());
            Assert.AreEqual(4, notifyCollectionChangedActions.Count);
            Assert.IsFalse(target.Undo());
            Assert.AreEqual(4, notifyCollectionChangedActions.Count);
            Assert.IsTrue(target.Redo());
            Assert.AreEqual(5, notifyCollectionChangedActions.Count);
            Assert.IsTrue(target.Redo());
            Assert.AreEqual(6, notifyCollectionChangedActions.Count);
            Assert.IsFalse(target.Redo());
            Assert.AreEqual(6, notifyCollectionChangedActions.Count);
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => notifyCollectionChangedActions.Add(e.Action);
    }
}
