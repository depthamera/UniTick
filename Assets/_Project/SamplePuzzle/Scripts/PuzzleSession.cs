using System;
using System.Collections.Generic;
using UnityEngine;

namespace SamplePuzzle 
{
    public class PuzzleSession
    {
        private PlayerModel_OLD _playerModel;
        private List<GhostModel> _ghostModels = new();

        private List<ITickable> _tickables = new();
        private List<List<MoveSnapshot>> _playerSnapshots = new();
        private List<MoveSnapshot> _currentPlayerSnapshots = new();

        private List<PhysicalDoor> _doors = new();
        private List<PhysicalButton> _buttons = new();

        private PuzzleSessionConfig _setting;

        private int _nextRemoveGhostIndex = 0;

        private CollisionWorld_OLD _collisionWorld = new();

        public PuzzleSession(PuzzleSessionConfig setting)
        {
            _setting = setting;

            _playerModel = new PlayerModel_OLD(setting.StartPosition, setting.MoveSpeed);
            _tickables.Add(_playerModel);
            _collisionWorld.AddEntity(_playerModel);

            _playerSnapshots.Add(new());

            foreach (var doorConfig in setting.Doors)
            {
                var door = new PuzzleDoor();
                var physicalDoor = new PhysicalDoor(door,
                    new AABB(Vector3.zero, new Vector3(1, 1, 0)), doorConfig.Position);

                _doors.Add(physicalDoor);
                _collisionWorld.AddEntity(physicalDoor);
            }

            foreach (var buttonConfig in setting.Buttons)
            {
                var linkedDoor = _doors[buttonConfig.LinkedDoorIndex].Door;
                var button = new PuzzleButton(linkedDoor);
                var physicalButton = new PhysicalButton(button, 
                    new AABB(Vector3.zero, new Vector3(1, 1, 0)), buttonConfig.Position);

                _buttons.Add(physicalButton);
                _collisionWorld.AddEntity(physicalButton);

            }
        }

        public IReadOnlyPlayerModel Player => _playerModel;

        public int GhostCount => _ghostModels.Count;

        public IReadOnlyList<GhostModel> Ghosts => _ghostModels;

        public IReadOnlyList<PhysicalDoor> Doors => _doors;
        public IReadOnlyList<PhysicalButton> Buttons => _buttons;

        public void AdvanceTicks(int ticks)
        {
            for (int i = 0; i < ticks; ++i) {
                foreach (var tickable in _tickables)
                {
                    tickable.Step();
                }

                _collisionWorld.Step();
                _playerSnapshots[^1].Add(_playerModel.TakeSnapshot());
            }
        }

        public IReadOnlyList<MoveSnapshot> GetSnapshots(int loopIndex)
        {
            return _playerSnapshots[loopIndex];
        }

        public void SetPlayerMove(Vector3 direction)
        {
            _playerModel.SetMoveDirection(direction.normalized);
        }

        public void TriggerLoop()
        {
            _playerModel.SetPosition(_setting.StartPosition);
            
            foreach(var ghost in _ghostModels)
            {
                ghost.SetPosition(_setting.StartPosition);
                ghost.ResetTimeline();
            }

            if (Ghosts.Count >= _setting.MaxGhost)
            {
                _ghostModels[_nextRemoveGhostIndex].SetTimeline(_playerSnapshots[^1]);
                _nextRemoveGhostIndex = (_nextRemoveGhostIndex + 1) % _setting.MaxGhost;
            }
            else
            {
                var newGhost = new GhostModel(_setting.MoveSpeed);
                _collisionWorld.AddEntity(newGhost);
                newGhost.SetPosition(_setting.StartPosition);
                newGhost.SetTimeline(_playerSnapshots[^1]);
                _ghostModels.Add(newGhost);
                _tickables.Add(newGhost);
            }

            _playerSnapshots.Add(new());

            _collisionWorld.Step();
        }
    }
}
