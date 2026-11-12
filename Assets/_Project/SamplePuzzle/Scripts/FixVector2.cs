using System;
using System.Collections.Generic;
using System.Text;

namespace SamplePuzzle
{
    public struct FixVector2 : IEquatable<FixVector2>
    {
        public static FixVector2 Zero => new(0, 0);
        public static FixVector2 Up => new(0, 1);
        public static FixVector2 Down => new(0, -1);
        public static FixVector2 Left => new(-1, 0);
        public static FixVector2 Right => new(1, 0);
        public static FixVector2 One => new(1, 1);
        public Fix64 X;
        public Fix64 Y;

        public FixVector2(Fix64 x, Fix64 y)
        {
            X = x;
            Y = y;
        }

        public FixVector2(double x, double y)
        {
            X = Fix64.FromDouble(x);
            Y = Fix64.FromDouble(y);
        }

        public static bool operator ==(FixVector2 a, FixVector2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(FixVector2 a, FixVector2 b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public bool Equals(FixVector2 other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is FixVector2 vec &&
                Equals(vec);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static FixVector2 operator +(FixVector2 a, FixVector2 b)
        {
            return new(a.X + b.X, a.Y + b.Y);
        }

        public static FixVector2 operator -(FixVector2 a, FixVector2 b)
        {
            return new(a.X - b.X, a.Y - b.Y);
        }

        public static FixVector2 operator -(FixVector2 v)
        {
            return new(-v.X, -v.Y);
        }

        public static FixVector2 operator *(FixVector2 a, Fix64 b)
        {
            return new(a.X * b, a.Y * b);
        }

        public static FixVector2 operator /(FixVector2 a, Fix64 b)
        {
            return new(a.X / b, a.Y / b);
        }

        public override string ToString()
        {
            return "(" + X.ToString() + ", " + Y.ToString() + ")";
        }
    }
}
