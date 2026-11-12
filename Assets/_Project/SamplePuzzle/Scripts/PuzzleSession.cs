using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

// TODO: def 기반으로 세션 생성과 초기화. 

namespace SamplePuzzle
{
    public sealed class PuzzleSession
    {
        public int CurrentTick { get; private set; }

        public FixVector2 PlayerPosition => _playerBodyHandle.Position;

        private CollisionWorld _world;
        private PlayerActor _player;
        private IDynamicBodyHandle _playerBodyHandle;
        private readonly List<IBodyHandle> _bodies = new();
        public List<ButtonState> Buttons { get; } = new();
        public List<DoorState> Doors { get; } = new();

        private readonly List<IBodyHandle> _overlapHandles = new();
        
        private readonly Dictionary<IBodyHandle, SimId> _handleToId = new();

        private List<FixVector2> _currentMoveRecord = new();
        private readonly List<List<FixVector2>> _closedMoveRecords = new();

        private SimIdAllocator _idAllocator = new();
        private readonly LevelDefinition _definition;

        public int MaxGhosts { get; }
        public int OldestGhostLoopIndex { get; private set; } = 0;
        public readonly List<GhostState> _ghosts = new();
        public PlayerState Player { get; private set; }

        public PuzzleSession(LevelDefinition definition)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            BuildLevel();
        }

        private void BuildLevel()
        {
            _world = new CollisionWorld();

            SpawnPlayer(_definition.StartPosition, FixVector2.One);

            foreach (var wall in _definition.Walls)
            {
                SpawnWall(wall.Position, wall.Size);
            }
            foreach (var button in _definition.Buttons)
            {
                SpawnButton(button.Position, button.Size, button.Id);
            }
            foreach (var door in _definition.Doors)
            {
                SpawnDoor(door.Position, door.Size, door.OpenedBy, door.Id);
            }
        }

        private void ClearLevel()
        {

        }

        public void Step(GameCommand cmd = GameCommand.None)
        {
            var direction = CommandInterpreter.ToDirection(cmd);
            var displacement = direction * _player.Speed;
            _playerBodyHandle.RequestMove(displacement);
            _currentMoveRecord.Add(displacement);

            for(int i = OldestGhostLoopIndex; i<_closedMoveRecords.Count; ++i)
            {
                if (_closedMoveRecords[i].Count <= CurrentTick) continue;

                var ghost = _ghosts[i - OldestGhostLoopIndex];
                ghost.BodyHandle.RequestMove(_closedMoveRecords[i][CurrentTick]);
            }

            _world.ResolveMove();

            _world.CheckOverlaps(_overlapHandles);
            Debug.Log($"Tick {CurrentTick}: {_overlapHandles.Count} overlaps detected.");

            for (int i = 0; i < Buttons.Count; ++i)
            {
                var button = Buttons[i];
                button.Pressed = false;
                Buttons[i] = button;
            }

            foreach (var handle in _overlapHandles)
            {
                if(_handleToId.TryGetValue(handle, out var id))
                {
                    int idx = Buttons.FindIndex(x => x.Id == id);
                    if(idx >= 0)
                    {
                        Debug.Log($"Button {id.Value} pressed at tick {CurrentTick}");
                        var button = Buttons[idx];
                        button.Pressed = true;
                        Buttons[idx] = button;
                    }
                }
            }

            for(int i = 0; i < Doors.Count; ++i)
            {
                var door = Doors[i];

                int idx = Buttons.FindIndex(x => x.Id == door.OpenedBy);

                if (idx >= 0)
                {
                    var button = Buttons[idx];
                    if (button.Pressed)
                    {
                        door.Open = true;
                    }
                    else
                    {
                        door.Open = false;
                    }
                }

                Doors[i] = door;
            }

            CurrentTick++;
        }

        public void TriggerLoop()
        {
            CurrentTick = 0;

            _closedMoveRecords.Add(_currentMoveRecord);
            _currentMoveRecord = new List<FixVector2>();

            _world.Place(_playerBodyHandle, _definition.StartPosition);

            foreach (var ghost in _ghosts)
            {
                _world.RemoveDynamicBody(ghost.BodyHandle);
            }

            _ghosts.Clear();
            OldestGhostLoopIndex = Math.Max(0, _closedMoveRecords.Count - MaxGhosts);
            for (int i = OldestGhostLoopIndex; i < _closedMoveRecords.Count; ++i)
            {
                var ghostPosition = _definition.StartPosition;
                var ghostBody = _world.CreateDynamicBody(
                    ghostPosition,
                    FixVector2.One,
                    FixVector2.Zero);
                _ghosts.Add(new GhostState(i, ghostBody));
            }
        }

        public void SpawnWall(FixVector2 position, FixVector2 size)
        {
            _world.CreateFixedBody(
                position,
                size,
                FixVector2.Zero);
        }

        public SimId SpawnPlayer(FixVector2 position, FixVector2 size) => SpawnPlayer(position, size, _idAllocator.Next());

        public SimId SpawnPlayer(FixVector2 position, FixVector2 size, SimId id)
        {
            _idAllocator.Reserve(id);
            _player = new PlayerActor(_definition.MoveSpeed);
            _playerBodyHandle = _world.CreateDynamicBody(
                position,
                FixVector2.One,
                FixVector2.Zero);
            return id;
        }

        public SimId SpawnButton(FixVector2 position, FixVector2 size) => SpawnButton(position, size, _idAllocator.Next());

        public SimId SpawnButton(FixVector2 position, FixVector2 size, SimId id)
        {
            _idAllocator.Reserve(id);
            var state = new ButtonState(id);
            var body = _world.CreateTriggerBody(
                position,
                size,
                FixVector2.Zero);
            _bodies.Add(body);
            Buttons.Add(state);
            _handleToId.Add(body, id);

            return id;
        }

        public SimId SpawnDoor(FixVector2 position, FixVector2 size, SimId openedBy, bool open = false) => SpawnDoor(position, size, openedBy, _idAllocator.Next(), open);

        public SimId SpawnDoor(FixVector2 position, FixVector2 size, SimId openedBy, SimId id, bool open = false)
        {
            _idAllocator.Reserve(id);
            var state = new DoorState(id, openedBy, open);
            var body = _world.CreateFixedBody(
                position,
                size,
                FixVector2.Zero);
            _bodies.Add(body);
            Doors.Add(state);
            _handleToId.Add(body, id);
            return id;
        }

        public bool IsOpen(SimId door)
        {
            int idx = Doors.FindIndex(x => x.Id == door);

            if(idx >= 0)
            {
                return Doors[idx].Open;
            }

            throw new ArgumentException($"Door with id {door.Value} not found.");
        }

        public bool IsPressed(SimId button)
        {
            int idx = Buttons.FindIndex(x => x.Id == button);

            if (idx >= 0)
            {
                return Buttons[idx].Pressed;
            }

            throw new ArgumentException($"Button with id {button.Value} not found.");
        }

        public IReadOnlyList<FixVector2> GetMoveRecord(int loopIndex)
        {
            if(loopIndex < 0 || loopIndex >= _closedMoveRecords.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(loopIndex), $"Loop index {loopIndex} is out of range.");
            }

            return _closedMoveRecords[loopIndex];
        }

        public FixVector2 GetGhostPosition(int sourceLoopIndex)
        {
            if(sourceLoopIndex < 0 || sourceLoopIndex >= _closedMoveRecords.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceLoopIndex), $"Source loop index {sourceLoopIndex} is out of range.");
            }

            if(sourceLoopIndex < OldestGhostLoopIndex)
            {
                throw new InvalidOperationException($"Source loop index {sourceLoopIndex} is too old. Oldest active ghost loop index is {OldestGhostLoopIndex}.");
            }

            return _ghosts[sourceLoopIndex - OldestGhostLoopIndex].BodyHandle.Position;
        }
    }
}
