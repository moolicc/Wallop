using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace WallApp
{
    [Services.Service]
    class EditModeHandler : Services.InitializableService
    {

        [Services.ServiceReference]
        private LayoutPicking _picker;
        [Services.ServiceReference()]
        private Layout _layout;

        private Texture2D _blankTexture;
        private SpriteBatch _spriteBatch;
        private MouseState _prevMouseState;
        private LayerSettings _dragLayer;
        private Vector2 _singleUnit;

        public void Init(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
            _blankTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _blankTexture.SetData(new Color[1] { Color.White });

            _singleUnit = new Vector2((1.0F * Settings.Instance.BackBufferWidthFactor), (1.0F * Settings.Instance.BackBufferHeightFactor));
        }

        public void Update(GameTime gameTime)
        {
            var layers = _picker.GetLayersUnderMouse();
            var mouseState = Mouse.GetState();
            if (_prevMouseState == null)
            {
                _prevMouseState = mouseState;
                return;
            }
            if (!layers.Any() && _dragLayer == null)
            {
                return;
            }

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (_prevMouseState.LeftButton == ButtonState.Released)
                {
                    //Pick the last layer that will be drawn to respect Z-order.
                    _dragLayer = layers.Last();
                }
                else if (_dragLayer != null)
                {
                    Point mouseMovement = mouseState.Position - _prevMouseState.Position;

                    if (mouseMovement.X >= _singleUnit.X && mouseMovement.X > 0)
                    {
                        _dragLayer.Dimensions.XValue += _singleUnit.X;
                    }
                    else if (Math.Abs(mouseMovement.X) >= _singleUnit.X && mouseMovement.X < 0)
                    {
                        _dragLayer.Dimensions.XValue -= _singleUnit.X;
                    }

                    if (mouseMovement.Y >= _singleUnit.Y && mouseMovement.Y > 0)
                    {
                        _dragLayer.Dimensions.YValue += _singleUnit.Y;
                    }
                    else if (Math.Abs(mouseMovement.Y) >= _singleUnit.Y && mouseMovement.Y < 0)
                    {
                        _dragLayer.Dimensions.YValue -= _singleUnit.Y;
                    }
                }
            }
            else
            {
                _dragLayer = null;
            }

            _prevMouseState = mouseState;
        }

        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            var layersBeneath = _picker.GetLayersUnderMouse();

            foreach (var item in _layout.Layers)
            {
                var color = new Color(0.5F, 0.5F, 0.5F, 0.05F);
                var cornerColor = new Color(0.6F, 0.6F, 0.6F, 1.0F);
                if (layersBeneath.Contains(item))
                {
                    color = new Color(0.0F, 0.0F, 0.2F, 0.1F);
                    cornerColor = Color.Blue;
                }
                var bounds = item.Dimensions.GetBoundsRectangle();
                bounds.X = (int)(bounds.X * Settings.Instance.BackBufferWidthFactor);
                bounds.Y = (int)(bounds.Y * Settings.Instance.BackBufferHeightFactor);
                bounds.Width = (int)(bounds.Width * Settings.Instance.BackBufferWidthFactor);
                bounds.Height = (int)(bounds.Height * Settings.Instance.BackBufferHeightFactor);
                bounds.X -= 2;
                bounds.Y -= 2;
                bounds.Width += 4; // += new Point(4, 4);
                bounds.Height += 4;
                _spriteBatch.Draw(_blankTexture, bounds, color);

                var cornerBounds = new RectangleF(bounds.X, bounds.Y, 2, 2);
                _spriteBatch.Draw(_blankTexture, cornerBounds, cornerColor);

                cornerBounds = new RectangleF(bounds.Right - 2, bounds.Y, 2, 2);
                _spriteBatch.Draw(_blankTexture, cornerBounds, cornerColor);

                cornerBounds = new RectangleF(bounds.Right - 2, bounds.Bottom - 2, 2, 2);
                _spriteBatch.Draw(_blankTexture, cornerBounds, cornerColor);

                cornerBounds = new RectangleF(bounds.X, bounds.Bottom - 2, 2, 2);
                _spriteBatch.Draw(_blankTexture, cornerBounds, cornerColor);
            }
            _spriteBatch.End();

        }
    }
}
