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

        private Layout.LayoutInfo _trackingLayout;

        public void Initialize()
        {
        }

        public void StartEdit(Layout.LayoutInfo layoutInfo)
        {
            if(_trackingLayout != null)
            {
                throw new InvalidOperationException("A layout is already being tracked.");
            }
            _trackingLayout = layoutInfo;
            Services.ServiceLocator.Locate<Services.BridgeService>().WriteSetEditMode(true);
        }

        public void StopEdit()
        {
            ServiceLocator.Locate<Services.BridgeService>().WriteSetEditMode(false);
            _trackingLayout = null;
        }

        public void AddLayer(string module)
        {
            ServiceLocator.Locate<Services.BridgeService>().WriteAddLayer(module);
        }

        public IEnumerable<XElement> GenerateXmlScript()
        {
            return null;
        }
    }
}
