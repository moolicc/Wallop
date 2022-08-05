namespace Wallop.Shared.ECS.ActorQuerying.FilterMachine
{
    public class VariableMember : MemberBase
    {
        public object Value
        {
            get => _stateValue.GetValue();
            set => _stateValue = State.CreateObject(value);
        }
        public bool Readonly { get; set; }
        private State _stateValue;

        public VariableMember(string name, object value)
            : this(name, value, "")
        {

        }

        public VariableMember(string name, object value, string qualifier)
            : base(name, qualifier)
        {
            Value = value;
        }


        protected override bool CheckArgs(object[] args)
        {
            if (args.Length > 1)
            {
                return false;
            }
            if (!Readonly && args.Length == 1 && args[0].GetType() == Value.GetType())
            {
                return true;
            }
            if (args.Length == 0)
            {
                return true;
            }

            return false;
        }

        protected override bool TryExecute(Machine machine, object[] args)
        {
            if (args.Length == 1)
            {
                Value = args[0];
            }
            else
            {
                machine.PushState(_stateValue);
            }
            return true;
        }
    }
}