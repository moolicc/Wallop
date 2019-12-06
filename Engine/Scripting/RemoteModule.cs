using System;

namespace WallApp.Engine.Scripting
{
    class RemoteModule : Module
    {
        public override Controller CreateController()
        {
            throw new NotImplementedException();
        }

        public override object CreateViewModel(LayerSettings settings)
        {
            throw new NotImplementedException();
        }

        protected override void Initialize()
        {
        }
    }
}
