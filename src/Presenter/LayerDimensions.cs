using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Wallop.Presenter
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

        public bool AbsoluteValues => _absoluteValues;
        public bool MarginValues => _marginValues;

        public string MonitorName { get; set; }

        public float XValue => _xValue;
        public float YValue => _yValue;
        public float ZValue => _zValue;
        public float WValue => _wValue;


        private bool _absoluteValues;
        private bool _marginValues;
        private float _xValue;
        private float _yValue;
        private float _zValue;
        private float _wValue;


        public LayerDimensions()
        {
            _absoluteValues = true;
            _marginValues = false;
            MonitorName = "[Extend]";
            _xValue = 0;
            _yValue = 0;
            _zValue = 10;
            _wValue = 10;
        }



        public RectangleF GetBoundsRectangle()
        {
            var (X, Y, Width, Height) = GetBounds();
            return new RectangleF(X, Y, Width, Height);
        }

        public void Set(float? x = null, float? y = null, float? z = null, float? w = null, bool? useAbsolutes = null, bool? useMargins = null)
        {
            bool changes = false;
            if (x.HasValue && _xValue != x.Value)
            {
                changes = true;
                _xValue = x.Value;
            }
            if (y.HasValue && _yValue != y.Value)
            {
                changes = true;
                _yValue = y.Value;
            }
            if (z.HasValue && _zValue != z.Value)
            {
                changes = true;
                _zValue = z.Value;
            }
            if (w.HasValue && _wValue != w.Value)
            {
                changes = true;
                _wValue = w.Value;
            }
            if (useAbsolutes.HasValue && _absoluteValues != useAbsolutes)
            {
                changes = true;
                _absoluteValues = useAbsolutes.Value;
            }
            if (useMargins.HasValue && _marginValues != useMargins)
            {
                changes = true;
                _marginValues = useMargins.Value;
            }

            if (changes)
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
                _absoluteValues = this.AbsoluteValues,
                _marginValues = this.MarginValues,
                MonitorName = this.MonitorName,
                _wValue = this.WValue,
                _xValue = this.XValue,
                _yValue = this.YValue,
                _zValue = this.ZValue,
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
