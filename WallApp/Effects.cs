using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WallApp
{
    class Effects
    {
        public static string[] GetEffects()
        {
            return Directory.GetFiles("effects", "*.xnb", SearchOption.AllDirectories).Select((s) =>
            {
                if (Path.GetDirectoryName(s) == "effects")
                {
                    return Path.GetFileName(s);
                }
                return Path.GetDirectoryName(s).TrimEnd('\\') + '\\' + Path.GetFileName(s);
            }).ToArray();
        }

        public static Effect CreateEffect(string effectFile, ContentManager contentManager)
        {
            try
            {
                return contentManager.Load<Effect>(effectFile.Remove(effectFile.Length - 4));
            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
