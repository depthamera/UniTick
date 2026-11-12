using UnityEngine;

namespace SamplePuzzle
{
    public class Body
    {
        public Vector2 Position { get; set; }

        public AABB_Old LocalBounds { get; }
        public AABB_Old WorldBounds => LocalBounds.Translate(Position);

        public Body(Vector2 position, Vector2 size, Vector2 offset)
        {
            Position = position;

            var half = size * 0.5f;
            LocalBounds = new AABB_Old(
                min: offset - half, 
                max: half + offset);
        }
    }
}
