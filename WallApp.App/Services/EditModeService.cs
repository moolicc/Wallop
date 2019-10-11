using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WallApp.App.Services
{
    class EditModeService : IService
    {
        public int InitPriority => int.MaxValue;

        private List<Layout.Layer> _layerTracker;

        public void Initialize()
        {
            _layerTracker = new List<Layout.Layer>();
        }

        public void AddLayer(Layout.Layer layer)
        {

        }

        public IEnumerable<XElement> GenerateXmlScript()
        {

            return null;
        }
    }
}
