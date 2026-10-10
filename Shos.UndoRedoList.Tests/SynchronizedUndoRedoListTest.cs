using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shos.Collections;

namespace Shos.UndoRedoList.Tests;

[TestClass]
public class SynchronizedUndoRedoListTest
{
    const int threadCount    = 8;
    const int countPerThread = 1000;
    const int total          = threadCount * countPerThread;

    [TestMethod]
    public void Construct()
    {
        var target = new SynchronizedUndoRedoList<int>();
        Assert.AreEqual(0, target.Count);
        Assert.IsFalse(target.CanUndo);
        Assert.IsFalse(target.CanRedo);
        Assert.IsTrue(target.UndoEnabled);
        Assert.IsNotNull(target.SyncRoot);
    }

    [TestMethod]
    public void ConstructWithInvalidSize()
        => Assert.ThrowsException<ArgumentOutOfRangeException>(() => new SynchronizedUndoRedoList<int>(1));

    [TestMethod]
    public void UndoRedo()
    {
        var target = new SynchronizedUndoRedoList<int>();
        Assert.IsFalse(target.Undo());

        target.Add(100);
        target.Add(200);
        target.Insert(0, 50);
        target[1] = 150;
        target.RemoveAt(2);
        Assert.IsTrue(new[] { 50, 150 }.SequenceEqual(target));

        Assert.IsTrue(target.Undo());
        Assert.IsTrue(new[] { 50, 150, 200 }.SequenceEqual(target));
        Assert.IsTrue(target.Undo());
        Assert.IsTrue(new[] { 50, 100, 200 }.SequenceEqual(target));
        Assert.IsTrue(target.CanRedo);
        Assert.IsTrue(target.Redo());
        Assert.IsTrue(new[] { 50, 150, 200 }.SequenceEqual(target));

        Assert.IsTrue(target.Remove(150));
        Assert.IsFalse(target.Remove(999));
        Assert.IsTrue(target.Contains(50));
        Assert.AreEqual(1, target.IndexOf(200));

        target.Clear();
        Assert.AreEqual(0, target.Count);
        Assert.IsTrue(target.Undo());
        Assert.IsTrue(new[] { 50, 200 }.SequenceEqual(target));

        target.ClearUndo();
        Assert.IsFalse(target.CanUndo);
        Assert.IsFalse(target.CanRedo);
    }

    [TestMethod]
    public void ExecuteAsOneAction()
    {
        var target = new SynchronizedUndoRedoList<int> { 1 };
        target.ExecuteAsOneAction(list => {
            list.Add(2);
            list.Add(3);
            list.RemoveAt(0);
        });
        Assert.IsTrue(new[] { 2, 3 }.SequenceEqual(target));

        Assert.IsTrue(target.Undo());
        Assert.IsTrue(new[] { 1 }.SequenceEqual(target));
        Assert.IsTrue(target.Redo());
        Assert.IsTrue(new[] { 2, 3 }.SequenceEqual(target));

        var removed = target.ExecuteAsOneAction(list => {
            if (list.Count == 0)
                return false;
            list.RemoveAt(0);
            return true;
        });
        Assert.IsTrue(removed);
        Assert.IsTrue(new[] { 3 }.SequenceEqual(target));
    }

    [TestMethod]
    public void ExecuteAsOneActionWithException()
    {
        var target = new SynchronizedUndoRedoList<int>();
        Assert.ThrowsException<InvalidOperationException>(() => target.ExecuteAsOneAction(list => {
            list.Add(1);
            list.Add(2);
            throw new InvalidOperationException();
        }));
        Assert.AreEqual(2, target.Count);

        // The changes before the exception are recorded as one undo step, and the scope is closed.
        Assert.IsTrue(target.Undo());
        Assert.AreEqual(0, target.Count);
        target.ExecuteAsOneAction(list => list.Add(3));
        Assert.IsTrue(new[] { 3 }.SequenceEqual(target));
    }

    [TestMethod]
    public void ExecuteWithoutUndo()
    {
        var target = new SynchronizedUndoRedoList<int>();
        target.ExecuteWithoutUndo(list => {
            list.Add(1);
            list.Add(2);
        });
        Assert.AreEqual(2, target.Count);
        Assert.IsFalse(target.CanUndo);
        Assert.IsTrue(target.UndoEnabled);

        target.Add(3);
        Assert.IsTrue(target.CanUndo);
    }

    [TestMethod]
    public void NullArguments()
    {
        var target = new SynchronizedUndoRedoList<int>();
        Assert.ThrowsException<ArgumentNullException>(() => target.ExecuteAsOneAction((Action<IList<int>>)null!));
        Assert.ThrowsException<ArgumentNullException>(() => target.ExecuteAsOneAction((Func<IList<int>, int>)null!));
        Assert.ThrowsException<ArgumentNullException>(() => target.ExecuteWithoutUndo(null!));
    }

    [TestMethod]
    public void EnumerateSnapshot()
    {
        var target = new SynchronizedUndoRedoList<int> { 1, 2, 3 };
        List<int> result = [];
        foreach (var element in target) {
            // Changing the list while enumerating doesn't throw because the enumerator uses a snapshot.
            target.Add(element * 10);
            result.Add(element);
        }
        Assert.IsTrue(new[] { 1, 2, 3 }.SequenceEqual(result));
        Assert.AreEqual(6, target.Count);
        Assert.IsTrue(new[] { 1, 2, 3, 10, 20, 30 }.SequenceEqual(target.ToArray()));
    }

    [TestMethod]
    public void ParallelAddAndUndo()
    {
        var target = new SynchronizedUndoRedoList<int>(total + 1);
        Parallel.For(0, total, index => target.Add(index));
        Assert.AreEqual(total, target.Count);
        Assert.IsTrue(Enumerable.Range(0, total).SequenceEqual(target.OrderBy(element => element)));

        Parallel.For(0, total, _ => Assert.IsTrue(target.Undo()));
        Assert.AreEqual(0, target.Count);
        Assert.IsFalse(target.CanUndo);
        Assert.IsFalse(target.Undo());

        Parallel.For(0, total, _ => Assert.IsTrue(target.Redo()));
        Assert.AreEqual(total, target.Count);
        Assert.IsFalse(target.CanRedo);
    }

    [TestMethod]
    public void ParallelExecuteAsOneAction()
    {
        var target = new SynchronizedUndoRedoList<int>(total + 1);
        Parallel.For(0, total, index => target.ExecuteAsOneAction(list => {
            list.Add(index);
            list.Add(-index - 1);
        }));
        Assert.AreEqual(total * 2, target.Count);

        // Each scope must be recorded as one undo step and its two elements must be adjacent.
        var elements = target.ToArray();
        for (var index = 0; index < elements.Length; index += 2)
            Assert.AreEqual(-elements[index] - 1, elements[index + 1]);

        for (var count = 0; count < total; count++)
            Assert.IsTrue(target.Undo());
        Assert.AreEqual(0, target.Count);
        Assert.IsFalse(target.CanUndo);
    }

    [TestMethod]
    public void ParallelMixedOperations()
    {
        var target = new SynchronizedUndoRedoList<int>(total * 2 + 1);
        var tasks  = Enumerable.Range(0, threadCount).Select(threadIndex => Task.Run(() => {
            var random = new Random(threadIndex);
            for (var count = 0; count < countPerThread; count++) {
                switch (random.Next(7)) {
                case 0: target.Add(count); break;
                case 1: target.Insert(0, count); break;
                case 2: target.Undo(); break;
                case 3: target.Redo(); break;
                case 4: _ = target.Sum(); break;
                case 5: target.ExecuteAsOneAction(list => { if (list.Count > 0) list.RemoveAt(list.Count - 1); }); break;
                case 6: target.ExecuteAsOneAction(list => { if (list.Count > 0) list[0] = -count; }); break;
                }
            }
        })).ToArray();
        Task.WaitAll(tasks);

        // The history must be consistent: undoing everything makes the list empty.
        while (target.Undo())
            ;
        Assert.AreEqual(0, target.Count);
    }

    [TestMethod]
    public void SyncRootLocksOtherThreads()
    {
        var target = new SynchronizedUndoRedoList<int>();
        Task addTask;
        lock (target.SyncRoot) {
            addTask = Task.Run(() => target.Add(1));
            Assert.IsFalse(addTask.Wait(100));
            Assert.AreEqual(0, target.Count);
        }
        Assert.IsTrue(addTask.Wait(TimeSpan.FromSeconds(10)));
        Assert.AreEqual(1, target.Count);
    }

    [TestMethod]
    public void UndoRedoListIsNotThreadSafe()
    {
        // Reference: the same parallel operations break UndoRedoList (not always, so try several times).
        var broken = false;
        for (var trial = 0; trial < 20 && !broken; trial++) {
            var target = new UndoRedoList<int>(total + 1);
            try {
                Parallel.For(0, total, index => target.Add(index));
                broken = target.Count != total;
            } catch (AggregateException) {
                broken = true;
            }
        }
        if (!broken)
            Assert.Inconclusive("The race condition didn't occur in this environment.");
    }
}
