using System;
using System.Collections.Generic;
using System.Text;

namespace WallApp.Bridge.Data
{
    public class EditModeResponse : IPayload
    {
        // The contents are the modules.
        public List<string> Layers { get; private set; }
        public List<(float x, float y, float z, float w)> LayerPositions { get; private set; }

        public EditModeResponse()
        {
            Layers = new List<string>();
            LayerPositions = new List<(float, float, float, float)>();
        }
    }
}
