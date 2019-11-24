using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.App.Layout
{
    class LayoutInfo
    {
        public static readonly LayoutInfo Default = new LayoutInfo() { Title = "Empty", Thumbnail = "logo.png" };

        public string Title { get; set; }
        public string Author { get; set; }
        public string Thumbnail { get; set; }
        public string Script { get; set; }

        //TODO: Implement this in the file loader.
        public bool ScriptIsData { get; set; }
        public Dictionary<string, byte[]> Resources { get; private set; }

        public Dictionary<int, VarTable> VariableTables { get; private set; }

        public LayoutInfo()
        {
            Title = "";
            Author = "";
            Thumbnail = "";
            Script = "";
            ScriptIsData = false;
            Resources = new Dictionary<string, byte[]>();
            VariableTables = new Dictionary<int, VarTable>();
        }
    }
}
