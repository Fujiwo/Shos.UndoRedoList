using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shos.Collections;
using System;

namespace Shos.UndoRedoList.Tests
{
    [TestClass]
    public class ModuloArithmeticTest
    {
        [TestMethod]
        public void Construct()
        {
            var target = new ModuloArithmetic(3);
            Assert.IsTrue(target.IsValid);
            Assert.AreEqual(3, target.Divisor);
            Assert.AreEqual(0, target.Value);
            Assert.AreEqual(new ModuloArithmetic(3), target);
            Assert.IsTrue(new ModuloArithmetic(5) == target);
            Assert.IsTrue(target == 0);
            Assert.IsTrue(0 == target);
            Assert.IsFalse(new ModuloArithmetic(3) != target);
            Assert.IsFalse(target != 0);
            Assert.IsFalse(0 != target);
            Assert.IsFalse(new ModuloArithmetic(6, false).IsValid);
        }

        [TestMethod]
        public void Next()
        {
            Assert.AreEqual(new ModuloArithmetic(3) { Value = 1 }, new ModuloArithmetic(3).Next);
        }

        [TestMethod]
        public void Previous()
        {
            Assert.AreEqual(new ModuloArithmetic(3) { Value = 2 }, new ModuloArithmetic(3).Previous);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InitializeWidthOne()
        {
            var target = new ModuloArithmetic(1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InitializeWidthZero()
        {
            var target = new ModuloArithmetic(0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InitializeWidthMinusValue()
        {
            var target = new ModuloArithmetic(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TooLargeValue()
        {
            var target = new ModuloArithmetic(3) { Value = 3 };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MinusValue()
        {
            var target = new ModuloArithmetic { Value = -1 };
        }

        [TestMethod]
        public void Increment()
        {
            var target = new ModuloArithmetic(3);
            Assert.AreEqual(1, (++target).Value);
            Assert.AreEqual(1, target.Value);
            Assert.AreEqual(1, (target++).Value);
            Assert.AreEqual(2, target.Value);
            target.MoveNext();
            Assert.AreEqual(0, target.Value);
        }

        [TestMethod]
        public void Decrement()
        {
            var target = new ModuloArithmetic(4) { Value = 2 };
            Assert.AreEqual(2, (target--).Value);
            Assert.AreEqual(0, (--target).Value);
            target.MovePrevious();
            Assert.AreEqual(3, target.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidMinus()
        {
            var result = new ModuloArithmetic(3) - new ModuloArithmetic(4);
        }

        [TestMethod]
        public void Minus()
        {
            Assert.AreEqual(0, new ModuloArithmetic(3) { Value = 2 } - new ModuloArithmetic(3) { Value = 2 });
            Assert.AreEqual(1, new ModuloArithmetic(3) { Value = 2 } - new ModuloArithmetic(3) { Value = 1 });
            Assert.AreEqual(2, new ModuloArithmetic(3) { Value = 1 } - new ModuloArithmetic(3) { Value = 2 });
        }
    }
}
