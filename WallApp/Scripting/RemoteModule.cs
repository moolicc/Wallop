using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.Scripting
{
    class RemoteModule : Module
    {
        public override SettingsController CreateSettingsController()
        {
            throw new NotImplementedException();
        }

        public override Controller CreateController()
        {
            throw new NotImplementedException();
        }

        protected override void Initialize()
        {
        }
    }
}
