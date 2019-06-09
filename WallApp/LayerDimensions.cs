using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WallApp
{
    public class LayerDimensions : ICloneable
    {
        private static Point _primaryOffset;

        static LayerDimensions()
        {
            int leftOffset = 0;
            int topOffset = 0;
            foreach (var curScreen in Screen.AllScreens)
            {
                if (curScreen.WorkingArea.X < leftOffset)
                {
                    leftOffset =  -curScreen.WorkingArea.X;
                }
                if (curScreen.WorkingArea.Y < topOffset)
                {
                    topOffset = -curScreen.WorkingArea.Y;
                }
            }
            _primaryOffset = new Point(leftOffset, topOffset);
        }

        public bool AbsoluteValues { get; set; }
        public bool MarginValues { get; set; }

        public string MonitorName { get; set; }

        public float XValue { get; set; }
        public float YValue { get; set; }
        public float ZValue { get; set; }
        public float WValue { get; set; }


        public LayerDimensions()
        {
            AbsoluteValues = true;
            MarginValues = false;
            MonitorName = "[Extend]";
            XValue = 0;
            YValue = 0;
            ZValue = 10;
            WValue = 10;
        }



        public RectangleF GetBoundsRectangle()
        {
            var (X, Y, Width, Height) = GetBounds();
            return new RectangleF(X, Y, Width, Height);
        }

        public (float X, float Y, float Width, float Height) GetBounds()
        {
            float x = XValue;
            float y = YValue;
            float width = ZValue;
            float height = WValue;
            Rectangle screenBounds = GetScreenBounds();

            if (MarginValues)
            {
                width = screenBounds.Right - ZValue;
                height = screenBounds.Bottom - WValue;
                if (!AbsoluteValues)
                {
                    width = (screenBounds.Right - (screenBounds.Width * (ZValue / 100.0F)));
                    height = (screenBounds.Bottom - (screenBounds.Height * (WValue / 100.0F)));
                }
            }
            else
            {
                if (!AbsoluteValues)
                {
                    width = (screenBounds.Width * (ZValue / 100.0F));
                    height = (screenBounds.Height * (WValue / 100.0F));
                }
            }

            if (!AbsoluteValues)
            {
                x = (screenBounds.X + (screenBounds.Width * (XValue / 100.0F)));
                y = (screenBounds.Y + (screenBounds.Height * (YValue / 100.0F)));
            }
            else
            {
                x = screenBounds.X;
                y = screenBounds.Y;
            }
            
            return (x, y, width, height);
        }



        public Rectangle GetScreenBounds()
        {
            //if (MonitorName == "" || MonitorName == "[Extend]")
            //{
            //    return new Rectangle(SystemInformation.WorkingArea.X, SystemInformation.WorkingArea.Y, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
            //}
            Screen screen = Screen.AllScreens.FirstOrDefault(s => s.DeviceName == MonitorName) ?? Screen.PrimaryScreen;
            Rectangle area = new Rectangle(screen.WorkingArea.X, screen.WorkingArea.Y, screen.WorkingArea.Width, screen.WorkingArea.Height);
            area.Offset(_primaryOffset);

            return area;
        }

        public object Clone()
        {
            return new LayerDimensions()
            {
                AbsoluteValues = this.AbsoluteValues,
                MarginValues = this.MarginValues,
                MonitorName = this.MonitorName,
                WValue = this.WValue,
                XValue = this.XValue,
                YValue = this.YValue,
                ZValue = this.ZValue,
            };
        }
    }
}
