using System.Collections.Generic;

namespace SamplePuzzle
{
    public sealed class LevelDefinition
    {
        public LevelDefinition(
            FixVector2 startPosition, 
            Fix64 moveSpeed, int maxGhosts, 
            List<WallDefinition> walls, 
            List<ButtonDefinition> buttons, 
            List<DoorDefinition> doors)
        {
            StartPosition = startPosition;
            MoveSpeed = moveSpeed;
            MaxGhosts = maxGhosts;
            Walls = walls?.AsReadOnly() ?? new List<WallDefinition>().AsReadOnly();
            Buttons = buttons?.AsReadOnly() ?? new List<ButtonDefinition>().AsReadOnly();
            Doors = doors?.AsReadOnly() ?? new List<DoorDefinition>().AsReadOnly();
        }

        public FixVector2 StartPosition { get; }
        public Fix64 MoveSpeed { get; }
        public int MaxGhosts { get; }
        public IReadOnlyList<WallDefinition> Walls { get; }
        public IReadOnlyList<ButtonDefinition> Buttons { get; }
        public IReadOnlyList<DoorDefinition> Doors { get; }
    }
}
