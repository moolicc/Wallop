using System;
using System.Collections.Generic;
using System.Windows.Input;
using WallApp.App.Services;

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

        public void StartEdit(Layout.LayoutInfo layout)
        {
            ServiceLocator.Locate<EditModeService>().StartEdit(layout);
        }

        public void StopEdit()
        {
            ServiceLocator.Locate<EditModeService>().StopEdit();
        }

        public void AddModule(Modules.Module module)
        {
            ServiceLocator.Locate<EditModeService>().AddLayer(module.Name);
        }
    }
}
