using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SamplePuzzle
{
    public class CollisionWorld_OLD : ITickable
    {
        private readonly List<ICollidable_OLD> _entities = new List<ICollidable_OLD>();
        private readonly HashSet<(ICollidable_OLD, ICollidable_OLD)> _overlappingPairs = new();

        private HashSet<(ICollidable_OLD, ICollidable_OLD)> _prevContacts = new();
        public void AddEntity(ICollidable_OLD entity)
        {
            _entities.Add(entity);
        }

        public void Step()
        {
            HashSet<(ICollidable_OLD, ICollidable_OLD)> currentContacts = new();

            for(int i = 0; i < _entities.Count; i++)
            {
                for (int j = i + 1; j < _entities.Count; j++)
                {
                    var entityA = _entities[i];
                    var entityB = _entities[j];


                    if (_overlappingPairs.TryGetValue((entityA, entityB), out var pair) || _overlappingPairs.TryGetValue((entityB, entityA), out pair))
                    {
                        if(!entityA.Bounds.Intersects(entityB.Bounds))
                        {
                            entityA.OnSeparate(entityB);
                            entityB.OnSeparate(entityA);
                            _overlappingPairs.Remove(pair);
                        }
                    }

                    else if (entityA.Bounds.Intersects(entityB.Bounds))
                    {
                        entityA.OnOverlap(entityB);
                        entityB.OnOverlap(entityA);
                        _overlappingPairs.Add((entityA, entityB));
                    }
                }
            }

            _prevContacts = currentContacts;
        }

    }
}
