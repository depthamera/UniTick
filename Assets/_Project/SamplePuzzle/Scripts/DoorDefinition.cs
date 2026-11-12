namespace SamplePuzzle
{
    public readonly struct DoorDefinition   
    {
        public FixVector2 Position { get; }
        public FixVector2 Size { get; }
        public SimId Id { get; }
        public SimId OpenedBy { get; }
        public DoorDefinition(FixVector2 position, FixVector2 size, SimId id, SimId openedBy)
        {
            Position = position;
            Size = size;
            Id = id;
            OpenedBy = openedBy;
        }
    }
}
