namespace Wallop.Engine.ECS.ActorQuerying.FilterMachine
{
    public abstract class MemberBase : IMachineMember
    {
        public string Name { get; init; }
        public string Qualifier { get; init; }

        protected MemberBase(string name, string qualifier)
        {
            Name = name;
            Qualifier = qualifier;
        }

        public bool TryExecute(Machine machine, string[] qualifiers, int argCount)
        {
            if(string.Join('.', qualifiers) != Qualifier)
            {
                return false;
            }

            var args = new object[argCount];
            if(argCount > 0)
            {
                for(int i = argCount - 1; i >= 0; i++)
                {
                    args[i] = machine.PopStateValue();
                }
                
                if(!CheckArgs(args))
                {
                    return false;
                }
            }
            return TryExecute(machine, args);
        }

        protected abstract bool CheckArgs(object[] args);

        protected abstract bool TryExecute(Machine machine, object[] args);
    }
}