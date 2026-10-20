using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SamplePuzzle.Tests
{

    // TODO: 버튼-문 시뮬레이션에 대해 세션 기반으로 확장
    public class SamplePuzzleTest
    {
        [Test]
        public void Player_AfterMultipleTicks_ReachesExpectedPosition()
        {
            int ticks = 42;

            var moveSpeed = 3f;
            var moveDirection = new Vector3(0.3f, 0.1f, 0.15f).normalized;
            var startPosition = Vector3.zero;
            var expectedPosition = startPosition + moveDirection * (moveSpeed * ticks);
            PlayerModel_OLD player = CreatePlayer(moveSpeed, moveDirection, startPosition);

            AdvanceTicks(player, ticks);

            AssertPositionEquals(player.Position, expectedPosition);
        }

        [Test]
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
        public void PuzzleSession_AfterLoop_ResetsPlayerToSpawn()
        {
            var startPosition = Vector3.zero;

            var session = CreateSession(startPosition: startPosition);
            session.AdvanceTicks(10);

            session.TriggerLoop();

            AssertPositionEquals(session.Player.Position, startPosition);
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
        public void AABB_WhenAABBsOverlap_ShouldReturnTrue()
        {
            var aabb1 = new AABB(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
            var aabb2 = new AABB(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1.5f, 1.5f, 1.5f));
            Assert.That(aabb1.Intersects(aabb2), Is.True);
        }

        [Test]
        public void CollisionWorld_WhenEntitiesOverlap_ShouldNotifyBothEntities()
        {
            var entity1 = new FakeCollisionEntity(new AABB(new Vector3(0, 0, 0), new Vector3(1, 1, 1)));
            var entity2 = new FakeCollisionEntity(new AABB(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1.5f, 1.5f, 1.5f)));
            var system = new CollisionWorld_OLD();

            system.AddEntity(entity1);
            system.AddEntity(entity2);
            system.Step();

            Assert.That(entity1.OverlapCount, Is.EqualTo(1));
            Assert.That(entity2.OverlapCount, Is.EqualTo(1));
            Assert.That(entity1.LastOverlappedWith, Is.SameAs(entity2));
            Assert.That(entity2.LastOverlappedWith, Is.SameAs(entity1));

        }

        [Test]
        public void CollisionWorld_WhenEntityMovesIntoAnother_TriggersOverlap()
        {
            var player = new FakeMovingEntity(position: Vector3.zero, localSize: new Vector3(1, 1, 1), offset: new Vector3(0.5f, 0.5f, 0.5f));
            var obstacle = new FakeMovingEntity(position: new Vector3(2, 0, 0), localSize: new Vector3(1, 1, 1), offset: new Vector3(0.5f, 0.5f, 0.5f));

            var system = new CollisionWorld_OLD();

            system.AddEntity(player);
            system.AddEntity(obstacle);

            system.Step();

            Assert.That(player.OverlapCount, Is.EqualTo(0));
            Assert.That(obstacle.OverlapCount, Is.EqualTo(0));

            player.Position = new Vector3(1.7f, 0, 0);

            system.Step();

            Assert.That(player.OverlapCount, Is.EqualTo(1));
            Assert.That(obstacle.OverlapCount, Is.EqualTo(1));
            Assert.That(player.LastOverlappedWith, Is.SameAs(obstacle));
            Assert.That(obstacle.LastOverlappedWith, Is.SameAs(player));
        }

        [Test]
        public void CollisionWorld_WhenEntityNotMovesIntoAnother_DoesNotTriggerOverlap ()
        {
            var player = new FakeMovingEntity(position: Vector3.zero, localSize: new Vector3(1, 1, 1), offset: new Vector3(0.5f, 0.5f, 0.5f));
            var obstacle = new FakeMovingEntity(position: new Vector3(2, 0, 0), localSize: new Vector3(1, 1, 1), offset: new Vector3(0.5f, 0.5f, 0.5f));

            var system = new CollisionWorld_OLD();

            system.AddEntity(player);
            system.AddEntity(obstacle);

            system.Step();

            Assert.That(player.OverlapCount, Is.EqualTo(0));
            Assert.That(obstacle.OverlapCount, Is.EqualTo(0));

            player.Position = new Vector3(1f, 0, 0);

            system.Step();

            Assert.That(player.OverlapCount, Is.EqualTo(0));
            Assert.That(obstacle.OverlapCount, Is.EqualTo(0));
            Assert.That(player.LastOverlappedWith, Is.Null);
            Assert.That(obstacle.LastOverlappedWith, Is.Null);
        }


        [Test]
        public void PuzzleButton_WhenPressed_ShouldOpenDoor()
        {
            var door = new PuzzleDoor();
            var button = new PuzzleButton(door);
            button.Press();
            Assert.That(door.IsOpen, Is.True);
        }


        [Test]
        public void CollisionWorld_WhenPlayerMovesIntoButton_ShouldOpenDoor()
        {
            var door = new PuzzleDoor();
            var button = new PuzzleButton(door);
            var border = new AABB(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
            var physicalButton = new PhysicalButton(button: button, border: border, position: new Vector3(5f, 0f, 0f));
            var player = new FakeMovingEntity(position: Vector3.zero, localSize: new Vector3(1, 1, 1), offset: new Vector3(0.5f, 0.5f, 0.5f));
            var collisionWorld = new CollisionWorld_OLD();

            collisionWorld.AddEntity(player);
            collisionWorld.AddEntity(physicalButton);
            collisionWorld.Step();

            Assert.That(door.IsOpen, Is.False);
            player.Position = new Vector3(5f, 0f, 0f);

            collisionWorld.Step();
            Assert.That(door.IsOpen, Is.True);
        }

        [Test]
        public void CollisionWorld_WhenPlayerGetsOutOfButton_ShouldCloseDoor()
        {
            var door = new PuzzleDoor();
            var button = new PuzzleButton(door);
            var border = new AABB(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
            var physicalButton = new PhysicalButton(button: button, border: border, position: new Vector3(5f, 0f, 0f));
            var player = new FakeMovingEntity(position: Vector3.zero, localSize: new Vector3(1, 1, 1), offset: new Vector3(0.5f, 0.5f, 0.5f));

            var collisionWorld = new CollisionWorld_OLD();
            collisionWorld.AddEntity(player);
            collisionWorld.AddEntity(physicalButton);

            collisionWorld.Step();
            Assert.That(door.IsOpen, Is.False);

            player.Position = new Vector3(5f, 0f, 0f);
            collisionWorld.Step();
            Assert.That(door.IsOpen, Is.True);

            player.Position = new Vector3(6f, 0f, 0f);
            collisionWorld.Step();
            Assert.That(door.IsOpen, Is.False);
        }

        [Test]
        public void PuzzleSession_WhenPlayerMovesIntoButton_ShouldOpenDoor()
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


            var session = new PuzzleSession(config);


            session.SetPlayerMove(Vector3.right);
            session.AdvanceTicks(2);

            Assert.That(session.Doors[0].Door.IsOpen, Is.True, 
                $"Player position: {session.Player.Position}\n" +
                $"Door position: {session.Doors[0].Position}\n" +
                $"Button position: {session.Buttons[0].Position}\n" +
                $"Player Collider: {(session.Player as ICollidable_OLD)?.Bounds.Min}, {(session.Player as ICollidable_OLD)?.Bounds.Max}\n" +
                $"Button Collider: {(session.Buttons[0] as ICollidable_OLD)?.Bounds.Min}, {(session.Buttons[0] as ICollidable_OLD)?.Bounds.Max}");
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


            var session = new PuzzleSession(config);

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

            var session = new PuzzleSession(config);

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
            var session = new PuzzleSession(config);

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
            var session = new PuzzleSession(config);

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


        [Ignore("아직 충돌에 관한 부분이 이 테스트를 클리어하기에 부적절함")]
        [Test]
        public void PuzzleSession_WhenPlayerMovesIntoClosedDoor_PlayerShouldNotPassThrough()
        {
            var config = new PuzzleSessionConfig(
                moveSpeed: 1f,
                startPosition: Vector3.zero,
                maxGhost: 1,
                doors: new List<DoorConfig>
                {
                    new DoorConfig(position: new Vector3(5f, 0f, 0f)),
                },
                buttons: null
            );
            var session = new PuzzleSession(config);

            // Player moves towards the closed door
            session.SetPlayerMove(Vector3.right);
            session.AdvanceTicks(6); // Move past the door position
            // The player should not have passed through the closed door
            Assert.That(session.Player.Position.x, Is.EqualTo(4f), "Player passed through the closed door");
        }

        [Test]
        public void CollisionWorld_WhenPlayerMovesIntoClosedDoor_PlayerShouldNotPassThrough()
        {
            var world = new CollisionWorld();

            var player = new PlayerModel();
            var playerBodyHandle = world.CreateBody(
                owner: player,
                position: new Vector2(0f, 0f), 
                size: new Vector2(1f, 1f), 
                offset: new Vector2(0f, -0.5f));

            var door = new DoorModel();
            world.CreateBody(
                owner: door,
                position: new Vector2(2, 0f), 
                size: new Vector2(1f, 1f), 
                offset: Vector2.zero);

            playerBodyHandle.RequestMove(displacement: new Vector2(2f, 0f));
            world.Resolve();

            Assert.That(playerBodyHandle.Position.x, Is.EqualTo(1f));
        }

        
        private static PuzzleSession CreateSession(
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

            return new PuzzleSession(setting);
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