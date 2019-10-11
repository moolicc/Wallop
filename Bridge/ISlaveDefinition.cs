using System;
using System.Collections.Generic;
using System.Text;

namespace WallApp.Bridge
{
    public interface ISlaveDefinition
    {
        void SetEditMode(Data.EditModePayload input);
        Data.LayerCreationResponsePayload CreateLayer(Data.LayerCreationPayload input);
        void CoordinateData(Data.PositionPayload input);
    }
}
