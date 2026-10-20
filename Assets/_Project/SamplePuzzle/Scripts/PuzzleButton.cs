using System;
using UnityEngine;

namespace SamplePuzzle
{
    public class PuzzleButton
    {
        private readonly PuzzleDoor _door;

        public PuzzleButton(PuzzleDoor door)
        {
            _door = door;
        }

        public void Press()
        {
            _door.Open();
        }

        public void Release()
        {
            _door.Close();
        }
    }
}
