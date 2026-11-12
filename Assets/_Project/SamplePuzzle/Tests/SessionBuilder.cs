using System;
using System.Collections.Generic;

namespace SamplePuzzle.Tests
{
    internal class SessionBuilder
    {
        private FixVector2 _startPosition = FixVector2.Zero;
        private Fix64 _moveSpeed = Fix64.One;
        private int _maxGhosts = 0;
        private List<WallDefinition> _walls = new();
        private List<ButtonDefinition> _buttons = new();
        private List<DoorDefinition> _doors = new();
        private SimIdAllocator _allocator = new();

        internal SimId CreateButton(FixVector2 position)
        {
            var sid = _allocator.Next();
            _buttons.Add(new ButtonDefinition(position, FixVector2.One, sid));
            return sid;
        }

        internal SimId CreateDoor(FixVector2 position, SimId openedBy)
        {
            var sid = _allocator.Next();
            _doors.Add(new DoorDefinition(position, FixVector2.One, sid, openedBy));
            return sid;
        }

        internal SimId CreatePlayer(FixVector2? position = null, Fix64? moveSpeed = null)
        {
            var sid = _allocator.Next();
            _startPosition = position ?? FixVector2.Zero;
            _moveSpeed = moveSpeed ?? Fix64.One;
            return sid;
        }

        internal void SetMaxGhosts(int maxGhosts)
        {
            _maxGhosts = maxGhosts;
        }

        internal PuzzleSession Build()
        {
            var definition = new LevelDefinition(_startPosition, _moveSpeed, _maxGhosts, _walls, _buttons, _doors);
            return new PuzzleSession(definition);
        }
    }
}
