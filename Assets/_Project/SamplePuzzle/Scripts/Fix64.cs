using System;

namespace SamplePuzzle
{
    public readonly struct Fix64 : IEquatable<Fix64>, IComparable<Fix64>, IComparable
    {
        public const int FractionBits = 16;
        public const long OneRaw = 1L << FractionBits;
        public const long HalfRaw = 1L << (FractionBits - 1);
        public static readonly Fix64 One = new(OneRaw);
        public static readonly Fix64 Zero = new(0);
        public readonly long Raw;

        private Fix64(long raw) => Raw = raw;


        public static Fix64 FromRaw(long raw) => new(raw);
        public static Fix64 FromInt(int value) => new((long)value << FractionBits);


        public static Fix64 operator +(Fix64 a, Fix64 b)
        {
            return new Fix64(checked(a.Raw + b.Raw));
        }

        public static Fix64 operator -(Fix64 a, Fix64 b)
        {
            return new Fix64(checked(a.Raw - b.Raw));
        }

        public static Fix64 operator -(Fix64 x)
        {
            return new Fix64(checked(-x.Raw));
        }

        public static Fix64 operator *(Fix64 a, Fix64 b)
        {
            return new Fix64(checked(a.Raw * b.Raw) + HalfRaw >> FractionBits);
        }

        public static Fix64 operator /(Fix64 a, Fix64 b)
        {
            long n = checked(a.Raw * OneRaw);
            long d = b.Raw;

            if(d < 0)
            {
                n = checked(-n);
                d = checked(-d);
            }

            long q = n / d;
            long r = n % d;

            if (r > 0 && r >= d - r) q++;
            if (r < 0 && -r > d + r) q--;

            return new Fix64(q);
        }

        public static bool operator ==(Fix64 a, Fix64 b)
        {
            return a.Raw == b.Raw;
        }

        public static bool operator !=(Fix64 a, Fix64 b)
        {
            return a.Raw != b.Raw;
        }

        public bool Equals(Fix64 other)
        {
            return Raw == other.Raw;
        }

        public override bool Equals(object obj)
        {
            return obj is Fix64 fix &&
                   Equals(fix);
        }

        public static bool operator <(Fix64 a, Fix64 b)
        {
            return a.Raw < b.Raw;
        }

        public static bool operator >(Fix64 a, Fix64 b)
        {
            return a.Raw > b.Raw;
        }

        public static bool operator <=(Fix64 a, Fix64 b)
        {
            return a.Raw <= b.Raw;
        }

        public static bool operator >=(Fix64 a, Fix64 b)
        {
            return a.Raw >= b.Raw;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Raw);
        }

        public float ToFloat()
        {
            return (float)Raw / OneRaw;
        }

        public double ToDouble()
        {
            return (double)Raw / OneRaw;
        }

        public override string ToString()
        {
            return ToDouble().ToString();
        }

        public static Fix64 FromDouble(double value)
        {
            double scaled = value * OneRaw;
            double fl = Math.Floor(scaled);

            double frac = scaled - fl;
            long raw = (long)fl;

            if(frac >= 0.5)
            {
                raw = raw + 1;
            }

            return FromRaw(raw);
        }

        public int CompareTo(Fix64 other)
        {
            return Raw.CompareTo(other.Raw);
        }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;
            if (obj is Fix64 other) return CompareTo(other);
            throw new ArgumentException("Fix64가 아님");
        }
    }
}
