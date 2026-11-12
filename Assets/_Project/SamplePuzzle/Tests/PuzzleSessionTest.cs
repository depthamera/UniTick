using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace SamplePuzzle.Tests
{
    public class PuzzleSessionTest
    {
        [Test]
        public void Step_MovesPlayerByCommand()
        {
            var pendingCommands = new GameCommand[]
            {
                GameCommand.Right,
                GameCommand.Right,
                GameCommand.Up,
                GameCommand.Left,
                GameCommand.Down

            };
            var expectedTicks = pendingCommands.Length;

            var startPosition = new FixVector2(1, 1);
            var moveSpeed = Fix64.One;

            var trajectory = new List<FixVector2>() { startPosition };
            var expectedTrajectory = new List<FixVector2>()
            {
                startPosition,
                startPosition + FixVector2.Right * moveSpeed,
                startPosition + FixVector2.Right * moveSpeed + FixVector2.Right * moveSpeed,
                startPosition + FixVector2.Right * moveSpeed + FixVector2.Right * moveSpeed + FixVector2.Up * moveSpeed,
                startPosition + FixVector2.Right * moveSpeed + FixVector2.Right * moveSpeed + FixVector2.Up * moveSpeed + FixVector2.Left * moveSpeed,
                startPosition + FixVector2.Right * moveSpeed + FixVector2.Right * moveSpeed + FixVector2.Up * moveSpeed + FixVector2.Left * moveSpeed + FixVector2.Down * moveSpeed
            };

            var session = TestUtils.CreateSession(startPosition: startPosition, moveSpeed: moveSpeed);
            foreach (var command in pendingCommands)
            {
                session.Step(command);
                trajectory.Add(session.PlayerPosition);
            }

            Assert.That(session.CurrentTick, Is.EqualTo(expectedTicks));
            Assert.That(trajectory, Is.EqualTo(expectedTrajectory));
        }

        [TestCase(42)]
        public void Step_PlayerStaysWithNoCommand(int ticks)
        {
            var session = TestUtils.CreateSession();
            var expected = Enumerable.Repeat(session.PlayerPosition, ticks).ToList();
            var trajectory = new List<FixVector2>();

            for (int i = 0; i < ticks; ++i)
            {
                session.Step();
                trajectory.Add(session.PlayerPosition);
            }

            Assert.That(trajectory, Is.EqualTo(expected));
        }

        [Test]
        public void Step_PlayerBlockedByWall()
        {
            var session = TestUtils.CreateSession(moveSpeed: Fix64.FromDouble(0.125));
            var trajectory = new List<FixVector2>();

            session.SpawnWall(
                position: new FixVector2(1.5, 0),
                size: FixVector2.One);


            for (int i = 0; i < 100; ++i)
            {
                session.Step(GameCommand.Right);
                trajectory.Add(session.PlayerPosition);
            }

            Assert.That(trajectory.Select(t => t.X).ToArray(), Has.All.LessThanOrEqualTo(Fix64.FromDouble(0.5)));
            Assert.That(session.PlayerPosition, Is.EqualTo(new FixVector2(0.5, 0)));
        }

        [TestCase(1, 1)]
        [TestCase(0, 0)]
        [TestCase(-1, -1)]
        [TestCase(256, -256)]
        public void TriggerLoop_ResetsPlayerPosition(int startX, int startY)
        {
            var startPosition = new FixVector2(startX, startY);
            var session = TestUtils.CreateSession(startPosition: startPosition, moveSpeed: Fix64.One);

            session.Step(GameCommand.Right);
            session.Step(GameCommand.Up);

            Assert.That(session.PlayerPosition, Is.Not.EqualTo(startPosition), "플레이어가 이동한 적이 없기에 판단 불가능");

            session.TriggerLoop();
            Assert.That(session.PlayerPosition, Is.EqualTo(startPosition));
        }

        [Test]
        public void PlayerMovesIntoButton_DoorOpens()
        {
            var session = TestUtils.CreateSession(moveSpeed: Fix64.FromDouble(0.5), spawnPlayer: false);

            var button = session.SpawnButton(position: new FixVector2(1, 1), size: FixVector2.One);
            var door = session.SpawnDoor(position: new FixVector2(10, 10), size: FixVector2.One, openedBy: button);
            var player = session.SpawnPlayer(position: FixVector2.Zero, size: FixVector2.One);


            session.Step(GameCommand.Right);
            session.Step(GameCommand.Right);
            session.Step(GameCommand.Up);
            session.Step(GameCommand.Up);

            Assert.That(session.IsOpen(door), Is.True);
        }

        [Test]
        public void PlayerMovesOutOfButton_DoorCloses()
        {
            var session = TestUtils.CreateSession(moveSpeed: Fix64.FromInt(2), spawnPlayer: false);
            var button = session.SpawnButton(position: FixVector2.Zero, size: FixVector2.One);
            var door = session.SpawnDoor(position: new FixVector2(10, 10), size: FixVector2.One, openedBy: button);
            var player = session.SpawnPlayer(position: FixVector2.Zero, size: FixVector2.One);

            // 기본 상태에서 문이 열려있음을 확인
            session.Step();
            Assert.That(session.IsPressed(button), Is.True);
            Assert.That(session.IsOpen(door), Is.True);

            session.Step(GameCommand.Right);
            session.Step(GameCommand.Up);
            Assert.That(session.PlayerPosition, Is.EqualTo(new FixVector2(2, 2)));
            Assert.That(session.IsPressed(button), Is.False, "Button should not be pressed after player moves away");
            Assert.That(session.IsOpen(door), Is.False, "Door should be closed after player moves away");

        }

        // WIP: 루프에 대한 테스트. 상태의 초기화 + 결정론적 아이디

        [Test]
        public void TriggerLoop_ResetsButtonAndDoorStates()
        {
            var session = TestUtils.CreateSession(moveSpeed: Fix64.FromDouble(1), spawnPlayer: false);
            var button = session.SpawnButton(position: new FixVector2(1, 1), size: FixVector2.One);
            var door = session.SpawnDoor(position: new FixVector2(10, 10), size: FixVector2.One, openedBy: button);
            var player = session.SpawnPlayer(position: FixVector2.Zero, size: FixVector2.One);

            // 플레이어가 버튼을 눌러 문이 열리도록 이동
            session.Step(GameCommand.Right);
            session.Step(GameCommand.Up);

            Assert.That(session.IsPressed(button), Is.True, "Button should be pressed after player moves onto it");
            Assert.That(session.IsOpen(door), Is.True, "Door should be open after player presses the button");

            // 루프 트리거 후 상태 초기화 확인
            session.TriggerLoop();
            Assert.That(session.IsPressed(button), Is.False, "Button should not be pressed after loop trigger");
            Assert.That(session.IsOpen(door), Is.False, "Door should be closed after loop trigger");
        }

        [Test]
        public void TriggerLoop_EachObjectsHasSameIdAfterLoop()
        {
            var builder = new SessionBuilder();
            var player = builder.CreatePlayer();
            var button = builder.CreateButton(position: new FixVector2(1, 0));
            var door = builder.CreateDoor(position: new FixVector2(10, 0), openedBy: button);
            var session = builder.Build();

            var buttonIdBeforeLoop = button;
            var doorIdBeforeLoop = door;
            var playerIdBeforeLoop = player;

            session.TriggerLoop();

            var buttonIdAfterLoop = session.Buttons.First().Id;
            var doorIdAfterLoop = session.Doors.First().Id;
            var playerIdAfterLoop = session.Player.Id;

            Assert.That(buttonIdBeforeLoop, Is.EqualTo(buttonIdAfterLoop), "Button ID should remain the same after loop");
            Assert.That(doorIdBeforeLoop, Is.EqualTo(doorIdAfterLoop), "Door ID should remain the same after loop");
            Assert.That(playerIdBeforeLoop, Is.EqualTo(playerIdAfterLoop), "Player ID should remain the same after loop");
        }

        [Test]
        public void TriggerLoopAfterPlayerMoves_RecordsPlayerMovement()
        {
            var session = TestUtils.CreateSession(moveSpeed: Fix64.FromDouble(0.5), spawnPlayer: false);
            var player = session.SpawnPlayer(position: FixVector2.Zero, size: FixVector2.One);

            var expected = new List<FixVector2>()
            {
                FixVector2.Right * Fix64.FromDouble(0.5),
                FixVector2.Up * Fix64.FromDouble(0.5)
            };

            session.Step(GameCommand.Right);
            session.Step(GameCommand.Up);
            session.TriggerLoop();

            Assert.That(session.GetMoveRecord(0), Is.EqualTo(expected));
        }

        [Test]
        public void AfterLoop_GhostMovesAccordingToPreviousPlayerMovement()
        {
            var session = TestUtils.CreateSession(moveSpeed: Fix64.FromDouble(0.5), spawnPlayer: false);
            var player = session.SpawnPlayer(position: FixVector2.Zero, size: FixVector2.One);

            var expected = new List<FixVector2>()
            {
                FixVector2.Right * Fix64.FromDouble(0.5),
                FixVector2.Right * Fix64.FromDouble(0.5) + FixVector2.Up * Fix64.FromDouble(0.5)
            };

            var trajectory = new List<FixVector2>();


            session.Step(GameCommand.Right);
            session.Step(GameCommand.Up);
            session.TriggerLoop();

            session.Step();
            trajectory.Add(session.GetGhostPosition(sourceLoopIndex: 0));

            session.Step();
            trajectory.Add(session.GetGhostPosition(sourceLoopIndex: 0));

            Assert.That(trajectory, Is.EqualTo(expected));
        }

        [Test]
        public void GhostStopsMovingAfterItsTimelineEnds()
        {
            var session = TestUtils.CreateSession(moveSpeed: Fix64.FromDouble(0.5), spawnPlayer: false);
            var player = session.SpawnPlayer(position: FixVector2.Zero, size: FixVector2.One);
            session.Step(GameCommand.Right);
            session.Step(GameCommand.Up);
            session.TriggerLoop();

            // 고스트의 이동이 끝난 후 몇 틱 더 진행
            for (int i = 0; i < 5; i++)
            {
                session.Step();
            }

            // 고스트의 위치가 마지막 위치와 동일한지 확인
            var ghostPosition = session.GetGhostPosition(sourceLoopIndex: 0);
            Assert.That(ghostPosition, Is.EqualTo(new FixVector2(0.5, 0.5)), "Ghost should remain at its last position after its timeline ends");
        }

        [Test]
        public void ExceedingMaxGhosts_RemovesOldestGhost()
        {
            int maxGhosts = 3;
            var session = TestUtils.CreateSession(moveSpeed: Fix64.FromDouble(0.5), spawnPlayer: false, maxGhosts: maxGhosts);
            var player = session.SpawnPlayer(position: FixVector2.Zero, size: FixVector2.One);
            // 플레이어 이동 및 루프 트리거
            for (int i = 0; i < maxGhosts + 5; i++)
            {
                session.Step(GameCommand.Right);
                session.TriggerLoop();
            }
            // 고스트 수 확인
            Assert.That(session._ghosts.Count, Is.EqualTo(maxGhosts), "Should only keep the maximum number of ghosts");
            // 가장 오래된 고스트가 제거되었는지 확인
            var oldestGhostIndex = session.OldestGhostLoopIndex;
            Assert.That(oldestGhostIndex, Is.EqualTo(5), "Oldest ghost should be removed when exceeding max ghosts");
        }


        [Test]
        public void GhostPressesButton_OpensDoor()
        {
            var session = TestUtils.CreateSession(moveSpeed: Fix64.FromDouble(1), spawnPlayer: false);
            var button = session.SpawnButton(position: new FixVector2(1, 1), size: FixVector2.One);
            var door = session.SpawnDoor(position: new FixVector2(10, 10), size: FixVector2.One, openedBy: button);
            var player = session.SpawnPlayer(position: FixVector2.Zero, size: FixVector2.One);

            // 플레이어가 버튼을 누르고 문이 열리도록 이동
            session.Step(GameCommand.Right);
            session.Step(GameCommand.Up);
            session.TriggerLoop();

            // 초기화 후에는 버튼이 눌리지 않은 상태임을 확인
            Assert.That(session.IsPressed(button), Is.False, "Button should not be pressed initially");
            Assert.That(session.IsOpen(door), Is.False, "Door should be closed initially");

            session.Step();
            session.Step();

            Assert.That(session.IsPressed(button), Is.True, "Ghost should press the button");
            Assert.That(session.IsOpen(door), Is.True, "Door should be open when ghost presses the button");
        }

        // TODO: 고스트가 버튼을 누르고 문을 열어 플레이어가 이동할 수 있는지 확인하는 테스트

        [Test]
        public void Constructor_InitializesAccordingToDefinition()
        {
            var definition = new LevelDefinition(
                startPosition: new FixVector2(1, 1),
                moveSpeed: Fix64.FromDouble(0.5),
                maxGhosts: 5,
                walls: new List<WallDefinition>()
                {
                    new WallDefinition(new FixVector2(0, 0), new FixVector2(1, 1)),
                    new WallDefinition(new FixVector2(10, 10), new FixVector2(3, 3))
                },
                buttons: new List<ButtonDefinition>() {
                    new ButtonDefinition(new FixVector2(2, 2), new FixVector2(1, 1), new SimId(2)),
                    new ButtonDefinition(new FixVector2(5, 5), new FixVector2(1, 1), new SimId(3))
                },
                doors: new List<DoorDefinition>()
                {
                    new DoorDefinition(new FixVector2(10, 10), new FixVector2(1, 1), new SimId(4), new SimId(2)),
                    new DoorDefinition(new FixVector2(15, 15), new FixVector2(1, 1), new SimId(5), new SimId(3))
                });

            var session = new PuzzleSession(definition);

            Assert.That(session.Buttons.Count, Is.EqualTo(definition.Buttons.Count), "Buttons count should match definition");
            Assert.That(session.Doors.Count, Is.EqualTo(definition.Doors.Count), "Doors count should match definition");    

            Assert.That(session.Buttons.Select(b => b.Id).ToList(), Is.EqualTo(definition.Buttons.Select(b => b.Id).ToList()), "Button IDs should match definition");
            Assert.That(session.Doors.Select(d => d.Id).ToList(), Is.EqualTo(definition.Doors.Select(d => d.Id).ToList()), "Door IDs should match definition");
        }

        // TODO: 생성자를 def 기반으로 마이그레이션
    }
}
