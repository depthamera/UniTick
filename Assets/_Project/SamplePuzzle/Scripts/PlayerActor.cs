namespace SamplePuzzle
{
    public class PlayerActor : ICollidable
    {
        public Fix64 Speed { get; }

        public PlayerActor(Fix64 speed)
        {
            Speed = speed;
        }
    }
}
