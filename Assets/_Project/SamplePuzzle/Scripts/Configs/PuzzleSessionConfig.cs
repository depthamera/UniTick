using System.Collections.Generic;
using UnityEngine;

namespace SamplePuzzle
{
    public readonly struct PuzzleSessionConfig
    {
        public float MoveSpeed { get; }
        public Vector3 StartPosition { get; }
        public int MaxGhost { get; }
        public List<DoorConfig> Doors { get; }
        public List<ButtonConfig> Buttons { get; }

        public PuzzleSessionConfig(float moveSpeed, Vector3 startPosition, int maxGhost, List<DoorConfig> doors, List<ButtonConfig> buttons)
        {
            MoveSpeed = moveSpeed;
            StartPosition = startPosition;
            MaxGhost = maxGhost;
            Doors = doors;
            Buttons = buttons;
        }
    }
}
