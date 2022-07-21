using System.Reflection;

namespace Wallop.ECS.ActorQuerying.FilterMachine
{
    public class PropertyInfoMember : MemberBase
    {
        public object? TargetObject { get; set; }
        public PropertyInfo Property { get; set; }

        public PropertyInfoMember(string name, string qualifier, PropertyInfo property, object? targetObject)
            : base(name, qualifier)
        {
            TargetObject = targetObject;
            Property = property;
        }


        protected override bool CheckArgs(object[] args)
        {
            if(args.Length == 1 && !Property.CanWrite)
            {
                return false;
            }
            else if (args.Length > 1)
            {
                return false;
            }

            return true;
        }

        protected override bool TryExecute(Machine machine, object[] args)
        {
            try
            {
                if(args.Length == 1)
                {
                    Property.SetValue(TargetObject, args[0]);
                }
                else
                {
                    machine.PushState(State.CreateObject(Property.GetValue(TargetObject)));
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