using System;
using System.Collections.Generic;
using System.Text;

namespace WallApp.Bridge.Data
{
    public class LayerCreationPayload : IPayload
    {
        public string Module { get; set; }

        public LayerCreationPayload(string module)
        {

        }

        public override string ToString()
        {
            return $"{nameof(LayerCreationPayload)} : {nameof(Module)} = \"{Module}\"";
        }
    }
}
