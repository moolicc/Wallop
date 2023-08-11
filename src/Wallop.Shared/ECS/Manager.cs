using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying;

namespace Wallop.Shared.ECS
{
    public class Manager
    {
        private List<IActor> _actors;

        public Manager()
        {
            _actors = new List<IActor>();
        }

        public IEnumerable<IActor> GetActors(string query)
            => QueryRunner.RunQuery(query, _actors);

        public IEnumerable<IActor> GetActors()
            => GetActors(QueryRunner.AllQuery);

        public IEnumerable<IActor> GetActors(ActorQuerying.Queries.IQuery query)
            => QueryRunner.RunQuery(query, _actors);

        public IEnumerable<TActor> GetActors<TActor>() where TActor : IActor
        {
            foreach (var actor in _actors)
            {
                if(actor is TActor tactor)
                {
                    yield return tactor;
                }
            }
        }

        public IEnumerable<TActor> GetActors<TActor>(string query) where TActor : IActor
        {
            foreach (var actor in GetActors(query))
            {
                if (actor is TActor tactor)
                {
                    yield return tactor;
                }
            }
        }

        public void Remove(IActor actor)
        {
            _actors.Remove(actor);
        }

        public void RemoveFirst<TActor>(Predicate<TActor> predicate) where TActor : IActor
        {
            for(int i = 0; i < _actors.Count; i++)
            {
                if (_actors[i] is TActor tactor)
                {
                    if (predicate(tactor))
                    {
                        _actors.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        public int RemoveNull()
        {
            int count = 0;
            for (int i = 0; i < _actors.Count; i++)
            {
                if (_actors[i] == null)
                {
                    count++;
                    _actors.RemoveAt(i);
                    i--;
                }
            }
            return count;
        }

        public void AddActor(IActor actor)
        {
            _actors.Add(actor);
        }

    }
}
