using System.Collections.Generic;
using System.IO;
using WallApp.App.Layout;

namespace WallApp.App.Services
{
    /// <summary>
    /// The layoutservice handles saving and loading layouts, as well as applying them to the engine.
    /// </summary>
    class LayoutService : IService
    {
        public int InitPriority => 10;

        public void Initialize()
        {
        }

        public void SaveLayout(string filePath, LayoutInfo layout)
        {

        }

        public void ApplyLayout(LayoutInfo layout)
        {
            //TODO: Send spin-up script runtime and set the engine's state.
            Layout.ILayoutScript layoutScript = null;
            if (layout.ScriptIsData)
            {
                layoutScript = new XmlLayoutScript();
            }
            else
            {

            }

            layoutScript.Execute(layout.Script);
        }

        public IEnumerable<Layout.LayoutInfo> LoadLayouts()
        {
            if (!Directory.Exists("layouts"))
            {
                Directory.CreateDirectory("layouts");
            }
            var files = Directory.GetFiles("layouts", "*.wapp");

            foreach (var item in files)
            {
                using (FileStream fs = new FileStream(item, FileMode.Open))
                {
                    Layout.Serializing.LayoutReader reader = new Layout.Serializing.LayoutReader(fs);
                    yield return reader.LoadLayout();
                }
            }
        }
    }
}
