namespace SamplePuzzle 
{ 
    public static class CommandInterpreter
    {
        public static FixVector2 ToDirection(GameCommand cmd) => cmd switch
        {
            GameCommand.Up => FixVector2.Up,
            GameCommand.Down => FixVector2.Down,
            GameCommand.Left => FixVector2.Left,
            GameCommand.Right => FixVector2.Right,
            _ => FixVector2.Zero
        };
    }
}
