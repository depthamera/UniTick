namespace SamplePuzzle
{
    public readonly struct GhostState
    {
        public int LoopIndex { get; }
        public IDynamicBodyHandle BodyHandle { get; }

        public GhostState(int loopIndex, IDynamicBodyHandle bodyHandle)
        {
            LoopIndex = loopIndex;
            BodyHandle = bodyHandle;
        }
    }
}
