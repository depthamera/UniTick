using UnityEngine;


namespace SamplePuzzle 
{ 
    public struct MoveSnapshot
    {
        public Vector3 Position;
        public Vector3 MoveDirection;

        public MoveSnapshot(Vector3 position, Vector3 moveDirection) 
        {
            Position = position;
            MoveDirection = moveDirection;
        }
    }
}
