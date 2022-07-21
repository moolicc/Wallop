using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.ECS.ActorQuerying.FilterMachine
{
    public class Machine
    {
        public IReadOnlyList<IActor> OriginalActorSet { get; private set; }
        public List<IActor> ActorSet { get; private set; }

        public List<IMachineMember> Members { get; init; }

        private Stack<State> _stateStack;


        public Machine(IEnumerable<IActor> set)
        {
            OriginalActorSet = set.ToList().AsReadOnly();
            ActorSet = set.ToList();

            Members = new List<IMachineMember>();
            _stateStack = new Stack<State>();
        }

        public void PushState(State state)
        {
            _stateStack.Push(state);
        }

        public State PopState()
        {
            return _stateStack.Pop();
        }

        public State PopState(ValueKinds expectedValueType)
        {
            var state = _stateStack.Pop();
            if(state.ValueType != expectedValueType)
            {
                throw new InvalidOperationException($"Expected {expectedValueType} value on stack.");
            }
            return state;
        }

        public object PopStateValue()
        {
            return _stateStack.Pop().GetValue();
        }

        public object PopStateValue(ValueKinds expectedValueType)
        {
            var state = _stateStack.Pop();
            if (state.ValueType != expectedValueType)
            {
                throw new InvalidOperationException($"Expected {expectedValueType} value on stack.");
            }
            return state.GetValue();
        }

        public T PopStateValue<T>(ValueKinds expectedValueType)
        {
            var state = _stateStack.Pop();
            if (state.ValueType != expectedValueType)
            {
                throw new InvalidOperationException($"Expected {expectedValueType} value on stack.");
            }
            return (T)state.GetValue();
        }

        public void InvokeMember(string member, string[]? memberQualifiers, int argCount)
        {
            bool memberFound = false;
            foreach (var item in Members)
            {
                if(!item.Name.Equals(member, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if(item.TryExecute(this, memberQualifiers ?? Array.Empty<string>(), argCount))
                {
                    memberFound = true;
                    break;
                }
            }

            if(!memberFound)
            {
                // TODO: Warning or error.
            }
        }
    }
}
