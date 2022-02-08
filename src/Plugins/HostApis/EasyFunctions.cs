using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TrippyGL;
using TrippyGL.ImageSharp;
using Wallop.DSLExtension.Scripting;
using Wallop.Engine.Scripting;

namespace HostApis
{
    internal class EasyFunctions
    {
        public const string VAR_DEVICE = "graphicsDevice";
        public const string VAR_SHADER = "shader";
        public const string VAR_BATCHER = "batcher";

        private delegate void DrawSeparateCoords(string texture, float x, float y);
        private delegate void DrawVectorCoords(string texture, Vector2 coords);


        private delegate void DrawSeparateCoordsScale(string texture, float x, float y, float scale);
        private delegate void DrawVectorCoordsScale(string texture, Vector2 coords, float scale);


        private delegate void DrawSeparateCoordsSeparateScale(string texture, float x, float y, float scaleX, float scaleY);
        private delegate void DrawVectorCoordsVectorScale(string texture, Vector2 coords, Vector2 scale);

        private delegate void DrawSeparateCoordsSeparateScaleSeparateOrigin(string texture, float x, float y, float scaleX, float scaleY, float rotation, float originX, float originY);
        private delegate void DrawSeparateCoordsSeparateScaleVectorOrigin(string texture, float x, float y, float scaleX, float scaleY, float rotation, Vector2 origin);
        private delegate void DrawVectorCoordsVectorScaleVectorOrigin(string texture, Vector2 coords, Vector2 scale, float rotation, Vector2 origin);



        private static Dictionary<string, Texture2D> _textureCache;

        public static Texture2D GetTexture(IScriptContext context, string texture)
        {
            if (_textureCache == null)
            {
                _textureCache = new Dictionary<string, Texture2D>();
            }

            var key = $"{context.GetName()}-{texture}";
            if(!_textureCache.TryGetValue(key, out var result))
            {
                if (!File.Exists(texture))
                {
                    texture = Path.Combine(context.GetBaseDirectory(), texture);
                }

                var device = context.GetValue<GraphicsDevice>(VAR_DEVICE);
                if (File.Exists(texture))
                {
                    result = Texture2DExtensions.FromFile(context.GetValue<GraphicsDevice>(VAR_DEVICE), texture);
                }
                else
                {
                    result = new Texture2D(device, 1, 1);
                    result.SetData(new ReadOnlySpan<Color4b>(new[] { new Color4b(1, 1, 1, 1) }));
                }
                _textureCache.Add(key, result);
            }
            return result;
        }


        public IScriptContext BoundContext;

        public EasyFunctions(IScriptContext boundContext, GraphicsDevice device, SimpleShaderProgram shader, TextureBatcher batcher)
        {
            BoundContext = boundContext;

            boundContext.AddValue(VAR_DEVICE, device);
            boundContext.AddValue(VAR_SHADER, shader);
            boundContext.AddValue(VAR_BATCHER, batcher);

            boundContext.AddDelegate(nameof(Image), new DrawSeparateCoords(Image));
            //boundContext.AddDelegate(nameof(Image), new DrawVectorCoords(Image));

            //boundContext.AddDelegate(nameof(Image), new DrawSeparateCoordsScale(Image));
            //boundContext.AddDelegate(nameof(Image), new DrawVectorCoordsScale(Image));

            boundContext.AddDelegate(nameof(ImageScaled), new DrawSeparateCoordsSeparateScale(ImageScaled));
            //boundContext.AddDelegate(nameof(Image), new DrawVectorCoordsVectorScale(Image));


            boundContext.AddDelegate(nameof(ImageScaledRotation), new DrawSeparateCoordsSeparateScaleSeparateOrigin(ImageScaledRotation));
            //boundContext.AddDelegate(nameof(Image), new DrawSeparateCoordsSeparateScaleVectorOrigin(Image));
            //boundContext.AddDelegate(nameof(Image), new DrawVectorCoordsVectorScaleVectorOrigin(Image));
        }

        public void Image(string texture, float x, float y)
            => Image(texture, new Vector2(x, y));

        public void Image(string texture, Vector2 coords)
        {
            var batcher = BoundContext.GetValue<TextureBatcher>("batcher");
            batcher?.Draw(GetTexture(BoundContext, texture), coords);
        }



        public void Image(string texture, float x, float y, float scale)
            => Image(texture, new Vector2(x, y), scale);

        public void Image(string texture, Vector2 coords, float scale)
        {
            var batcher = BoundContext.GetValue<TextureBatcher>("batcher");
            batcher?.Draw(GetTexture(BoundContext, texture), coords, null, Color4b.White, scale, 0.0f);
        }


        public void ImageScaled(string texture, float x, float y, float scaleX, float scaleY)
            => Image(texture, new Vector2(x, y), new Vector2(scaleX, scaleY));

        public void Image(string texture, Vector2 coords, Vector2 scale)
        {
            var batcher = BoundContext.GetValue<TextureBatcher>("batcher");
            batcher?.Draw(GetTexture(BoundContext, texture), coords, null, Color4b.White, scale, 0.0f);
        }


        public void ImageScaledRotation(string texture, float x, float y, float scaleX, float scaleY, float rotation, float originX, float originY)
            => Image(texture, new Vector2(x, y), new Vector2(scaleX, scaleY), rotation, new Vector2(originX, originY));

        public void Image(string texture, float x, float y, float scaleX, float scaleY, float rotation, Vector2 origin)
            => Image(texture, new Vector2(x, y), new Vector2(scaleX, scaleY), rotation, origin);

        public void Image(string texture, Vector2 coords, Vector2 scale, float rotation, Vector2 origin)
        {
            var batcher = BoundContext.GetValue<TextureBatcher>("batcher");
            batcher?.Draw(GetTexture(BoundContext, texture), coords, null, Color4b.White, scale, rotation, origin);
        }
    }
}
