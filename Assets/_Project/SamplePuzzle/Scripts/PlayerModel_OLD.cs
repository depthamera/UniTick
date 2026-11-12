using System;
using UnityEngine;

namespace SamplePuzzle
{
    public class PlayerModel_OLD : ITickable, IReadOnlyPlayerModel, ICollidable_OLD
    {
        private Vector3 _position;
        private float _moveSpeed;
        private Vector3 _moveDirection = Vector3.right;
        private SnapshotLogger _logger;

        public Vector3 Position => _position;

        public AABB_Old Bounds => new AABB_Old(Vector3.zero, new Vector3(1, 1, 0)).Translate(Position);

        public Vector3 Offset { get; }
        public bool IsTrigger => false;
        public bool IsStatic => false;

        public PlayerModel_OLD(Vector3 startPosition, float moveSpeed, SnapshotLogger logger = null, Vector3 offset = default)
        {
            _position = startPosition;
            _moveSpeed = moveSpeed;
            Offset = offset;

            if (logger is not null)
                _logger = logger;
        }

        public void SetPosition(Vector3 position)
        {
            _position = position;
        }

        public void SetSpeed(float speed)
        {
            _moveSpeed = speed;
        }

        public void SetMoveDirection(Vector3 moveDirection)
        {
            _moveDirection = moveDirection;
        }


        public void Step()
        {
            _position += _moveDirection * _moveSpeed;

            _logger?.Log(TakeSnapshot());
        }

        public void SetLogger(SnapshotLogger snapshotLogger)
        {
            _logger = snapshotLogger;
        }

        public MoveSnapshot TakeSnapshot()
        {
            return new MoveSnapshot(position: _position, moveDirection: _moveDirection);
        }

        public void OnOverlap(ICollidable_OLD other)
        {
            
        }

        public void OnSeparate(ICollidable_OLD entityB)
        {
            
        }
    }
}