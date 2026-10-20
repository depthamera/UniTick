namespace SamplePuzzle
{
    public static class FixMath
    {
        public static int FloorToInt(Fix64 x)
        {
            return checked((int)(x.Raw >> Fix64.FractionBits));
        }
    }
}
