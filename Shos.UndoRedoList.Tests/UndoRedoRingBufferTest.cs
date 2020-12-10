using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shos.Collections;

namespace Shos.UndoRedoList.Tests
{
    [TestClass]
    public class UndoRedoRingBufferTest
    {
        [TestMethod]
        public void Test()
        {
            const int dividor = 3;
            var target = new UndoRedoRingBuffer<int>(dividor);
            Assert.AreEqual(0, target.Count);
            Assert.IsFalse(target.CanGoBackward);
            Assert.IsFalse(target.CanGoForward);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.TopIndex);
            Assert.IsFalse(target.BottomIndex.IsValid);
            Assert.IsFalse(target.CurrentIndex.IsValid);

            target.Add(100);
            Assert.AreEqual(1, target.Count);
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsFalse(target.CanGoForward);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.TopIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.BottomIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.CurrentIndex);

            target.Add(200);
            target.Add(300);
            Assert.AreEqual(3, target.Count);
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsFalse(target.CanGoForward);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.TopIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 2 }, target.BottomIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 2 }, target.CurrentIndex);

            Assert.IsTrue(target.GoBackward());
            Assert.AreEqual(200, target.Current);
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsTrue(target.CanGoForward);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.TopIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 2 }, target.BottomIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 1 }, target.CurrentIndex);

            Assert.IsTrue(target.GoBackward());
            Assert.AreEqual(100, target.Current);
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsTrue(target.CanGoForward);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.TopIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 2 }, target.BottomIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 0 }, target.CurrentIndex);

            Assert.IsTrue(target.GoBackward());
            Assert.IsFalse(target.CanGoBackward);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.TopIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 2 }, target.BottomIndex);
            Assert.IsFalse(target.CurrentIndex.IsValid);

            Assert.IsTrue(target.GoForward());
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsTrue(target.CanGoForward);
            Assert.AreEqual(100, target.Current);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.TopIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 2 }, target.BottomIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 0 }, target.CurrentIndex);

            Assert.IsTrue(target.GoForward());
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsTrue(target.CanGoForward);
            Assert.AreEqual(200, target.Current);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.TopIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 2 }, target.BottomIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 1 }, target.CurrentIndex);

            Assert.IsTrue(target.GoForward());
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsFalse(target.CanGoForward);
            Assert.AreEqual(300, target.Current);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.TopIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 2 }, target.BottomIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 2 }, target.CurrentIndex);

            Assert.IsTrue(target.GoBackward());
            Assert.IsTrue(target.GoBackward());
            Assert.IsTrue(target.GoBackward());
            Assert.AreEqual(new ModuloArithmetic(dividor), target.TopIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 2 }, target.BottomIndex);
            Assert.IsFalse(target.CurrentIndex.IsValid);

            target.Add(500);
            Assert.AreEqual(dividor, target.Count);
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsFalse(target.CanGoForward);
            Assert.AreEqual(500, target.Current);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 1 }, target.TopIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.BottomIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.CurrentIndex);

            target.Add(600);
            target.Add(700);
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsFalse(target.CanGoForward);
            Assert.AreEqual(700, target.Current);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.TopIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 2 }, target.BottomIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 2 }, target.CurrentIndex);

            target.Add(-100);
            Assert.IsTrue(target.CanGoBackward);
            Assert.IsFalse(target.CanGoForward);
            Assert.AreEqual(-100, target.Current);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 1 }, target.TopIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 0 }, target.BottomIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 0 }, target.CurrentIndex);
        }

        [TestMethod]
        public void SmallSize()
        {
            const int dividor = 2;
            var target = new UndoRedoRingBuffer<int>(dividor);
            Assert.AreEqual(0, target.Count);
            Assert.IsFalse(target.CanGoBackward);
            Assert.IsFalse(target.CanGoForward);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.TopIndex);
            Assert.IsFalse(target.BottomIndex.IsValid);
            Assert.IsFalse(target.CurrentIndex.IsValid);

            target.Add(100);
            Assert.AreEqual(1, target.Count);
            Assert.IsTrue (target.CanGoBackward);
            Assert.IsFalse(target.CanGoForward );
            Assert.AreEqual(new ModuloArithmetic(dividor), target.TopIndex    );
            Assert.AreEqual(new ModuloArithmetic(dividor), target.BottomIndex );
            Assert.AreEqual(new ModuloArithmetic(dividor), target.CurrentIndex);

            target.Add(200);
            target.Add(300);
            Assert.AreEqual(2, target.Count);
            Assert.IsTrue (target.CanGoBackward);
            Assert.IsFalse(target.CanGoForward );
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 1 }, target.TopIndex    );
            Assert.AreEqual(new ModuloArithmetic(dividor)              , target.BottomIndex );
            Assert.AreEqual(new ModuloArithmetic(dividor)              , target.CurrentIndex);
        }
    }
}
