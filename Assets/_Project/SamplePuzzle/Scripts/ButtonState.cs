namespace SamplePuzzle
{
    public struct ButtonState
    {
        public SimId Id;
        public bool Pressed;

        public ButtonState(SimId id)
        {
            Id = id;
            Pressed = false;
        }
    }
}
