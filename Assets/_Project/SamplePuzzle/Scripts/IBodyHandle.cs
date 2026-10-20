using UnityEngine;

namespace SamplePuzzle
{
    public interface IBodyHandle
    {
        public Vector2 Position { get; }

        public void RequestMove(Vector2 displacement);
    }
}
