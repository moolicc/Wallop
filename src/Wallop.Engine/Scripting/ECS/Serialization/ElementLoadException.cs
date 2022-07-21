using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Scripting.ECS.Serialization
{

    [Serializable]
    public class ElementLoadException : Exception
    {
        public ElementLoadException() { }
        public ElementLoadException(string message) : base(message) { }
        public ElementLoadException(string message, Exception inner) : base(message, inner) { }
        protected ElementLoadException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
