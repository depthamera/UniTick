using System;
using System.Collections.Generic;
using UnityEngine;

namespace SamplePuzzle
{
    public class SnapshotLogger
    {
        private List<MoveSnapshot> _log = new();

        public IReadOnlyList<MoveSnapshot> GetAll()
        {
            return _log;
        }

        public void Log(MoveSnapshot snapshot)
        {
            _log.Add(snapshot);
        }
    }
}
