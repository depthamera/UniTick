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
}
