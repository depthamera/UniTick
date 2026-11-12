namespace SamplePuzzle
{
    public interface IDynamicBodyHandle : IBodyHandle
    {
        public void RequestMove(FixVector2 displacement);
    }
}
