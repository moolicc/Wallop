using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Scripting.ECS.Serialization
{

    [Serializable]
    public class ElementInitializationException : Exception
    {
        public ElementInitializationException() { }
        public ElementInitializationException(string message) : base(message) { }
        public ElementInitializationException(string message, Exception inner) : base(message, inner) { }
        protected ElementInitializationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
