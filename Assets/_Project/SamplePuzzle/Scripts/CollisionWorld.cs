using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SamplePuzzle
{
    public class CollisionWorld
    {
        private readonly List<DynamicBody> _dynamicBodies = new();
        private readonly List<FixedBody> _staticBodies = new();
        private readonly List<FixedBody> _triggerBodies = new();

        public IDynamicBodyHandle CreateDynamicBody(FixVector2 position, FixVector2 size, FixVector2 offset)
        {
            var bounds = GetBounds(size, offset);

            var body = new DynamicBody(
                position: position,
                bounds: bounds);

            _dynamicBodies.Add(body);

            return body;
        }

        public IBodyHandle CreateFixedBody(FixVector2 position, FixVector2 size, FixVector2 offset)
        {
            var bounds = GetBounds(size, offset);

            var body = new FixedBody(
                position: position,
                bounds: bounds);

            _staticBodies.Add(body);

            return body;
        }

        public IBodyHandle CreateTriggerBody(FixVector2 position, FixVector2 size, FixVector2 offset)
        {
            var bounds = GetBounds(size, offset);
            var body = new FixedBody(
                position: position,
                bounds: bounds);
            _triggerBodies.Add(body);
            return body;
        }

        public void Place(IDynamicBodyHandle handle, FixVector2 targetPosition)
        {
            if (_dynamicBodies.Find(x => ReferenceEquals(x, handle)) is not DynamicBody body)
            {
                throw new ArgumentException("현재 월드에 존재하지 않는 오브젝트");
            }

            var bounds = body.Bounds.Translate(targetPosition);
            foreach(var staticBody in _staticBodies)
            {
                var staticBounds = staticBody.Bounds.Translate(staticBody.Position);
                if(bounds.Intersects(staticBounds))
                {
                    throw new ArgumentException("배치하려는 위치에 이미 오브젝트가 있음");
                }
            }

            body.Position = targetPosition;
        }

        public void ResolveMove()
        {
            foreach(var body in _dynamicBodies)
            {
                var move = body.PendingMove;
                var position = body.Position;

                if(move.X != Fix64.Zero)
                {
                    position.X += move.X;
                    var bounds = body.Bounds.Translate(position);

                    foreach(var other in _staticBodies)
                    {
                        var otherBounds = other.Bounds.Translate(other.Position);
                        if (!otherBounds.Intersects(bounds)) continue;

                        if (move.X > Fix64.Zero)
                            position.X = otherBounds.Min.X - body.Bounds.Max.X;
                        else
                            position.X = otherBounds.Max.X - body.Bounds.Min.X;

                        bounds = body.Bounds.Translate(position);
                    }
                }

                if (move.Y != Fix64.Zero)
                {
                    position.Y += move.Y;
                    var bounds = body.Bounds.Translate(position);

                    foreach(var other in _staticBodies)
                    {
                        var otherBounds = other.Bounds.Translate(other.Position);
                        if (!otherBounds.Intersects(bounds)) continue;

                        if (move.Y > Fix64.Zero)
                            position.Y = otherBounds.Min.Y - body.Bounds.Max.Y;
                        else
                            position.Y = otherBounds.Max.Y - body.Bounds.Min.Y;

                        bounds = body.Bounds.Translate(position);
                    }
                }

                body.Position = position;
                body.PendingMove = FixVector2.Zero;
            }
        }

        private static AABB GetBounds(FixVector2 size, FixVector2 offset)
        {
            var half = size / Fix64.FromInt(2);
            return new AABB(
                min: offset - half,
                max: offset + half);
        }

        public void CheckOverlaps(List<IBodyHandle> result)
        {
            Debug.Log($"Checking overlaps: {_dynamicBodies.Count} dynamic bodies, {_triggerBodies.Count} trigger bodies.");
            result.Clear();

            foreach (var dynamicBody in _dynamicBodies)
            {
                var dynamicBounds = dynamicBody.Bounds.Translate(dynamicBody.Position);
                foreach (var triggerBody in _triggerBodies)
                {
                    var triggerBounds = triggerBody.Bounds.Translate(triggerBody.Position);
                    if (dynamicBounds.Intersects(triggerBounds))
                    {
                        result.Add(triggerBody);
                    }
                }
            }
        }

        public void RemoveDynamicBody(IBodyHandle ghost)
        {
            if(ghost is not DynamicBody dynamicBody)
            {
                throw new ArgumentException("Ghost must be a DynamicBody.");
            }

            if (!_dynamicBodies.Remove(dynamicBody))
            {
                throw new ArgumentException("The specified ghost is not part of the world.");
            }
        }
    }
}
