using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;


GetController = new Func<Controller>(() => new StaticImageController());

public class StaticImageController : Controller
{

    private SpriteBatch _spriteBatch;
    private Texture2D _texture;

    public override void Setup()
    {
        _spriteBatch = new SpriteBatch(Rendering.GraphicsDevice);
        using (var fs = new FileStream("test.png", FileMode.Open))
        {
            _texture = Texture2D.FromStream(Redering.GraphicsDevice, fs);
        }
    }

    public override void Dispose()
    {

    }

    public override void Update(GameTime gameTime)
    {

    }

    public override void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin();
        _spriteBatch.Draw(_texture, Vector2.Zero, Color.White);
        _spriteBatch.End();
    }
}
