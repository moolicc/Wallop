namespace Wallop.ECS.ActorQuerying.FilterMachine
{
    public class FunctionMember : MemberBase
    {
        public Delegate Action { get; set; }

        public FunctionMember(string name, string qualifier, Delegate action)
            : base(name, qualifier)
        {
            Action = action;
        }


        protected override bool CheckArgs(object[] args)
        {
            var parameters = Action.Method.GetParameters();
            if(args.Length != parameters.Length - parameters.Where(p => p.IsOptional).Count())
            {
                return false;
            }

            for(int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var param = parameters[i];

                if(arg.GetType() != param.ParameterType)
                {
                    return false;
                }
            }

            return true;
        }

        protected override bool TryExecute(Machine machine, object[] args)
        {
            try
            {
                if(Action.Method.ReturnType != typeof(void))
                {
                    var result = Action.DynamicInvoke(args);
                    if(result != null)
                    {
                        machine.PushState(State.CreateObject(result));
                    }
                }
                else
                {
                    Action.DynamicInvoke(args);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}