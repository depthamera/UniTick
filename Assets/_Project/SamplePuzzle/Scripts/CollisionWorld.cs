using System;
using System.Collections.Generic;
using UnityEngine;

namespace SamplePuzzle
{
    public class CollisionWorld
    {
        private readonly List<CollisionBody> _bodies = new();

        public IBodyHandle CreateBody(ICollidable owner, Vector2 position, Vector2 size, Vector2 offset)
        {
            var half = size * 0.5f;
            var bounds = new AABB(
                min: half - offset, 
                max: half + offset);

            var body = new CollisionBody(
                owner: owner,
                position: position,
                bounds: bounds);
            _bodies.Add(body);

            return body;
        }

        public void Resolve()
        {
            foreach(var body in _bodies)
            {
                var nextPosition = body.Position + body.PendingMove;
                var nextBounds = body.Bounds.Translate(nextPosition);


                body.Position += body.PendingMove;
                body.PendingMove = Vector2.zero;
            }


        }
    }
}
