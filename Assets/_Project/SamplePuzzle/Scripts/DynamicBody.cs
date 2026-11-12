namespace SamplePuzzle
{
    internal sealed class DynamicBody : IDynamicBodyHandle
    {
        public FixVector2 Position { get; internal set; }
        public AABB Bounds { get; }
        public FixVector2 PendingMove { get; internal set; }

        public DynamicBody(FixVector2 position, AABB bounds)
        {
            Position = position;
            Bounds = bounds;
        }

        public void RequestMove(FixVector2 displacement)
        {
            PendingMove = displacement;
        }
    }
}
