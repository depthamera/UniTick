namespace SamplePuzzle
{
    public readonly struct ButtonDefinition
    {
        public FixVector2 Position { get; }
        public FixVector2 Size { get; }
        public SimId Id { get; }
        public ButtonDefinition(FixVector2 position, FixVector2 size, SimId id)
        {
            Position = position;
            Size = size;
            Id = id;
        }
    }
}
