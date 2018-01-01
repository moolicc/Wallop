using System;
using System.Collections.Generic;
using System.Drawing;
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

        public int XValue { get; set; }
        public int YValue { get; set; }
        public int ZValue { get; set; }
        public int WValue { get; set; }


        public LayerDimensions()
        {
            AbsoluteValues = true;
            MarginValues = false;
            MonitorName = "[Extend]";
            XValue = 0;
            YValue = 0;
            ZValue = 1;
            WValue = 1;
        }

        public (int x, int y, int width, int height) GetBounds()
        {
            int x = XValue;
            int y = YValue;
            int width = ZValue;
            int height = WValue;
            Rectangle screenBounds = GetScreenBounds();

            if (MarginValues)
            {
                width = screenBounds.Right - ZValue;
                height = screenBounds.Bottom - WValue;
                if (!AbsoluteValues)
                {
                    width = screenBounds.Right - (int)(screenBounds.Width * (ZValue / 100.0F));
                    height = screenBounds.Bottom - (int)(screenBounds.Height * (WValue / 100.0F));
                }
            }
            else
            {
                if (!AbsoluteValues)
                {
                    width = (int)(screenBounds.Width * (ZValue / 100.0F));
                    height = (int)(screenBounds.Height * (WValue / 100.0F));
                }
            }

            if (!AbsoluteValues)
            {
                x = screenBounds.X + (int)(screenBounds.Width * (XValue / 100.0F));
                y = screenBounds.Y + (int)(screenBounds.Height * (YValue / 100.0F));
            }
            else
            {
                x = screenBounds.X;
                y = screenBounds.Y;
            }
            
            return (x, y, width, height);
        }

        public Microsoft.Xna.Framework.Rectangle GetBoundsRectangle()
        {
            var bounds = GetBounds();
            return new Microsoft.Xna.Framework.Rectangle(bounds.x, bounds.y, bounds.width, bounds.height);
        }

        public Rectangle GetScreenBounds()
        {
            //if (MonitorName == "" || MonitorName == "[Extend]")
            //{
            //    return new Rectangle(SystemInformation.WorkingArea.X, SystemInformation.WorkingArea.Y, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
            //}
            Screen screen = Screen.AllScreens.FirstOrDefault(s => s.DeviceName == MonitorName) ?? Screen.PrimaryScreen;
            Rectangle area = screen.WorkingArea;
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
