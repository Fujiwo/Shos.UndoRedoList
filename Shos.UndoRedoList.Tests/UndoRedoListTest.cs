using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shos.Collections;
using System.Collections.Generic;

namespace Shos.UndoRedoList.Tests
{
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

            // You can redo 2 times also.
            Assert.IsFalse(target.CanUndo);
            Assert.IsTrue (target.CanRedo);
            Assert.IsTrue (target.Redo());

            //Assert.IsTrue (target.CanUndo);
            //Assert.IsTrue (target.CanRedo);
            //Assert.IsTrue (target.Redo());

            //Assert.IsTrue (target.CanUndo);
            //Assert.IsFalse(target.CanRedo);
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
}
