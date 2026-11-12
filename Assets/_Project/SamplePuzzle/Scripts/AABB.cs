using UnityEngine;

namespace SamplePuzzle
{
    public struct AABB
    {
        public FixVector2 Min { get; set; }
        public FixVector2 Max { get; set; }

        public AABB(FixVector2 min, FixVector2 max)
        {
            Min = min;
            Max = max;
        }

        public readonly bool Intersects(AABB other)
        {
            return Min.X < other.Max.X && Max.X > other.Min.X &&
                   Min.Y < other.Max.Y && Max.Y > other.Min.Y;
        }

        public AABB Translate(FixVector2 position)
        {
            return new AABB(Min + position, Max + position);
        }
    }
}
