using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS
{
    public class Manager
    {
        private static readonly ActorQuerying.Queries.IQuery _allQuery;

        static Manager()
        {
            _allQuery = ActorQuerying.QueryRunner.Parse("*");
        }


        private List<Actor> _actors;

        public Manager()
        {
            _actors = new List<Actor>();
        }

        public IEnumerable<Actor> GetActors(string query)
        {
            return _actors;
        }

        public IEnumerable<Actor> GetActors()
        {
            return GetActors(_allQuery);
        }

        public IEnumerable<TActor> GetActors<TActor>() where TActor : Actor
        {
            return _actors.Where(a => a.GetType() == typeof(TActor)).Select(a => (TActor)a);
        }

        public IEnumerable<Actor> GetActors(ActorQuerying.Queries.IQuery query)
        {
            return ActorQuerying.QueryRunner.RunQuery(query, _actors);
        }

        public void AddActor(Actor actor)
        {
            _actors.Add(actor);
        }
    }
}
