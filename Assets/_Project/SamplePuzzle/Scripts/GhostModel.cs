using System;
using System.Collections.Generic;
using UnityEngine;

namespace SamplePuzzle
{
    public class GhostModel : ITickable, ICollidable_OLD
    {
        private Vector3 _position;
        private float _speed = 1f;

        private IReadOnlyList<MoveSnapshot> _timeline;
        private int _currentTimelineIndex = 0;

        public GhostModel(float moveSpeed, Vector3 offset = default)
        {
            _speed = moveSpeed;
            Offset = offset;
        }

        public Vector3 Position => _position;

        public AABB_Old Bounds => new AABB_Old(Vector3.zero, new Vector3(1, 1, 0)).Translate(Position);

        public Vector3 Offset { get; }

        public bool IsTrigger => false;

        public bool IsStatic => false;

        public void SetPosition(Vector3 startPosition)
        {
            _position = startPosition;
        }

        public void SetTimeline(IReadOnlyList<MoveSnapshot> timeline)
        {
            _timeline = timeline;
            _currentTimelineIndex = 0;
        }

        public void Step()
        {
            if (_timeline == null) return;
            if (_currentTimelineIndex >= _timeline.Count) return;
            
            
            MoveSnapshot snapshot = _timeline[_currentTimelineIndex];
            _position += snapshot.MoveDirection * _speed;

            _currentTimelineIndex++;
        }

        public void ResetTimeline()
        {
            _currentTimelineIndex = 0;
        }

        public void OnOverlap(ICollidable_OLD other)
        {
            
        }

        public void OnSeparate(ICollidable_OLD entityB)
        {
            
        }
    }
}