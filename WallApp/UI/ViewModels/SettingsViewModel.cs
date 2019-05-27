using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace WallApp.UI.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand EditLayersCommand
        {
            get
            {
                if (_editLayersCommand == null)
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

        public double FrameRate
        {
            get => _frameRate;
            set
            {
                if (_frameRate == value)
                {
                    return;
                }
                _frameRate = value;
                RaisePropertyChange(nameof(FrameRate), true);
            }
        }

        public double BackBufferScale
        {
            get => _backBufferScale;
            set
            {
                if (_backBufferScale == value)
                {
                    return;
                }
                _backBufferScale = value;
                RaisePropertyChange(nameof(BackBufferScale), true);
            }
        }
        public string LayersText
        {
            get => _layersText;
            set
            {
                if (_layersText == value)
                {
                    return;
                }
                _layersText = value;
                RaisePropertyChange(nameof(LayersText), true);
            }
        }
        public string ModulesText
        {
            get => _modulesText;
            set
            {
                if (_modulesText == value)
                {
                    return;
                }
                _modulesText = value;
                RaisePropertyChange(nameof(ModulesText), true);
            }
        }

        public string ErrorText
        {
            get => _errorText;
            set
            {
                if (_errorText == value)
                {
                    return;
                }
                _errorText = value;
                RaisePropertyChange(nameof(ErrorText), false);
                RaisePropertyChange(nameof(ErrorTextVisibility), false);
            }
        }
        public Visibility ErrorTextVisibility
        {
            get => string.IsNullOrWhiteSpace(ErrorText) ? Visibility.Hidden : Visibility.Visible;
        }

        private ICommand _editLayersCommand;
        private ICommand _modulesCommand;

        private double _frameRate;
        private double _backBufferScale;
        private string _layersText;
        private string _modulesText;
        private string _errorText;

        private Models.SettingsModel _model;
        private bool _updatingViewModel;

        private Views.LayerEditorWindow _layerEditorWindow;


        public SettingsViewModel()
        {
            _model = ModelProvider.Instance.GetSettingsModel();
            UpdateViewModel();
        }

        public void OnEditLayers(object param)
        {
            if(_layerEditorWindow != null)
            {
                return;
            }
            _layerEditorWindow = new Views.LayerEditorWindow();
            _layerEditorWindow.Closed += LayerEditorWindowClosed;

            var viewModel = new LayerSettingsViewModel();
            _layerEditorWindow.DataContext = viewModel;

            _layerEditorWindow.ShowDialog();
            UpdateViewModel();
        }

        private void LayerEditorWindowClosed(object sender, System.EventArgs e)
        {
            _layerEditorWindow.Closed -= LayerEditorWindowClosed;
            _layerEditorWindow = null;
        }

        public void OnGetModules(object param)
        {

        }

        public void OnClosed()
        {
            _model.Apply();
        }

        private void RaisePropertyChange(string property, bool updateModel)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
            if (updateModel)
            {
                UpdateModel();
            }
        }

        private void UpdateModel()
        {
            if (_updatingViewModel)
            {
                return;
            }
            _model.FrameRate = (int)_frameRate;
            _model.BackBufferScale = _backBufferScale;
        }
        private void UpdateViewModel()
        {
            _updatingViewModel = true;
            FrameRate = _model.FrameRate;
            BackBufferScale = _model.BackBufferScale;
            LayersText = GetPrettyNumberText("{0} active layer{1}", _model.GetLayerCount());
            ModulesText = GetPrettyNumberText("{0} active module{1}", _model.GetModuleCount());
            _updatingViewModel = false;
        }

        private string GetPrettyNumberText(string input, int value)
        {
            if (value == 0)
            {
                return string.Format(input, "No", "s");
            }
            else if (value == 1)
            {
                return string.Format(input, value, "");
            }
            return string.Format(input, value, "s");
        }
    }
}
