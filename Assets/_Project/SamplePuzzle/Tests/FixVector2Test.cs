using NUnit.Framework;
using SamplePuzzle;

namespace SamplePuzzle.Tests
{
    public class FixVector2Test
    {
        [Test]
        public void Constructor_AssignsXandY()
        {
            var x = Fix64.FromInt(3);
            var y = Fix64.FromInt(7);

            FixVector2 actual = new(x: x, y: y);

            Assert.That(actual.X, Is.EqualTo(x));
            Assert.That(actual.Y, Is.EqualTo(y));
        }

        [TestCase(3, 5, 3, 5, true)]
        [TestCase(3, 5, 4, 5, false)]
        [TestCase(3, 5, 3, 6, false)]
        [TestCase(3, 5, 2, 4, false)]
        public void Equality_ComparesComponentwise(int aX, int aY, int bX, int bY, bool expected)
        {
            var a = V(aX, aY);
            var b = V(bX, bY);

            Assert.That(a == b, Is.EqualTo(expected));
            Assert.That(a != b, Is.EqualTo(!expected));
            Assert.That(a.Equals(b), Is.EqualTo(expected));
            Assert.That(a.Equals((object)b), Is.EqualTo(expected));
        }

        [TestCase(1, 2, 3, 4, 4, 6)]
        [TestCase(1, 2, -3, -4, -2, -2)]
        [TestCase(1, 1, 0, 0, 1, 1)]
        [TestCase(-1, -2, -3, -4, -4, -6)]
        public void Addition_AddsComponentwise(int aX, int aY, int bX, int bY, int expectedX, int expectedY)
        {
            var a = V(aX, aY);
            var b = V(bX, bY);
            var expected = V(expectedX, expectedY);

            var actual = a + b;

            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(1, 2, 3, 4, -2, -2)]
        [TestCase(1, 2, -3, -4, 4, 6)]
        [TestCase(1, 1, 0, 0, 1, 1)]
        [TestCase(-1, -2, -3, -4, 2, 2)]
        public void Subtraction_SubtractsComponentwise(int aX, int aY, int bX, int bY, int expectedX, int expectedY)
        {
            var a = V(aX, aY);
            var b = V(bX, bY);
            var expected = V(expectedX, expectedY);

            var actual = a - b;

            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(1, 1, -1, -1)]
        [TestCase(0, 0, 0, 0)]
        [TestCase(5, -5, -5, 5)]
        [TestCase(-3, -3, 3, 3)]
        public void Negation_FlipsSign(int x, int y, int expectedX, int expectedY)
        {
            var v = V(x, y);
            var expected = V(expectedX, expectedY);

            var actual = -v;

            Assert.That(actual, Is.EqualTo(expected));
        }

        private static FixVector2 V(int x, int y) => new(Fix64.FromInt(x), Fix64.FromInt(y));
    }

}