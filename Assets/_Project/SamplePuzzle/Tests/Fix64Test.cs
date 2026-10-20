using NUnit.Framework;
using SamplePuzzle;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using static TestUtils;

public class Fix64Test
{
    [Test]
    public void FromRaw_PreservesRawValue()
    {
        var fix = Fix64.FromRaw(1);

        Assert.That(fix.Raw, Is.EqualTo(1));
    }

    [TestCase(0, 0L)]
    [TestCase(1, 65536L)]
    [TestCase(-1, -65536L)]
    [TestCase(int.MaxValue, 140737488289792L)]
    [TestCase(int.MinValue, -140737488355328L)]
    public void FromInt_RepresentsSameIntegerValue(int value, long expectedRaw)
    {
        var fix = Fix64.FromInt(value);

        Assert.That(fix.Raw, Is.EqualTo(expectedRaw));
    }

    [TestCase(0b11L << 15, 0b1L << 14, 0b111 << 14)] // 1.5 + 0.25 = 1.75
    public void Addition_ReturnsSum(long aRaw, long bRaw, long expectedRaw)
    {
        Fix64 actual = Fix64.FromRaw(aRaw) + Fix64.FromRaw(bRaw);

        Assert.That(actual.Raw, Is.EqualTo(expectedRaw));
    }

    [TestCase(long.MaxValue, 1L)]
    [TestCase(long.MinValue, -1L)]
    public void Addition_Overflow_Throws(long aRaw, long bRaw)
    {
        Assert.That(() => {
            var result = Fix64.FromRaw(aRaw) + Fix64.FromRaw(bRaw);
        }, Throws.TypeOf<OverflowException>());
    }

    [TestCase(0b11L << 15, 0b1L << 14, 0b101 << 14)] // 1.5 - 0.25 = 1.25
    [TestCase(long.MinValue, long.MinValue, 0)]
    [TestCase(long.MaxValue, long.MaxValue, 0)]
    public void Subtraction_ReturnsSub(long aRaw, long bRaw, long expectedRaw)
    {
        Fix64 actual = Fix64.FromRaw(aRaw) - Fix64.FromRaw(bRaw);

        Assert.That(actual.Raw, Is.EqualTo(expectedRaw));
    }

    [TestCase(long.MaxValue, -1L)]
    [TestCase(long.MinValue, 1L)]
    public void Subtraction_Overflow_Throws(long aRaw, long bRaw)
    {
        Assert.That(() => {
            var result = Fix64.FromRaw(aRaw) - Fix64.FromRaw(bRaw);
        }, Throws.TypeOf<OverflowException>());
    }

    [TestCase(0b11L << 15, -(0b11L << 15))] // 1.5 -> -1.5
    [TestCase(0, 0)] // 0 -> 0
    [TestCase(-(0b11L << 15), (0b11L << 15))] // -1.5 -> 1.5
    [TestCase(1L, -1L)] 
    public void Negation_FlipsSign(long raw, long expectedRaw)
    {
        Fix64 actual = -Fix64.FromRaw(raw);

        Assert.That(actual.Raw, Is.EqualTo(expectedRaw));
    }

    [Test]
    public void Negation_MinValue_Throws()
    {
        Fix64 min = Fix64.FromRaw(long.MinValue);

        Assert.That(() => -min, Throws.TypeOf<OverflowException>());
    }

    [TestCase(4L << 16, 1L << 15, 2L << 16)] // 4 * 0.5 = 2
    [TestCase(46340L << 16, 46340L << 16, (46340L * 46340L) << 16)] // 경계값
    [TestCase(18L << 16, 0b11 << 15, 27L << 16)] // 18 * 1.5 = 27
    [TestCase(3L << 16, 1L << 15, 0B11 << 15)] // 3 * 0.5 = 1.5
    public void Multiplication_ReturnsMul(long aRaw, long bRaw, long expectedRaw)
    {
        Fix64 actual = Fix64.FromRaw(aRaw) * Fix64.FromRaw(bRaw);

        Assert.That(actual.Raw, Is.EqualTo(expectedRaw));
    }

    [TestCase(1L, 0b1L << 14, 0L)] // 2^-16 * 2^-2 = 2^-18 -> 0
    [TestCase(3L, 0b1L << 15, 2L)] // (3 * 2^-16) * 2^-1 = 1.5 * 2^-16 -> 2^-16에 대해 1.5 칸 -> 반올림해서 2^-16 * 2
    [TestCase(-3L, 0b1L << 15, -1L)] // -1.5칸 -> 반올림하면 -1칸
    public void Multiplication_RoundsHalfTowardPositiveInfinity(long aRaw, long bRaw, long expectedRaw)
    {
        Fix64 actual = Fix64.FromRaw(aRaw) * Fix64.FromRaw(bRaw);

        Assert.That(actual.Raw, Is.EqualTo(expectedRaw));
    }

    [Test]
    public void Multiplication_ResultExceedsIntermediateLimit_Throws()
    {
        var x = Fix64.FromInt(46341);

        Assert.That(() => x = x * x, Throws.TypeOf<OverflowException>());
    }

    [TestCase(3L << 16, 2L << 16, 0b11L << 15)] // 3 / 2 = 1.5
    [TestCase(4L << 16, 2L << 16, 2L << 16)] // 4 / 2 = 2
    [TestCase(100L << 16, 100L << 16, 1L << 16)] // 100 / 100 = 1
    public void Division_ReturnsDiv(long aRaw, long bRaw, long expectedRaw)
    {
        Fix64 actual = Fix64.FromRaw(aRaw) / Fix64.FromRaw(bRaw);

        Assert.That(actual.Raw, Is.EqualTo(expectedRaw));
    }

    [TestCase(3L, -(2L << 16), -1L)]     // 양 / 음, -1.5칸 → -1
    [TestCase(-3L, -(2L << 16), 2L)]     // 음 / 음, 1.5칸 → 2
    [TestCase(3L, 4L << 16, 1L)]         // 0.75칸 → 1
    [TestCase(-3L, 4L << 16, -1L)]       // -0.75칸 → -1
    [TestCase(-1L, 4L << 16, 0L)]        // -0.25칸 → 0
    public void Division_RoundsHalfTowardPositiveInfinity(long aRaw, long bRaw, long expectedRaw)
    {
        Fix64 actual = Fix64.FromRaw(aRaw) / Fix64.FromRaw(bRaw);

        Assert.That(actual.Raw, Is.EqualTo(expectedRaw));
    }

    [Test]
    public void Division_NegationOverflow_Throws()
    {
        Assert.That(() => 
            Fix64.FromInt(int.MinValue) / Fix64.FromInt(-1),
            Throws.TypeOf<OverflowException>());
    }

    [Test]
    public void Division_DivisorIsMinValue_Throws()
    {
        Assert.That(() => 
            Fix64.FromInt(int.MaxValue) / Fix64.FromRaw(long.MinValue), 
            Throws.TypeOf<OverflowException>());
    }

    [Test]
    public void Division_DividendShiftOverflow_Throws()
    {
        var dividend = Fix64.FromInt(int.MaxValue) + Fix64.One;

        Assert.That(() => dividend / Fix64.One, Throws.TypeOf<OverflowException>());
    }

    [Test]
    public void Golden_RandomOperations_MatchRecordedHash()
    {
        var rng = new XorShift64(12345);
        ulong hash = 14695981039346656037UL;

        for (int i = 0; i < 10000; i++)
        {
            var a = Fix64.FromRaw(NextRaw(ref rng));
            var b = Fix64.FromRaw(NextRaw(ref rng));

            hash = Mix(hash, (a + b).Raw);
            hash = Mix(hash, (a - b).Raw);
            hash = Mix(hash, (-a).Raw);
            hash = Mix(hash, (a * b).Raw);
            if (b.Raw != 0) hash = Mix(hash, (a / b).Raw);
            hash = Mix(hash, FixMath.FloorToInt(a));
        }

        Assert.That(hash, Is.EqualTo(4846482198651724452UL)); // 처음엔 0으로 두고 실행
    }

    private static long NextRaw(ref XorShift64 rng)
    {
        // 실제 값 ±32768: 곱셈 결과가 2^30 이하라 중간값 오버플로가 없음
        return (int)(rng.Next() >> 32);
    }
}
