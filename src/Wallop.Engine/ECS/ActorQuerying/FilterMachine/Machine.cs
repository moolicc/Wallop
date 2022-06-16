using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying.FilterMachine
{
    public class Machine
    {
        private Stack<State> _stateStack;

        public Machine()
        {
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

        public void PushMemberInvocation(string member, string[]? memberQualifiers, int argCount)
        {

        }
    }
}
