using System.Collections.Generic;
using UnityEngine;

namespace SamplePuzzle
{
    public class PhysicalButton : ICollidable_OLD
    {
        private PuzzleButton _button;
        private AABB _border;
        private Vector3 _position;

        private HashSet<ICollidable_OLD> _overlappingEntities = new HashSet<ICollidable_OLD>();

        public Vector3 Position => _position;
        public PhysicalButton(PuzzleButton button, AABB border, Vector3 position, Vector3 offset = default)
        {
            _button = button;
            _border = border;
            _position = position;
            Offset = offset;
        }
        public AABB Bounds => _border.Translate(_position);

        public Vector3 Offset { get; }

        public bool IsTrigger => true;
        public bool IsStatic => false;

        public void OnOverlap(ICollidable_OLD other)
        {
            _overlappingEntities.Add(other);

            if(_overlappingEntities.Count > 0)
            {
                _button.Press();
            }
        }

        public void OnSeparate(ICollidable_OLD entityB)
        {
            _overlappingEntities.Remove(entityB);

            if (_overlappingEntities.Count == 0)
            {
                _button.Release();
            }
        }
    }
}

      