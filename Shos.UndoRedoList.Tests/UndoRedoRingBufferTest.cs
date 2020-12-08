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
}
