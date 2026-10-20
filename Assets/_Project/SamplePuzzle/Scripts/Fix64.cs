using NUnit.Framework.Constraints;

namespace SamplePuzzle
{
    public readonly struct Fix64
    {
        public const int FractionBits = 16;
        public const long OneRaw = 1L << FractionBits;
        public const long HalfRaw = 1L << (FractionBits - 1);
        public static readonly Fix64 One = new(OneRaw);
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
    }
}
