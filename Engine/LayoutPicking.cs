using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace WallApp.Engine
{
    [Services.Service]
    class LayoutPicking
    {

        [Services.ServiceReference()]
        private Layout _layout;

        public IEnumerable<LayerSettings> GetLayersUnderMouse(params Rectangle[] ExclusionZones)
        {
            var factor = new Vector2(Settings.Instance.BackBufferWidthFactor, Settings.Instance.BackBufferHeightFactor);
            var mouseLoc = Mouse.GetState().Position.ToVector2() * factor;

            if (ExclusionZones.Any(r => r.Contains(mouseLoc)))
            {
                yield break;
            }

            foreach (var item in _layout.Layers)
            {
                var rect = item.Dimensions.GetBoundsRectangle();
                rect.X = (rect.X * Settings.Instance.BackBufferWidthFactor);
                rect.Y = (rect.Y * Settings.Instance.BackBufferHeightFactor);
                rect.Width = (rect.Width * Settings.Instance.BackBufferWidthFactor);
                rect.Height = (rect.Height * Settings.Instance.BackBufferHeightFactor);

                if (rect.Contains(mouseLoc))
                {
                    yield return item;
                }
            }
        }
    }
}
