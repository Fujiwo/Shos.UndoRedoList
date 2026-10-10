using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shos.Collections;
using System.Collections.Specialized;

namespace Shos.UndoRedoList.Tests;

[TestClass]
public class UndoRedoObservableCollectionTest
{
    UndoRedoObservableCollection<int>   target                         = null!;
    List<NotifyCollectionChangedAction> notifyCollectionChangedActions = null!;

    [TestInitialize]
    public void Initialize()
    {
        target = new(maximumUndoTimes: 2);
        target.CollectionChanged += OnCollectionChanged;
        notifyCollectionChangedActions = [];
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

    [TestMethod]
    public void CanUndoIsUpdatedInCollectionChanged()
    {
        List<bool> canUndoList = [];
        target.CollectionChanged += (_, _) => canUndoList.Add(target.CanUndo);
        target.Add(100);
        target.Insert(0, 200);
        target[0] = 300;
        target.RemoveAt(0);
        target.Clear();
        CollectionAssert.AreEqual(new[] { true, true, true, true, true }, canUndoList);
    }

    [TestMethod]
    public void CanUndoAndCanRedoAreUpdatedInCollectionChangedWhileUndoRedo()
    {
        target.Add(100);
        List<(bool canUndo, bool canRedo)> states = [];
        target.CollectionChanged += (_, _) => states.Add((target.CanUndo, target.CanRedo));

        Assert.IsTrue(target.Undo());
        Assert.IsTrue(target.Redo());
        CollectionAssert.AreEqual(new[] { (false, true), (true, false) }, states);
    }

    void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => notifyCollectionChangedActions.Add(e.Action);
}
