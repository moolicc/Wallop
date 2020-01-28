using System.Xml.Linq;

namespace Wallop.Composer.Layout
{
    class XmlLayoutScript : ILayoutScript
    {
        private string _source;

        public void Execute(string source)
        {
            //TODO: Load-up the xml and create the necessary layers.
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
            if (rootElement.Name != "layout")
            {
                // TODO: Error
            }

            foreach (var node in rootElement.Elements("layer"))
            {
                LoadLayer(node);
            }
        }

        private void LoadLayer(XElement element)
        {
            float GetPosValue(XElement input)
            {
                float result = 0.0F;
                if (input == null)
                {
                    //TODO: warning
                }
                else if (!float.TryParse(input.Value, out result))
                {
                    //TODO: Error out with input.Name as debug info.
                }
                return result;
            }


            var moduleElement = element.Element("module");
            var dimensionsElement = element.Element("dimensions");

            if (moduleElement == null)
            {
                //TODO: Error
            }
            if (dimensionsElement == null)
            {
                //TODO: Error
            }

            var referenceMonitorElement = element.Element("dimensions").Element("monitor");
            var posXElement = element.Element("dimensions").Element("x");
            var posYElement = element.Element("dimensions").Element("y");
            var posZElement = element.Element("dimensions").Element("z");
            var posWElement = element.Element("dimensions").Element("w");
            var absPosElement = element.Element("dimensions").Element("absolute");
            var marginPosElement = element.Element("dimensions").Element("margin");

            string module = moduleElement.Value;

            string referenceMonitor = "";
            float posX = 0.0F;
            float posY = 0.0F;
            float posZ = 0.0F;
            float posW = 0.0F;
            bool absolutePos = false;
            bool marginPos = false;

            if (referenceMonitorElement != null)
            {
                referenceMonitor = referenceMonitorElement.Value;
            }

            posX = GetPosValue(posXElement);
            posY = GetPosValue(posYElement);
            posZ = GetPosValue(posZElement);
            posW = GetPosValue(posWElement);

            //TODO: Handle invalid Value data.
            if (absPosElement != null)
            {
                bool.TryParse(absPosElement.Value, out absolutePos);
            }
            if (marginPosElement != null)
            {
                bool.TryParse(marginPosElement.Value, out absolutePos);
            }

            var service = Services.ServiceLocator.Locate<Services.LayoutScriptService>();
            int layerId = service.AddLayer(module);
            service.SetReferenceMonitor(layerId, referenceMonitor);
            service.SetDimensions(layerId, posX, posY, posZ, posW);
            service.SetAbsoluteDimensions(layerId, absolutePos);
            service.SetMarginDimensions(layerId, marginPos);
        }
    }
}
