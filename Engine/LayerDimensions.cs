using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Windows.Forms;

namespace WallApp.Engine
{
    public delegate void DimensionsChangedHandler(object sender);

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
                    leftOffset = -curScreen.WorkingArea.X;
                }
                if (curScreen.WorkingArea.Y < topOffset)
                {
                    topOffset = -curScreen.WorkingArea.Y;
                }
            }
            _primaryOffset = new Point(leftOffset, topOffset);
        }

        public event DimensionsChangedHandler DimensionsChanged;

        public bool AbsoluteValues
        {
            get => absoluteValues;
            set
            {
                if(absoluteValues == value)
                {
                    return;
                }
                absoluteValues = value;
                DimensionsChanged?.Invoke(this);
            }
        }
        public bool MarginValues
        {
            get => marginValues;
            set
            {
                if (marginValues == value)
                {
                    return;
                }
                marginValues = value;
                DimensionsChanged?.Invoke(this);
            }
        }

        public string MonitorName { get; set; }

        public float XValue
        {
            get => xValue;
            set
            {
                xValue = value;
                DimensionsChanged?.Invoke(this);
            }
        }
        public float YValue
        {
            get => yValue;
            set
            {
                yValue = value;
                DimensionsChanged?.Invoke(this);
            }
        }
        public float ZValue
        {
            get => zValue;
            set
            {
                zValue = value;
                DimensionsChanged?.Invoke(this);
            }
        }
        public float WValue
        {
            get => wValue;
            set
            {
                wValue = value;
                DimensionsChanged?.Invoke(this);
            }
        }


        private bool absoluteValues;
        private bool marginValues;
        private float xValue;
        private float yValue;
        private float wValue;
        private float zValue;


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

        public void Set(float? x = null, float? y = null, float? z = null, float? w = null, bool? useAbsolutes = null, bool? useMargins = null)
        {
            bool changes = false;
            if(x.HasValue && xValue != x.Value)
            {
                changes = true;
                xValue = x.Value;
            }
            if(y.HasValue && xValue != y.Value)
            {
                changes = true;
                yValue = y.Value;
            }
            if(z.HasValue && xValue != z.Value)
            {
                changes = true;
                zValue = z.Value;
            }
            if(w.HasValue && xValue != w.Value)
            {
                changes = true;
                wValue = w.Value;
            }
            if(useAbsolutes.HasValue && absoluteValues != useAbsolutes)
            {
                changes = true;
                absoluteValues = useAbsolutes.Value;
            }
            if(useMargins.HasValue && marginValues != useMargins)
            {
                changes = true;
                marginValues = useMargins.Value;
            }

            if(changes)
            {
                DimensionsChanged?.Invoke(this);
            }
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

        public (float x, float y, float z, float w) AsTuple()
        {
            return this;
        }

        public void Deconstruct(out float x, out float y, out float z, out float w)
        {
            x = XValue;
            y = YValue;
            z = ZValue;
            w = WValue;
        }

        public static implicit operator (float x, float y, float z, float w)(LayerDimensions a)
        {
            return (a.XValue, a.YValue, a.ZValue, a.WValue);
        }
    }
}
