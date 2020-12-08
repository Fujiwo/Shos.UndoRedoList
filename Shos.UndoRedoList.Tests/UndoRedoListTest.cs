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
        public void ActionScope()
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

        [TestMethod]
        public void DisabledUndoScope()
        {
            var list = new UndoRedoList<int, List<int>> { 100 };
            Assert.IsTrue(list.CanUndo);

            using (var scope = new UndoRedoList<int, List<int>>.DisabledUndoScope(list))
                list.Add(200);

            Assert.IsTrue(list.CanUndo);
            Assert.IsTrue(list.Undo());
            Assert.IsFalse(list.CanUndo);
            list.Add(300);
            Assert.IsTrue(list.CanUndo);
        }

        [TestMethod]
        public void ClearUndo()
        {
            var list = new UndoRedoList<int, List<int>> { 100, 200, 300 };
            Assert.IsTrue(list.CanUndo);
            list.ClearUndo();
            Assert.IsFalse(list.CanUndo);
        }
    }
}
