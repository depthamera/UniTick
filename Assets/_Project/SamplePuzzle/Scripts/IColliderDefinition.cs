namespace SamplePuzzle
{
    public interface IColliderDefinition
    {
        public string DefinitionId { get; }
        public AABB AABB { get; }
    }
}
