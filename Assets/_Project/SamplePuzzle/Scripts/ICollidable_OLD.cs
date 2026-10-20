using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SamplePuzzle
{ 
    public interface ICollidable_OLD
    {
        AABB Bounds { get; }
        Vector3 Offset { get; }
        bool IsTrigger { get; }
        bool IsStatic { get; }

        void OnOverlap(ICollidable_OLD other);
        void OnSeparate(ICollidable_OLD entityB);
    }
}
