using UnityEngine;

namespace SamplePuzzle
{
    internal sealed class CollisionBody : IBodyHandle
    {
        internal ICollidable Owner;
        public Vector2 Position { get; internal set; }
        internal AABB Bounds;
        internal Vector2 PendingMove;

        public CollisionBody(ICollidable owner, Vector2 position, AABB bounds)
        {
            Owner = owner;
            Position = position;
            Bounds = bounds;
        }

        public void RequestMove(Vector2 displacement)
        {
            PendingMove = displacement;
        }
    }
}
