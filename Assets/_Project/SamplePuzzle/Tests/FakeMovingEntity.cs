
using UnityEngine;

namespace SamplePuzzle.Tests
{
    public class FakeMovingEntity : FakeCollisionEntity
    {
        private Vector3 _position;
        private Vector3 _localSize;
        private Vector3 _offset;

        public Vector3 Position { get; set; }
        public override AABB_Old Bounds => _bounds.Translate(Position);
        public FakeMovingEntity(AABB_Old bounds) : base(bounds)
        {
        }

        public FakeMovingEntity(Vector3 position, Vector3 localSize, Vector3 offset) : base(new AABB_Old(position - localSize / 2 + offset, position + localSize / 2 + offset))
        {
            _position = position;
            _localSize = localSize;
            _offset = offset;
        }
    }
}
