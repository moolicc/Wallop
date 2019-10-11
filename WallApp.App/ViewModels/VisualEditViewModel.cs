using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WallApp.App.ViewModels
{
    class VisualEditViewModel
    {
        public List<Modules.Module> Modules { get; private set; }

        public ICommand BackCommand => _backCommand;

        private ICommand _backCommand;

        public VisualEditViewModel(Action<object> onBackClicked)
        {
            _backCommand = new RelayCommand(onBackClicked);
        }

        public void AddModule(Modules.Module module)
        {
            Services.ServiceLocator.Locate<Services.BridgeService>().AddLayer(module.Name);
        }
    }
}
