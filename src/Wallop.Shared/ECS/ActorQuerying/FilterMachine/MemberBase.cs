namespace Wallop.Shared.ECS.ActorQuerying.FilterMachine
{
    public abstract class MemberBase : IMachineMember
    {
        public string Tag { get; set; }
        public bool RequireQualifier { get; set; }
        public string Name { get; init; }
        public string Qualifier { get; init; }

        protected MemberBase(string name, string qualifier)
        {
            Tag = string.Empty;
            Name = name;
            Qualifier = qualifier;
            RequireQualifier = !string.IsNullOrEmpty(qualifier);
        }

        public bool TryExecute(Machine machine, string[] qualifiers, int argCount)
        {
            if (RequireQualifier && string.Join('.', qualifiers) != Qualifier)
            {
                return false;
            }

            var args = new object[argCount];
            if (argCount > 0)
            {
                for (int i = argCount - 1; i >= 0; i--)
                {
                    args[i] = machine.PopStateValue();
                }

                if (!CheckArgs(args))
                {
                    for (int i = 0; i < argCount; i++)
                    {
                        machine.PushState(State.CreateObject(args[i]));
                    }
                    return false;
                }
            }
            return TryExecute(machine, args);
        }

        protected abstract bool CheckArgs(object[] args);

        protected abstract bool TryExecute(Machine machine, object[] args);
    }
}