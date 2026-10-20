using System;
using UnityEngine;

namespace SamplePuzzle
{
    public class PuzzleDoor
    {

        public bool IsOpen { get; private set; } = false;


        public void Open()
        {
            IsOpen = true;
        }

        internal void Close()
        {
            IsOpen = false;
        }
    }
}
