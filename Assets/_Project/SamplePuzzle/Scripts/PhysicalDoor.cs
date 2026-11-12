using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SamplePuzzle
{
    public class PhysicalDoor : ICollidable_OLD
    {
        public PuzzleDoor Door { get; }

        public Vector3 Position => _position;


        public AABB_Old Bounds => _aABB.Translate(_position);

        public Vector3 Offset { get; }

        private AABB_Old _aABB;
        private Vector3 _position;

        public bool IsTrigger => true;
        public bool IsStatic => true;
        public PhysicalDoor(PuzzleDoor door, AABB_Old aABB, Vector3 position, Vector3 offset = default)
        {
            Door = door;
            _aABB = aABB;
            _position = position;
            Offset = offset;
        }

        public void OnOverlap(ICollidable_OLD other)
        {
        }

        public void OnSeparate(ICollidable_OLD entityB)
        {
        }
    }
}
