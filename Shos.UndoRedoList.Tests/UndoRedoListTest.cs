using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shos.Collections;

namespace Shos.UndoRedoList.Tests;

[TestClass]
public class UndoRedoListTest
{
    [TestMethod]
    public void Construct()
    {
        var target = new UndoRedoList<int, List<int>>();
        Assert.AreEqual(0, target.Count);
        Assert.AreEqual(0, target.List.Count);
        Assert.IsFalse(target.CanUndo);
        Assert.IsFalse(target.CanRedo);
    }

    [TestMethod]
    public void Undo()
    {
        var target = new UndoRedoList<int, List<int>>();
        Assert.IsFalse(target.Undo());

        target.Add(100);
        Assert.IsTrue(target.CanUndo);
        Assert.AreEqual(1, target.Count);
        Assert.AreEqual(100, target[0]);

        target.Add(200);
        Assert.IsTrue(target.CanUndo);
        Assert.IsFalse(target.CanRedo);
        Assert.AreEqual(2, target.Count);
        Assert.AreEqual(100, target[0]);
        Assert.AreEqual(200, target[1]);

        Assert.IsTrue(target.Undo());
        Assert.IsTrue(target.CanUndo);
        Assert.AreEqual(1, target.Count);
        Assert.AreEqual(100, target[0]);

        Assert.IsTrue(target.Undo());
        Assert.IsFalse(target.CanUndo);
        Assert.AreEqual(0, target.Count);

        target.Add(300);
        Assert.IsTrue(target.CanUndo);
        target.Add(400);
        target.RemoveAt(0);
        Assert.IsTrue(target.CanUndo);
        Assert.AreEqual(1, target.Count);
        Assert.AreEqual(400, target[0]);

        Assert.IsTrue(target.Undo());
        Assert.IsTrue(target.CanUndo);
        Assert.AreEqual(2, target.Count);
        Assert.AreEqual(300, target[0]);
        Assert.AreEqual(400, target[1]);

        target.Add(500);
        target[target.Count - 1] = 600;
        Assert.AreEqual(3, target.Count);
        Assert.AreEqual(600, target[target.Count - 1]);
        Assert.IsTrue(target.CanUndo);
        Assert.IsTrue(target.Undo());
        Assert.AreEqual(3, target.Count);
        Assert.AreEqual(500, target[target.Count - 1]);

        target.Clear();
        Assert.AreEqual(0, target.Count);
        Assert.IsTrue(target.CanUndo);
        Assert.IsTrue(target.Undo());
        Assert.AreEqual(3, target.Count);
        Assert.AreEqual(300, target[0]);
        Assert.AreEqual(400, target[1]);
        Assert.AreEqual(500, target[2]);
    }

    [TestMethod]
    public void Redo()
    {
        var target = new UndoRedoList<int, List<int>> { 100, 200 };
        Assert.IsFalse(target.CanRedo);
        Assert.IsFalse(target.Redo());

        target.Undo();
        Assert.IsTrue(target.CanRedo);

        Assert.IsTrue(target.Redo());
        Assert.IsFalse(target.CanRedo);
        Assert.AreEqual(2, target.Count);
        Assert.AreEqual(100, target[0]);
        Assert.AreEqual(200, target[1]);

        target.Undo();
        Assert.IsTrue(target.CanRedo);
        target.Add(300);
        Assert.AreEqual(2, target.Count);
        Assert.AreEqual(100, target[0]);
        Assert.AreEqual(300, target[1]);
        Assert.IsFalse(target.CanRedo);

        target.RemoveAt(0);
        target.Undo();
        Assert.IsTrue(target.CanRedo);

        Assert.IsTrue(target.Redo());
        Assert.IsFalse(target.CanRedo);
        Assert.AreEqual(1, target.Count);
        Assert.AreEqual(300, target[0]);

        target.Add(500);
        target[target.Count - 1] = 600;
        Assert.AreEqual(2, target.Count);
        target.Undo();
        Assert.IsTrue(target.CanRedo);
        Assert.IsTrue(target.Redo());
        Assert.AreEqual(2, target.Count);
        Assert.AreEqual(600, target[target.Count - 1]);

        target.Clear();
        target.Undo();
        Assert.IsTrue(target.CanRedo);
        Assert.AreEqual(2, target.Count);
        Assert.AreEqual(300, target[0]);
        Assert.AreEqual(600, target[1]);
        Assert.IsTrue(target.Redo());
        Assert.AreEqual(0, target.Count);
        Assert.IsTrue(target.CanUndo);
        Assert.IsFalse(target.CanRedo);
    }

    [TestMethod]
    public void MaximumUndoTimes()
    {
        // List which support undo/redo.
        // maximumUndoTimes is 2.
        var target = new UndoRedoList<int>(maximumUndoTimes: 2);

        // Add an element 3 times.
        target.Add(100);
        target.Add(200);
        target.Add(300);

        // You can undo 2 times.
        Assert.IsTrue (target.CanUndo);
        Assert.IsFalse(target.CanRedo);
        Assert.IsTrue (target.Undo());

        Assert.IsTrue (target.CanUndo);
        Assert.IsTrue (target.CanRedo);
        Assert.IsTrue (target.Undo());

        Assert.IsFalse(target.CanUndo);
        CollectionAssert.AreEqual(new[] { 100 }, target.ToArray());

        // You can redo 2 times also.
        Assert.IsTrue (target.CanRedo);
        Assert.IsTrue (target.Redo());
        CollectionAssert.AreEqual(new[] { 100, 200 }, target.ToArray());

        Assert.IsTrue (target.CanUndo);
        Assert.IsTrue (target.CanRedo);
        Assert.IsTrue (target.Redo());
        CollectionAssert.AreEqual(new[] { 100, 200, 300 }, target.ToArray());

        Assert.IsTrue (target.CanUndo);
        Assert.IsFalse(target.CanRedo);
    }

    [TestMethod]
    public void AddAfterUndoAll()
    {
        var target = new UndoRedoList<int> { 1, 2 };
        Assert.IsTrue(target.Undo());
        Assert.IsTrue(target.Undo());
        Assert.AreEqual(0, target.Count);

        // Undone actions can't be redone and can't be undone any more.
        target.Add(3);
        Assert.IsFalse(target.CanRedo);
        Assert.IsTrue (target.Undo());
        Assert.AreEqual(0, target.Count);
        Assert.IsFalse(target.CanUndo);

        Assert.IsTrue (target.Redo());
        Assert.IsFalse(target.Redo());
        CollectionAssert.AreEqual(new[] { 3 }, target.ToArray());
    }

    [TestMethod]
    public void AddAfterUndoAllWithWrapAround()
    {
        var target = new UndoRedoList<int>(maximumUndoTimes: 3) { 1, 2, 3, 4 };
        while (target.Undo())
            ;
        CollectionAssert.AreEqual(new[] { 1 }, target.ToArray());

        target.Add(9);
        Assert.IsTrue (target.Undo());
        Assert.IsFalse(target.Undo());
        CollectionAssert.AreEqual(new[] { 1 }, target.ToArray());
    }

    [TestMethod]
    public void FailedOperationIsNotRecorded()
    {
        var target = new UndoRedoList<int> { 1 };
        target.ClearUndo();

        Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => target.Insert(-1, 9));
        Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => target.Insert( 2, 9));
        Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => target.RemoveAt(1));
        Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => target[1] = 9);
        Assert.IsFalse(target.CanUndo);
        CollectionAssert.AreEqual(new[] { 1 }, target.ToArray());
    }

    [TestMethod]
    public void ClearEmptyListIsNotRecorded()
    {
        var target = new UndoRedoList<int>();
        target.Clear();
        Assert.IsFalse(target.CanUndo);
    }

    [TestMethod]
    public void InsertAndRemoveAndExchange()
    {
        var target = new UndoRedoList<int> { 1, 2, 3 };
        target.Insert(1, 10);
        target.Remove(3);
        target[0] = 20;
        CollectionAssert.AreEqual(new[] { 20, 10, 2 }, target.ToArray());

        Assert.IsTrue(target.Undo());
        CollectionAssert.AreEqual(new[] { 1, 10, 2 }, target.ToArray());
        Assert.IsTrue(target.Undo());
        CollectionAssert.AreEqual(new[] { 1, 10, 2, 3 }, target.ToArray());
        Assert.IsTrue(target.Undo());
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, target.ToArray());

        Assert.IsTrue(target.Redo());
        Assert.IsTrue(target.Redo());
        Assert.IsTrue(target.Redo());
        CollectionAssert.AreEqual(new[] { 20, 10, 2 }, target.ToArray());
    }

    [TestMethod]
    public void ActionScope()
    {
        var target = new UndoRedoList<int, List<int>> { 100, 200 };
        using (var scope = new UndoRedoList<int, List<int>>.ActionScope(target)) {
            target.Add(300);
            target.RemoveAt(0);
            target.RemoveAt(1);
            target.Add(400);
            target.Add(500);
        }
        Assert.AreEqual(3, target.Count);
        Assert.AreEqual(200, target[0]);
        Assert.AreEqual(400, target[1]);
        Assert.AreEqual(500, target[2]);
        Assert.IsTrue(target.CanUndo);
        Assert.IsFalse(target.CanRedo);

        Assert.IsTrue(target.Undo());
        Assert.IsTrue(target.CanUndo);
        Assert.IsTrue(target.CanRedo);
        Assert.AreEqual(2, target.Count);
        Assert.AreEqual(100, target[0]);
        Assert.AreEqual(200, target[1]);

        Assert.IsTrue(target.Redo());
        Assert.IsTrue(target.CanUndo);
        Assert.IsFalse(target.CanRedo);
        Assert.AreEqual(3, target.Count);
        Assert.AreEqual(200, target[0]);
        Assert.AreEqual(400, target[1]);
        Assert.AreEqual(500, target[2]);
    }

    [TestMethod]
    public void DisabledUndoScope()
    {
        var target = new UndoRedoList<int, List<int>> { 100 };
        Assert.IsTrue(target.CanUndo);

        using (var scope = new UndoRedoList<int, List<int>>.DisabledUndoScope(target))
            target.Add(200);

        Assert.IsTrue(target.CanUndo);
        Assert.IsTrue(target.Undo());
        Assert.IsFalse(target.CanUndo);
        target.Add(300);
        Assert.IsTrue(target.CanUndo);
    }

    [TestMethod]
    public void ClearUndo()
    {
        var target = new UndoRedoList<int, List<int>> { 100, 200, 300 };
        Assert.IsTrue(target.CanUndo);
        target.ClearUndo();
        Assert.IsFalse(target.CanUndo);
    }

    [TestMethod]
    public void AnotherUndoRedoList()
    {
        // List which support undo/redo.
        var target = new UndoRedoList<int>(maximumUndoTimes: 2);

        target.Add(100);
        target.RemoveAt(0);
        Assert.IsTrue (target.Undo());
        Assert.IsTrue (target.Undo());
        Assert.IsFalse(target.Undo());
        Assert.IsTrue (target.Redo());
        Assert.IsTrue (target.Redo());
        Assert.IsFalse(target.Redo());
    }
}
