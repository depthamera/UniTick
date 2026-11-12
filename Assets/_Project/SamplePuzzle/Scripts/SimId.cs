using System;

namespace SamplePuzzle
{
    public readonly struct SimId : IEquatable<SimId>
    {
        public int Value { get; }
        public SimId(int value)
        {
            Value = value;
        }

        public static bool operator ==(SimId left, SimId right) => left.Equals(right);
        public static bool operator !=(SimId left, SimId right) => !left.Equals(right);

        public bool Equals(SimId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is SimId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
