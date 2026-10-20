using UnityEngine;

namespace SamplePuzzle.Tests
{
    public class FakeCollisionEntity : ICollidable_OLD
    {
        protected AABB _bounds;
        public int OverlapCount { get; set; }
        public ICollidable_OLD LastOverlappedWith { get; set; }
        
        public FakeCollisionEntity(AABB bounds)
        {
            _bounds = bounds;
            OverlapCount = 0;
            LastOverlappedWith = null;
        }
        
        public virtual AABB Bounds => _bounds;
        public bool IsTrigger => true;
        public bool IsStatic => false;

        public Vector3 Offset { get; } = Vector3.zero;

        public void OnOverlap(ICollidable_OLD other)
        {
            OverlapCount++;
            LastOverlappedWith = other;
        }

        public void OnSeparate(ICollidable_OLD entityB)
        {
            OverlapCount--;
            LastOverlappedWith = null;
        }
    }
}
