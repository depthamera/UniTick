
using System;
using UnityEngine;

namespace SamplePuzzle
{
    public struct AABB
    {
        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }

        public AABB(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public readonly bool Intersects(AABB other)
        {
            return Min.x < other.Max.x && Max.x > other.Min.x &&
                   Min.y < other.Max.y && Max.y > other.Min.y;
        }

        public AABB Translate(Vector3 position)
        {
            return new AABB(Min + position, Max + position);
        }
    }
}
