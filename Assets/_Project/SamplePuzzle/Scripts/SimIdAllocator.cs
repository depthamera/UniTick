using System;

namespace SamplePuzzle
{
    public sealed class SimIdAllocator
    {
        private int _next = 0;
        public SimId Next() => new(_next++);
        public void Reserve(SimId id) => _next = Math.Max(_next, id.Value + 1);
        public void Reset() => _next = 0;
    }
}
