using NUnit.Framework;
using SamplePuzzle;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace SamplePuzzle.Tests
{
    public class FixMathTest
    {

        [TestCase(0b11L << 15, 1)]      // 1.5
        [TestCase(-(0b11L << 15), -2)]  // -1.5
        [TestCase(0b1L << 15, 0)]       // 0.5
        [TestCase(-1L, -1)]             // -1/65536
        [TestCase((long)int.MaxValue << 16 | 0xFFFF, int.MaxValue)]
        [TestCase((long)int.MinValue << 16, int.MinValue)]
        public void FloorToInt_ReturnsLargestIntegerNotGreaterThanValue(long raw, int expectedValue)
        {
            var fix = Fix64.FromRaw(raw);

            var result = FixMath.FloorToInt(fix);

            Assert.That(result, Is.EqualTo(expectedValue));
        }


        [TestCase((long)int.MaxValue + 1 << 16)]
        [TestCase((long)int.MinValue - 1 << 16)]
        public void FloorToInt_ExceedsIntRange_Throws(long raw)
        {
            var fix = Fix64.FromRaw(raw);

            Assert.That(() => FixMath.FloorToInt(fix), Throws.TypeOf<OverflowException>());
        }
    }
}