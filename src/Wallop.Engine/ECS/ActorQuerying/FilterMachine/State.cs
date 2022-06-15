using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying.FilterMachine
{
    public enum ValueKinds : byte
    {
        String,
        Integer,
        Boolean,
        Float,
    }

    [StructLayout(LayoutKind.Explicit)]
    public readonly struct State
    {
        [FieldOffset(0)]
        public readonly ValueKinds ValueType;

        [FieldOffset(1)]
        public readonly int ValueI;

        [FieldOffset(1)]
        public readonly bool ValueB;

        [FieldOffset(1)]
        public readonly double ValueD;

        [FieldOffset(16)]
        public readonly string ValueS;

        public State(string value)
        {
            ValueType = ValueKinds.String;
            ValueI = 0;
            ValueB = false;
            ValueD = double.NaN;
            ValueS = value;
        }

        public State(int value)
        {
            ValueType = ValueKinds.Integer;
            ValueS = "";
            ValueB = false;
            ValueD = double.NaN;
            ValueI = value;
        }

        public State(bool value)
        {
            ValueType = ValueKinds.Boolean;
            ValueI = 0;
            ValueS = "";
            ValueD = double.NaN;
            ValueB = value;
        }

        public State(double value)
        {
            ValueType = ValueKinds.Float;
            ValueI = 0;
            ValueS = "";
            ValueB = false;
            ValueD = value;
        }

        public object GetValue()
        {
            switch (ValueType)
            {
                case ValueKinds.String:
                    return ValueS;
                case ValueKinds.Integer:
                    return ValueI;
                case ValueKinds.Boolean:
                    return ValueB;
                case ValueKinds.Float:
                    return ValueD;
                default:
                    return new Exception();
            }
        }
    }
}
