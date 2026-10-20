using UnityEngine;

namespace SamplePuzzle
{
    public readonly struct DoorConfig
    {
        public DoorConfig(Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; }
    }
}
