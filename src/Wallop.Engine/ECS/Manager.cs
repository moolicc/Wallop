using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.Scripting.ECS;

namespace Wallop.Engine.ECS
{
    public class Manager
    {
        private static readonly ActorQuerying.Queries.IQuery _allQuery;

        static Manager()
        {
            _allQuery = ActorQuerying.QueryRunner.Parse("*");
        }


        private List<IActor> _actors;

        public Manager()
        {
            _actors = new List<IActor>();
        }

        public IEnumerable<IActor> GetActors(string query)
        {
            return _actors;
        }

        public IEnumerable<IActor> GetActors()
        {
            return GetActors(_allQuery);
        }

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

        internal void Remove(IActor actor)
        {
            _actors.Remove(actor);
        }

        public IEnumerable<IActor> GetActors(ActorQuerying.Queries.IQuery query)
        {
            return ActorQuerying.QueryRunner.RunQuery(query, _actors);
        }


        public void AddActor(IActor actor)
        {
            _actors.Add(actor);
        }

    }
}
