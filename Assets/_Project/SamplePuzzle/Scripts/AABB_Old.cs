using UnityEngine;

namespace SamplePuzzle
{
    public struct AABB_Old
    {
        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }

        public AABB_Old(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public readonly bool Intersects(AABB_Old other)
        {
            return Min.x < other.Max.x && Max.x > other.Min.x &&
                   Min.y < other.Max.y && Max.y > other.Min.y;
        }

        public AABB_Old Translate(Vector3 position)
        {
            return new AABB_Old(Min + position, Max + position);
        }
    }
}
