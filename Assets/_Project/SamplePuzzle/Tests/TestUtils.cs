using System;
using System.Drawing;
using System.Security.Policy;

namespace SamplePuzzle.Tests
{
    public static class TestUtils
    {
        public static ulong Mix(ulong hash, long value)
        {
            unchecked { return (hash ^ (ulong)value) * 1099511628211UL; }
        }

        public struct XorShift64
        {
            private ulong state;
            public XorShift64(ulong seed) => state = seed;  // seed는 0이 아니어야 함

            public ulong Next()
            {
                state ^= state << 13;
                state ^= state >> 7;
                state ^= state << 17;
                return state;
            }
        }

        /// <summary>      
        /// 기본적으로 영벡터 위치에 크기가 1인 동적 오브젝트를 생성
        /// </summary>
        internal static IDynamicBodyHandle CreateDynamicBody(
            CollisionWorld world,
            FixVector2? position = null,
            FixVector2? size = null,
            FixVector2? offset = null)
        {

            return world.CreateDynamicBody(
                position ?? FixVector2.Zero,
                size ?? FixVector2.One,
                offset ?? FixVector2.Zero);
        }

        /// <summary>      
        /// 기본적으로 영벡터 위치에 크기가 1인 정적 오브젝트를 생성
        /// </summary>

        internal static IBodyHandle CreateStaticBody(
            CollisionWorld world,
            FixVector2? position = null,
            FixVector2? size = null,
            FixVector2? offset = null)
        {
            return world.CreateFixedBody(
                position ?? FixVector2.Zero,
                size ?? FixVector2.One,
                offset ?? FixVector2.Zero);
        }

        internal static PuzzleSession CreateSession(
            FixVector2? startPosition = null,
            Fix64? moveSpeed = null,
            bool spawnPlayer = true,
            int maxGhosts = 5)
        {
            var definition = new LevelDefinition(
                startPosition ?? FixVector2.Zero,
                moveSpeed ?? Fix64.One,
                maxGhosts,
                null,
                null,
                null);

            var session = new PuzzleSession(definition);

            if (spawnPlayer) session.SpawnPlayer(
                position: startPosition ?? FixVector2.Zero,
                FixVector2.One);

            return session;
        }

        internal static IBodyHandle CreateTriggerBody(
            CollisionWorld world,
            FixVector2? position = null,
            FixVector2? size = null,
            FixVector2? offset = null)
        {
            return world.CreateTriggerBody(
                position ?? FixVector2.Zero,
                size ?? FixVector2.One,
                offset ?? FixVector2.Zero);
        }
    }
}