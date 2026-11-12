namespace SamplePuzzle
{
    public readonly struct WallDefinition
    {
        public FixVector2 Position { get; }
        public FixVector2 Size { get; }
        public WallDefinition(FixVector2 position, FixVector2 size)
        {
            Position = position;
            Size = size;
        }
    }
}
