using NUnit.Framework;
using System;
using System.Collections.Generic;


namespace SamplePuzzle.Tests
{
    public class CollisionWorldTest
    {

        [Test]
        public void AABB_WhenAABBsOverlap_ShouldReturnTrue()
        {
            var aabb1 = new AABB(new FixVector2(0, 0), new FixVector2(1, 1));
            var aabb2 = new AABB(new FixVector2(0.5f, 0.5f), new FixVector2(1.5f, 1.5f));
            Assert.That(aabb1.Intersects(aabb2), Is.True);
        }


        [TestCase(7, 1.5, 2.5, 0.25, 0.125, 3.25, 3.375)]
        [TestCase(0, 1.5, 2.5, 0.25, 0.125, 1.5, 2.5)]       // 0틱: 움직이지 않음
        [TestCase(4, 0.0, 0.0, -0.5, 0.75, -2.0, 3.0)]       // 음수 방향
        public void DynamicBody_AfterMultipleTicks_ReachesExpectedPosition(int ticks, double startX, double startY, double displacementX, double displacementY, double expectedX, double expectedY)
        {
            var displacement = new FixVector2(displacementX, displacementY);
            var expected = new FixVector2(expectedX, expectedY);

            var world = new CollisionWorld();
            var handle = world.CreateDynamicBody(
                new(startX, startY),
                FixVector2.Zero,
                FixVector2.Zero);

            for (int i = 0; i < ticks; ++i)
            {
                handle.RequestMove(displacement);
                world.ResolveMove();
            }

            Assert.That(handle.Position, Is.EqualTo(expected));
        }

        [Test(Description = "사실 이 테스트는 배치에 대해 이동과 차이를 제대로 보여주지 못한다.")]
        public void Place_ToEmptySpace_IgnoresWall()
        {
            var world = new CollisionWorld();
            var player = TestUtils.CreateDynamicBody(world);
            TestUtils.CreateStaticBody(world, position: new FixVector2(1, 0));

            // 벽 너머 공간에 배치 
            var targetPosition = new FixVector2(2, 0);
            world.Place(player, targetPosition);

            Assert.That(player.Position, Is.EqualTo(targetPosition));
        }

        [Test]
        public void Place_ToOccupiedPlace_Throws()
        {
            var world = new CollisionWorld();
            var player = TestUtils.CreateDynamicBody(world);
            TestUtils.CreateStaticBody(world, position: new FixVector2(1, 0));

            // 벽에 배치
            var targetPosition = new FixVector2(1, 0);
            Assert.That(() => world.Place(player, targetPosition), Throws.ArgumentException);
        }

        [Test]

        public void Place_NotExsistObject_Throws()
        {
            var worldA = new CollisionWorld();
            IDynamicBodyHandle fake = worldA.CreateDynamicBody(FixVector2.Zero, FixVector2.Zero, FixVector2.Zero);

            var worldB = new CollisionWorld();

            Assert.That(() => worldB.Place(fake, FixVector2.Zero), Throws.ArgumentException);
        }

        [TestCase(1, 0)]
        [TestCase(-1, 0)]
        [TestCase(0, 1)]
        [TestCase(0, -1)] // 이동량이 2 이상이라면 현재 구현 상 벽과 겹치지를 않아서 터널링이 발생, 막히지가 않음.
        public void WhenPlayerTouchingWallMovesIntoIt_PlayerShouldStayInPlace(int dx, int dy)
        {
            var world = new CollisionWorld();
            var body = TestUtils.CreateDynamicBody(world);

            // 이동 방향 쪽에 벽 배치
            var dir = new FixVector2(Math.Sign(dx), Math.Sign(dy));
            TestUtils.CreateStaticBody(world, position: dir);

            body.RequestMove(new FixVector2(dx, dy));
            world.ResolveMove();

            Assert.That(body.Position, Is.EqualTo(FixVector2.Zero));
        }

        [TestCase(2, 0)]
        [TestCase(-2, 0)]
        [TestCase(0, 2)]
        [TestCase(0, -2)]
        public void WhenPlayerMovesIntoWallFast_PlayerShouldMoveThroughWall(int dx, int dy)
        {
            var world = new CollisionWorld();
            var body = TestUtils.CreateDynamicBody(world);
            var expected = new FixVector2(dx, dy);
            // 이동 방향 쪽에 벽 배치
            var dir = new FixVector2(Math.Sign(dx), Math.Sign(dy));
            TestUtils.CreateStaticBody(world, position: dir);

            body.RequestMove(new FixVector2(dx, dy));
            world.ResolveMove();

            Assert.That(body.Position, Is.EqualTo(expected));
        }

        [TestCase(1, 0, 1, 0)]
        [TestCase(-3, 0, -3, 0)]
        [TestCase(0, 4, 0, 4)]
        [TestCase(0, -5, 0, -5)]
        public void WhenPlayerTouchingWallMovesAlongIt_PlayerShouldNotBeBlocked(int dx, int dy, int expectedX, int expectedY)
        {
            var world = new CollisionWorld();
            var body = TestUtils.CreateDynamicBody(world);

            var dir = new FixVector2(Math.Sign(dx), Math.Sign(dy));

            // 한 축으로만 이동한다고 가정하여 이동 방향을 제외한 모든 곳을 막음
            for (int x = -1; x <= 1; ++x)
            {
                for (int y = -1; y <= 1; ++y)
                {
                    if (x == 0 && y == 0) continue;
                    if (dir.X == Fix64.FromInt(x) && dir.Y == Fix64.FromInt(y)) continue;

                    TestUtils.CreateStaticBody(world, position: new FixVector2(x, y));
                }
            }

            var expected = new FixVector2(expectedX, expectedY);

            body.RequestMove(new FixVector2(dx, dy));
            world.ResolveMove();

            Assert.That(body.Position, Is.EqualTo(expected));
        }

        [TestCase(2, 0, 1, 0, TestName = "Right_StopsAtLeftEdge")]
        [TestCase(-2, 0, -1, 0, TestName = "Left_StopsAtRightEdge")]
        [TestCase(0, 2, 0, 1, TestName = "Up_StopsAtBottomEdge")]
        [TestCase(0, -2, 0, -1, TestName = "Down_StopsAtTopEdge")]
        public void WhenPlayerMovesIntoClosedWall_PlayerShouldNotPassThrough(int dx, int dy, int expectedX, int expectedY)
        {
            var world = new CollisionWorld();
            var body = TestUtils.CreateDynamicBody(world);

            // 이동 방향 쪽에 벽 배치
            TestUtils.CreateStaticBody(world, position: new FixVector2(dx, dy));

            var expected = new FixVector2(expectedX, expectedY);

            body.RequestMove(new FixVector2(dx, dy));
            world.ResolveMove();

            Assert.That(body.Position, Is.EqualTo(expected));
        }

        [Test]

        public void WhenPlayerOverlapsButton_ReturnsButtonBody()
        {
            var world = new CollisionWorld();
            var player = TestUtils.CreateDynamicBody(world);
            var button = TestUtils.CreateTriggerBody(world);

            var result = new List<IBodyHandle>();
            world.CheckOverlaps(result);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result, Does.Contain(button));
        }

        [Test]
        public void WhenPlayerOutOfButton_DoesNotReturnButtonBody()
        {
            var world = new CollisionWorld();
            var player = TestUtils.CreateDynamicBody(world);
            var button = TestUtils.CreateTriggerBody(world);
            var result = new List<IBodyHandle>();

            world.CheckOverlaps(result);
            Assert.That(result.Count, Is.EqualTo(1));

            player.RequestMove(FixVector2.Right * Fix64.FromInt(2));
            world.ResolveMove();

            world.CheckOverlaps(result);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void WhenWallOverlapsButton_NotReturnsButtonBody()
        {
            var world = new CollisionWorld();
            var wall = TestUtils.CreateStaticBody(world);
            var button = TestUtils.CreateTriggerBody(world);
            var result = new List<IBodyHandle>();

            world.CheckOverlaps(result);

            Assert.That(result.Count, Is.EqualTo(0));
        }
    }
}
