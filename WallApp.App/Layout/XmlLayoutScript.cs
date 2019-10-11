using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace WallApp.App.Layout
{
    class XmlLayoutScript : ILayoutScript
    {
        private ScriptTypes.LayoutScriptContext _context;
        private string _source;

        public void Execute(string source, ScriptTypes.LayoutScriptContext context)
        {
            //TODO: Load-up the xml and create the necessary layers.
            _context = context;
            _source = source;
            LoadXml();
        }

        public void Cleanup()
        {
        }

        private void LoadXml()
        {
            XDocument document = XDocument.Parse(_source);
            LoadRoot(document.Root);
        }

        private void LoadRoot(XElement rootElement)
        {
            foreach (var node in rootElement.Elements("layer"))
            {
                LoadLayer(node);
            }
        }

        private void LoadLayer(XElement element)
        {
            float GetPosValue(XAttribute input)
            {
                float result = 0.0F;
                if(input == null)
                {
                    //TODO: warning
                }
                else if(!float.TryParse(input.Value, out result))
                {
                    //TODO: Error out with input.Name as debug info.
                }
                return result;
            }

            var moduleAttrib = element.Attribute("module");
            var referenceMonitorAttrib = element.Attribute("monitor");
            var posXAttrib = element.Attribute("x");
            var posYAttrib = element.Attribute("y");
            var posZAttrib = element.Attribute("z");
            var posWAttrib = element.Attribute("w");
            var absPosAttrib = element.Attribute("absolute");
            var marginPosAttrib = element.Attribute("margin");

            string module = "";
            string referenceMonitor = "";
            float posX = 0.0F;
            float posY = 0.0F;
            float posZ = 0.0F;
            float posW = 0.0F;
            bool absolutePos = false;
            bool marginPos = false;

            if (moduleAttrib == null)
            {
                //TODO: Error
            }
            else
            {
                module = moduleAttrib.Value;
            }

            if(referenceMonitorAttrib == null)
            {
                //TODO: Error
            }
            else
            {
                referenceMonitor = referenceMonitorAttrib.Value;
            }

            posX = GetPosValue(posXAttrib);
            posY = GetPosValue(posYAttrib);
            posZ = GetPosValue(posZAttrib);
            posW = GetPosValue(posWAttrib);

            //TODO: Handle invalid attrib.Value data.
            if(absPosAttrib != null)
            {
                bool.TryParse(absPosAttrib.Value, out absolutePos);
            }
            if (marginPosAttrib != null)
            {
                bool.TryParse(marginPosAttrib.Value, out absolutePos);
            }

            int layerId = _context.CreateLayer(module);
            _context.SetReferenceMonitor(layerId, referenceMonitor);
            _context.SetDimensions(layerId, posX, posY, posZ, posW);
            _context.SetAbsoluteDimensions(layerId, absolutePos);
            _context.SetMarginDimensions(layerId, marginPos);
        }
    }
}
