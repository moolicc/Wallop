using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wallop.Presenter
{
    static class Extensions
    {
        public static bool IsNull(this string input)
        {
            return string.IsNullOrWhiteSpace(input);
        }

        public static void Draw(this SpriteBatch spriteBatch, Texture2D texture, RectangleF bounds, Color color)
        {
            var trueBounds = new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
            spriteBatch.Draw(texture, trueBounds, color);
        }
    }
}
