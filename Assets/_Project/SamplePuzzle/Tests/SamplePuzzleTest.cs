using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace SamplePuzzle.Tests
{

    public class SamplePuzzleTest
    {

        [TestCase(GameCommand.Up, 0, 1)]
        [TestCase(GameCommand.Down, 0, -1)]
        [TestCase(GameCommand.Left, -1, 0)]
        [TestCase(GameCommand.Right, 1, 0)]
        public void CommandInterpreter_ToDirection_ReturnsUnitVector(GameCommand cmd, int expectedX, int expectedY)
        {
            var actual = CommandInterpreter.ToDirection(cmd);
            Assert.That(actual, Is.EqualTo(new FixVector2(expectedX, expectedY)));

        }


        [Test]
        [Category("OLD")]
        public void Ghost_MimicPlayerActions_ReachesExpectedPosition()
        {
            int ticks = 42;

            var snapshotLogger = new SnapshotLogger();
            var player = CreatePlayer(logger: snapshotLogger);

            var snapshots = AdvanceTicksWithPlayerLogs(player, ticks);

            GhostModel ghost = CreateGhost();
            ghost.SetTimeline(snapshots);
            AdvanceTicks(ghost, ticks);

            AssertPositionEquals(ghost.Position, player.Position);
        }

        

        private IReadOnlyList<MoveSnapshot> AdvanceTicksWithPlayerLogs(PlayerModel_OLD player, int ticks)
        {
            List<MoveSnapshot> snapshots = new();

            for (int i = 0; i < ticks; ++i)
            {
                player.Step();
                snapshots.Add(player.TakeSnapshot());
            }

            return snapshots;
        }


        [Test]
        public void PuzzleSession_WhenLoopTriggered_ShouldSpawnGhostAndMimicPreviousPlayerActions()
        {
            var startPosition = Vector3.zero;

            var session = CreateSession(startPosition: startPosition);
            session.AdvanceTicks(10);

            session.TriggerLoop();

            IReadOnlyList<MoveSnapshot> previousSnapshots = session.GetSnapshots(loopIndex: 0);
            var ghost = session.Ghosts[0];

            Assert.That(ghost.Position, Is.EqualTo(startPosition));

            for (int i = 0; i < 10; ++i)
            {
                session.AdvanceTicks(1);

                Assert.That(ghost.Position, Is.EqualTo(previousSnapshots[i].Position));
            }
        }

        [Test]
        public void PuzzleSession_AdvanceAfterLoop_ShouldMoveBothPlayerAndGhost()
        {
            var session = CreateSession();

            session.SetPlayerMove(Vector3.right);
            session.AdvanceTicks(1);
            session.TriggerLoop();

            session.SetPlayerMove(Vector3.up);
            session.AdvanceTicks(1);

            Assert.That(session.Player.Position, Is.EqualTo(Vector3.up));
            Assert.That(session.Ghosts[0].Position, Is.EqualTo(Vector3.right));
        }

        [Test]
        public void PuzzleSession_AfterPlayerMove_CaptureCorrectSnapshot()
        {
            var session = CreateSession();
            var expected = new List<MoveSnapshot>()
        {
            new(new Vector3(0, 1, 0), Vector3.up),
            new(new Vector3(-1, 1, 0), Vector3.left),
            new(new Vector3(-2, 1, 0), Vector3.left)
        };

            session.SetPlayerMove(Vector3.up);
            session.AdvanceTicks(1);

            session.SetPlayerMove(Vector3.left);
            session.AdvanceTicks(2);

            var snapshots = session.GetSnapshots(0);

            for (int i = 0; i < 3; ++i)
            {
                Assert.That(snapshots[i], Is.EqualTo(expected[i]));
            }
        }

        [Test]
        public void PuzzleSession_WhenInitializedWithMaxGhostCount_ShouldCreateExactNumberOfGhosts()
        {
            var session = CreateSession(maxGhost: 10);

            for (int i = 0; i < 16; ++i)
            {
                session.TriggerLoop();
            }

            Assert.That(session.Ghosts.Count, Is.EqualTo(10));
        }

        [Test]
        public void PuzzleSession_AdvanceAfterLoop_ShouldMovePlayerAndAllGhosts()
        {
            var playerMoves = new List<List<Vector3>>()
            {
                new() { Vector3.right, Vector3.up, Vector3.left },
                new() { Vector3.down, Vector3.down, Vector3.down },
                new() { Vector3.left, Vector3.up, Vector3.right },
                new() { Vector3.up, Vector3.up, Vector3.up }
            };

            var expectedPositions = new List<Vector3>()
            {
                Vector3.up, Vector3.down * 3, Vector3.up, Vector3.up * 3
            };

            var session = CreateSession(maxGhost: 2);

            for (int loopIndex = 0; loopIndex < playerMoves.Count; ++loopIndex)
            {
                foreach (var move in playerMoves[loopIndex])
                {
                    session.SetPlayerMove(move);
                    session.AdvanceTicks(1);
                }

                if (loopIndex < expectedPositions.Count - 1)
                {
                    session.TriggerLoop();
                }
            }

            Assert.That(session.Player.Position, Is.EqualTo(expectedPositions[3]));
            Assert.That(session.Ghosts[0].Position, Is.EqualTo(expectedPositions[2]));
            Assert.That(session.Ghosts[1].Position, Is.EqualTo(expectedPositions[1]));
        }

        [Test]
        public void PuzzleSession_InitializedWithConfig_ShouldSetupPlayerAndDoorAndButton()
        {
            var config = new PuzzleSessionConfig(
                moveSpeed: 1f,
                startPosition: Vector3.zero,
                maxGhost: 1,
                doors: new List<DoorConfig>
                {
                    new DoorConfig(position: new Vector3(5f, 0f, 0f)),
                    new DoorConfig(position: new Vector3(10f, 0f, 0f))
                },
                buttons: new List<ButtonConfig>
                {
                    new ButtonConfig(position: new Vector3(2f, 0f, 0f), linkedDoorIndex: 0),
                    new ButtonConfig(position: new Vector3(7f, 0f, 0f), linkedDoorIndex: 1)
                }
            );


            var session = new PuzzleSession_OLD(config);

            Assert.That(session.Player.Position, Is.EqualTo(Vector3.zero));

            Assert.That(session.Doors[0].Position, Is.EqualTo(new Vector3(5f, 0f, 0f)));
            Assert.That(session.Doors[1].Position, Is.EqualTo(new Vector3(10f, 0f, 0f)));

            Assert.That(session.Buttons[0].Position, Is.EqualTo(new Vector3(2f, 0f, 0f)));
            Assert.That(session.Buttons[1].Position, Is.EqualTo(new Vector3(7f, 0f, 0f)));
        
        }

        [Test]
        public void PuzzleSession_AfterLoop_DoorShouldClose()
        {
            var config = new PuzzleSessionConfig(
                moveSpeed: 1f,
                startPosition: Vector3.zero,
                maxGhost: 1,
                doors: new List<DoorConfig>
                {
                    new DoorConfig(position: new Vector3(5f, 0f, 0f)),
                },
                buttons: new List<ButtonConfig>
                {
                    new ButtonConfig(position: new Vector3(2f, 0f, 0f), linkedDoorIndex: 0)
                }
            );

            var session = new PuzzleSession_OLD(config);

            // Player moves to button and opens the door
            session.SetPlayerMove(Vector3.right);
            session.AdvanceTicks(2);
            Assert.That(session.Doors[0].Door.IsOpen, Is.True);


            session.TriggerLoop();
            Assert.That(session.Doors[0].Door.IsOpen, Is.False);
        }

        [Test]
        public void PuzzleSession_WhenGhostMovesIntoButton_ShouldOpenDoor()
        {
            var config = new PuzzleSessionConfig(
                moveSpeed: 1f,
                startPosition: Vector3.zero,
                maxGhost: 1,
                doors: new List<DoorConfig>
                {
                    new DoorConfig(position: new Vector3(5f, 0f, 0f)),
                },
                buttons: new List<ButtonConfig>
                {
                    new ButtonConfig(position: new Vector3(2f, 0f, 0f), linkedDoorIndex: 0)
                }
            );
            var session = new PuzzleSession_OLD(config);

            // Player moves to button and opens the door

            session.SetPlayerMove(Vector3.right);
            session.AdvanceTicks(2);
            Assert.That(session.Doors[0].Door.IsOpen, Is.True);

            session.TriggerLoop();
            Assert.That(session.Doors[0].Door.IsOpen, Is.False);

            // Ghost should now mimic the player's previous actions
            session.SetPlayerMove(Vector3.left);
            session.AdvanceTicks(2);
            Assert.That(session.Ghosts[0].Position, Is.EqualTo(new Vector3(2f, 0f, 0f)), "Ghost is not at the expected position");
            Assert.That(session.Doors[0].Door.IsOpen, Is.True, "Ghost did not open the door");
        }

        [Test]
        public void PuzzleSession_WhenGhostMovesOutOfButton_ShouldCloseDoor()
        {
            var config = new PuzzleSessionConfig(
                moveSpeed: 1f,
                startPosition: Vector3.zero,
                maxGhost: 1,
                doors: new List<DoorConfig>
                {
                    new DoorConfig(position: new Vector3(15f, 0f, 0f)),
                },
                buttons: new List<ButtonConfig>
                {
                    new ButtonConfig(position: new Vector3(2f, 0f, 0f), linkedDoorIndex: 0)
                }
            );
            var session = new PuzzleSession_OLD(config);

            // Player moves to button and opens the door
            session.SetPlayerMove(Vector3.right);
            session.AdvanceTicks(2);
            Assert.That(session.Doors[0].Door.IsOpen, Is.True);
            session.AdvanceTicks(1);
            Assert.That(session.Doors[0].Door.IsOpen, Is.False);

            session.TriggerLoop();

            // Ghost should now mimic the player's previous actions
            session.SetPlayerMove(Vector3.left);
            session.AdvanceTicks(2);
            Assert.That(session.Ghosts[0].Position, Is.EqualTo(new Vector3(2f, 0f, 0f)), "Ghost is not at the expected position");
            Assert.That(session.Doors[0].Door.IsOpen, Is.True, "Ghost did not open the door");
            // Now move the player away from the button to close the door

            session.AdvanceTicks(1);
            Assert.That(session.Ghosts[0].Position, Is.EqualTo(new Vector3(3f, 0f, 0f)), "Ghost is not at the expected position after moving away");
            Assert.That(session.Doors[0].Door.IsOpen, Is.False, "Door did not close when ghost moved away");
        }

     

        private static PuzzleSession_OLD CreateSession(
            float moveSpeed = 1f,
            Vector3? startPosition = null,
            int maxGhost = 1)
        {
            var setting = new PuzzleSessionConfig(
                moveSpeed: moveSpeed,
                startPosition: startPosition ?? Vector3.zero,
                maxGhost: maxGhost,
                doors: new List<DoorConfig>(),
                buttons: new List<ButtonConfig>()
            );

            return new PuzzleSession_OLD(setting);
        }

        private static GhostModel CreateGhost(
            float moveSpeed = 1f,
            Vector3? startPosition = null)
        {
            var ghost = new GhostModel(moveSpeed);
            ghost.SetPosition(startPosition ?? Vector3.zero);
            return ghost;
        }

        /// <summary>
        /// float 오차를 인정하는 검증 함수
        /// </summary>
        private static void AssertPositionEquals(Vector3 actual, Vector3 expected, float tolerance = 0.0001f)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(tolerance));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(tolerance));
        }

        private static PlayerModel_OLD CreatePlayer(
            float moveSpeed = 1f,
            Vector3? moveDirection = null,
            Vector3? startPosition = null,
            SnapshotLogger logger = null)
        {
            var startPos = startPosition ?? Vector3.zero;
            moveDirection ??= Vector3.right;

            var player = new PlayerModel_OLD(startPos, moveSpeed, logger);
            player.SetMoveDirection(moveDirection ?? Vector3.forward);

            return player;
        }

        private static void AdvanceTicks(ITickable entity, int ticks)
        {
            for (int i = 0; i < ticks; ++i)
            {
                entity.Step();
            }
        }

    }
}