using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WallApp.UI.Annotations;

namespace WallApp.UI.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string FrameRate
        {
            get => _frameRate;
            set
            {
                if(_frameRate == value)
                {
                    return;
                }
                _frameRate = value;
                OnPropertyChanged(nameof(FrameRate));
                _model.SetFrameRate(value);
            }
        }

        public string BackBufferScale
        {
            get => _backBufferScale;
            set
            {
                if (_backBufferScale == value)
                {
                    return;
                }
                _backBufferScale = value;
                OnPropertyChanged(nameof(BackBufferScale));
                _model.SetBackBufferScale(value);
            }
        }

        public List<LayerItemViewModel> Layers { get; private set; }
        public List<ModuleItemViewModel> Modules { get; private set; }

        public ICommand EditLayersCommand
        {
            get
            {
                if(_editLayersCommand == null)
                {
                    _editLayersCommand = new RelayCommand(OnEditLayers);
                }
                return _editLayersCommand;
            }
        }
        public ICommand ModulesCommand
        {
            get
            {
                if (_modulesCommand == null)
                {
                    _modulesCommand = new RelayCommand(OnGetModules);
                }
                return _modulesCommand;
            }
        }

        private ICommand _editLayersCommand;
        private ICommand _modulesCommand;

        private Models.SettingsModel _model;
        private string _frameRate;
        private string _backBufferScale;


        public SettingsViewModel(Models.SettingsModel model)
        {
            _model = model;

            Modules = new List<ModuleItemViewModel>();
            Layers = new List<LayerItemViewModel>();

            Modules.AddRange(_model.GetModuleItems());
            Layers.AddRange(_model.GetLayerItems());

            FrameRate = model.GetFrameRate();
            BackBufferScale = model.GetBackBufferScale();
        }

        public void OnEditLayers(object param)
        {

        }

        public void OnGetModules(object param)
        {

        }

        
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class ModuleItemViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Directory { get; set; }
        public string Version { get; set; }
    }
}
