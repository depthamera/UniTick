using UnityEngine;

namespace SamplePuzzle
{
    public readonly struct ButtonConfig
    {
        public ButtonConfig(Vector3 position, int linkedDoorIndex)
        {
            Position = position;
            LinkedDoorIndex = linkedDoorIndex;
        }

        public Vector3 Position { get; }
        public int LinkedDoorIndex { get; }
    }
}
