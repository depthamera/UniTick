namespace SamplePuzzle
{
    internal sealed class FixedBody : IBodyHandle
    {
        public FixVector2 Position { get; internal set; }
        internal AABB Bounds;
        internal FixVector2 PendingMove;

        public FixedBody(FixVector2 position, AABB bounds)
        {
            Position = position;
            Bounds = bounds;
        }
    }
}
