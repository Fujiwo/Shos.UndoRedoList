using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shos.Collections;
using System;

namespace Shos.UndoRedoList.Tests
{
    [TestClass]
    public class RingBufferTest
    {
        [TestMethod]
        public void Construct()
        {
            var target = new RingBuffer<int>();
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        public void Add()
        {
            const int dividor = 3;
            var target = new RingBuffer<int>(dividor) { 100 };
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(100, target[new ModuloArithmetic(dividor)]);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.TopIndex   );
            Assert.AreEqual(new ModuloArithmetic(dividor), target.BottomIndex);

            target.Add(300);
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual(100, target[new ModuloArithmetic(dividor)]);
            Assert.AreEqual(300, target[new ModuloArithmetic(dividor) { Value = 1 }]);
            Assert.AreEqual(new ModuloArithmetic(dividor)              , target.TopIndex   );
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 1 }, target.BottomIndex);

            target.Add(200);
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(200, target[new ModuloArithmetic(dividor) { Value = 2 }]);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.TopIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 2 }, target.BottomIndex);

            target.Add(-100);
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(200, target[new ModuloArithmetic(dividor) { Value = 2 }]);
            Assert.AreEqual(-100, target[new ModuloArithmetic(dividor) { Value = 0 }]);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 1 }, target.TopIndex   );
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 0 }, target.BottomIndex);

            target.Add(-300);
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(-100, target[new ModuloArithmetic(dividor) { Value = 0 }]);
            Assert.AreEqual(-300, target[new ModuloArithmetic(dividor) { Value = 1 }]);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 2 }, target.TopIndex   );
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 1 }, target.BottomIndex);
        }

        [TestMethod]
        public void SmallSize()
        {
            const int dividor = 2;
            var target = new RingBuffer<int>(dividor) { 100 };
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(100, target[new ModuloArithmetic(dividor)]);
            Assert.AreEqual(new ModuloArithmetic(dividor), target.TopIndex   );
            Assert.AreEqual(new ModuloArithmetic(dividor), target.BottomIndex);

            target.Add(300);
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual(100, target[new ModuloArithmetic(dividor)]);
            Assert.AreEqual(300, target[new ModuloArithmetic(dividor) { Value = 1 }]);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 0 }, target.TopIndex   );
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 1 }, target.BottomIndex);

            target.Add(200);
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual(200, target[new ModuloArithmetic(dividor) { Value = 0 }]);
            Assert.AreEqual(300, target[new ModuloArithmetic(dividor) { Value = 1 }]);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 1 }, target.TopIndex   );
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 0 }, target.BottomIndex);

            target.Add(-100);
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual( 200, target[new ModuloArithmetic(dividor) { Value = 0 }]);
            Assert.AreEqual(-100, target[new ModuloArithmetic(dividor) { Value = 1 }]);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 0 }, target.TopIndex);
            Assert.AreEqual(new ModuloArithmetic(dividor) { Value = 1 }, target.BottomIndex);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InitializeWidthOne()
        {
            var target = new RingBuffer<int>(1);
        }

        [TestMethod]
        public void ForLoop()
        {
            var target = new RingBuffer<int>(3) { 100, 300, 200, 400 };

            //var count = target.Count;
            var index = target.TopIndex;
            for (var count = target.Count; count >= 0; index++, count--) {
                switch (index) {
                    case ModuloArithmetic i when i == target.TopIndex:
                        Assert.AreEqual(1, i.Value);
                        Assert.AreEqual(300, target[i]);
                        break;
                    case ModuloArithmetic i when i == target.TopIndex.Next:
                        Assert.AreEqual(2, i.Value);
                        Assert.AreEqual(200, target[i]);
                        break;
                    case ModuloArithmetic i when i == target.TopIndex.Next.Next:
                        Assert.AreEqual(0, i.Value);
                        Assert.AreEqual(400, target[i]);
                        break;
                    default:
                        Assert.Fail();
                        break;
                }
            }
        }

        [TestMethod]
        public void ForEachLoop()
        {
            var target = new RingBuffer<int>(3) { 100, 300, 200, 400, 500 };
            var index = target.TopIndex;
            foreach (var element in target) {
                switch (index) {
                    case ModuloArithmetic i when i == target.TopIndex:
                        Assert.AreEqual(2, i.Value);
                        Assert.AreEqual(200, target[i]);
                        break;
                    case ModuloArithmetic i when i == target.TopIndex.Next:
                        Assert.AreEqual(0, i.Value);
                        Assert.AreEqual(400, target[i]);
                        break;
                    case ModuloArithmetic i when i == target.TopIndex.Next.Next:
                        Assert.AreEqual(1, i.Value);
                        Assert.AreEqual(500, target[i]);
                        break;
                    default:
                        Assert.Fail();
                        break;
                }
                index++;
            }
        }

        [TestMethod]
        public void Clear()
        {
            var target = new RingBuffer<int>(3) { -100, 300, -200, 400 };
            target.Clear();
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        public void Remove()
        {
            var target = new RingBuffer<int>(3) { -100, 300, -200, 400 };
            Assert.IsTrue(target.Remove());
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual(300, target[target.TopIndex]);
            Assert.AreEqual(-200, target[target.TopIndex.Next]);
            Assert.IsTrue(target.Remove());
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(300, target[target.TopIndex]);
            Assert.IsTrue(target.Remove());
            Assert.AreEqual(0, target.Count);
            Assert.IsFalse(target.Remove());
        }

        [TestMethod]
        public void RemoveAfter()
        {
            var target = new RingBuffer<int> { 10, 20, 30, 40 };
            target.RemoveAfter(target.BottomIndex);
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(10, target[target.TopIndex]);
            Assert.AreEqual(20, target[target.TopIndex.Next]);
            Assert.AreEqual(30, target[target.TopIndex.Next.Next]);
            target.RemoveAfter(target.BottomIndex.Previous);
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(10, target[target.TopIndex]);
            target.Add(100);
            target.Add(200);
            target.RemoveAfter(target.TopIndex);
            Assert.AreEqual(0, target.Count);
        }
    }
}
