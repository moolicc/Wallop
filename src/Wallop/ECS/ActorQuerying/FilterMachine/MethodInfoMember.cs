using System.Reflection;

namespace Wallop.ECS.ActorQuerying.FilterMachine
{
    public class MethodInfoMember : MemberBase
    {
        public object? TargetObject { get; set; }
        public MethodInfo Action { get; set; }

        public MethodInfoMember(string name, string qualifier, MethodInfo action, object? TargetObject)
            : base(name, qualifier)
        {
            Action = action;
        }


        protected override bool CheckArgs(object[] args)
        {
            var parameters = Action.GetParameters();
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
                if(Action.ReturnType != typeof(void))
                {
                    var result = Action.Invoke(TargetObject, args);
                    if(result != null)
                    {
                        machine.PushState(State.CreateObject(result));
                    }
                }
                else
                {
                    Action.Invoke(TargetObject, args);
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