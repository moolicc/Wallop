using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace WallApp.UI.ViewModels
{
    public class LayerSettingsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<LayerItemViewModel> Layers { get; private set; }

        public ICommand MoveUpCommand
        {
            get
            {
                if (_moveUpCommand == null)
                {
                    _moveUpCommand = new RelayCommand(OnMoveUp);
                }
                return _moveUpCommand;
            }
        }
        public ICommand MoveDownCommand
        {
            get
            {
                if (_moveDownCommand == null)
                {
                    _moveDownCommand = new RelayCommand(OnMoveDown);
                }
                return _moveDownCommand;
            }
        }
        public ICommand NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new RelayCommand(OnAdd);
                }
                return _newCommand;
            }
        }
        public ICommand RemoveCommand
        {
            get
            {
                if (_removeCommand == null)
                {
                    _removeCommand = new RelayCommand(OnRemove);
                }
                return _removeCommand;
            }
        }
        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(OnOk, o => string.IsNullOrWhiteSpace(ErrorText));
                }
                return _okCommand;
            }
        }
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(OnCancel);
                }
                return _cancelCommand;
            }
        }

        public int SelectedLayerIndex
        {
            get => _selectedLayerIndex;
            set
            {
                if (_selectedLayerIndex == value || (value < 0 && Layers.Count > 0))
                {
                    return;
                }
                _selectedLayerIndex = value;
                OnSelectedLayerChange();
                RaisePropertyChange(nameof(SelectedLayerIndex), true);
                FlyoutOpen = true;
            }
        }

        public string LayerName
        {
            get => _layerName;
            set
            {
                if (_layerName == value)
                {
                    return;
                }
                _layerName = value;
                RaisePropertyChange(nameof(LayerName), true);
            }
        }
        public string LayerDescription
        {
            get => _layerDescription;
            set
            {
                if (_layerDescription == value)
                {
                    return;
                }
                _layerDescription = value;
                RaisePropertyChange(nameof(LayerDescription), true);
            }
        }
        public List<string> ModuleItems { get; private set; }
        public int SelectedModuleIndex
        {
            get => _selectedModuleIndex;
            set
            {
                if (_selectedModuleIndex == value)
                {
                    return;
                }
                _selectedModuleIndex = value;
                RaisePropertyChange(nameof(SelectedModuleIndex), true);
                RaisePropertyChange(nameof(SelectedModuleName), false);
                LoadModuleView();
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                {
                    return;
                }
                _enabled = value;
                RaisePropertyChange(nameof(Enabled), true);
            }
        }

        public List<string> Screens { get; private set; }
        public int SelectedScreenIndex
        {
            get => _selectedScreenIndex;
            set
            {
                if (_selectedScreenIndex == value)
                {
                    return;
                }
                _selectedScreenIndex = value;
                RaisePropertyChange(nameof(SelectedScreenIndex), true);
            }
        }
        public bool AbsPositioning
        {
            get => _absPositioning;
            set
            {
                if (_absPositioning == value)
                {
                    return;
                }
                _absPositioning = value;
                RaisePropertyChange(nameof(AbsPositioning), true);
            }
        }
        public bool MarginalPositioning
        {
            get => _marginalPositioning;
            set
            {
                if (_marginalPositioning == value)
                {
                    return;
                }
                _marginalPositioning = value;
                RaisePropertyChange(nameof(MarginalPositioning), true);
            }
        }
        public string XPosition
        {
            get => _xPosition;
            set
            {
                if (_xPosition == value)
                {
                    return;
                }
                _xPosition = value;
                RaisePropertyChange(nameof(XPosition), true);
            }
        }
        public string YPosition
        {
            get => _yPosition;
            set
            {
                if (_yPosition == value)
                {
                    return;
                }
                _yPosition = value;
                RaisePropertyChange(nameof(YPosition), true);
            }
        }
        public string ZPosition
        {
            get => _zPosition;
            set
            {
                if (_zPosition == value)
                {
                    return;
                }
                _zPosition = value;
                RaisePropertyChange(nameof(ZPosition), true);
            }
        }
        public string WPosition
        {
            get => _wPosition;
            set
            {
                if (_wPosition == value)
                {
                    return;
                }
                _wPosition = value;
                RaisePropertyChange(nameof(WPosition), true);
            }
        }

        public ICommand TintCommand
        {
            get
            {
                if (_tintCommand == null)
                {
                    _tintCommand = new RelayCommand(OnTintChange);
                }
                return _tintCommand;
            }
        }

        public Color TintColor
        {
            get => _tintColor;
            set
            {
                if (_tintColor == value)
                {
                    return;
                }
                _tintColor = value;
                RaisePropertyChange(nameof(TintColor), true);
            }
        }
        public List<string> EffectItems { get; private set; }
        public int SelectedEffectIndex
        {
            get => _selectedEffectIndex;
            set
            {
                if (_selectedEffectIndex == value)
                {
                    return;
                }
                _selectedEffectIndex = value;
                RaisePropertyChange(nameof(SelectedEffectIndex), true);
            }
        }

        public string ID
        {
            get => $"ID: {_idText}";
            set
            {
                if (_idText == value)
                {
                    return;
                }
                _idText = value;
                RaisePropertyChange(nameof(ID), true);
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
                RaisePropertyChange(nameof(ErrorTextVisible), false);
            }
        }
        public Visibility ErrorTextVisible
        {
            get => string.IsNullOrWhiteSpace(ErrorText) ? Visibility.Hidden : Visibility.Visible;
        }

        public bool FlyoutOpen
        {
            get => _flyoutOpen;
            set
            {
                if(_flyoutOpen == value)
                {
                    return;
                }
                _flyoutOpen = value;
                RaisePropertyChange(nameof(FlyoutOpen), false);
            }
        }

        public string SelectedModuleName
        {
            get => ModuleItems[SelectedModuleIndex];
        }

        public object ModuleView
        {
            get => _moduleView;
            set
            {
                if (_moduleView == value)
                {
                    return;
                }
                _moduleView = value;
                RaisePropertyChange(nameof(ModuleView), false);
            }
        }


        private ICommand _moveUpCommand;
        private ICommand _moveDownCommand;
        private ICommand _newCommand;
        private ICommand _removeCommand;

        private ICommand _okCommand;
        private ICommand _cancelCommand;

        private int _selectedLayerIndex;

        private string _layerName;
        private string _layerDescription;
        private int _selectedModuleIndex;
        private bool _enabled;

        private int _selectedScreenIndex;
        private bool _absPositioning;
        private bool _marginalPositioning;
        private string _xPosition;
        private string _yPosition;
        private string _zPosition;
        private string _wPosition;

        private ICommand _tintCommand;
        private Color _tintColor;
        private int _selectedEffectIndex;
        private string _idText;

        private string _errorText;
        private bool _flyoutOpen;
        private object _moduleView;

        private Models.LayerSettingsModel _model;
        private bool _updating;

        public event PropertyChangedEventHandler PropertyChanged;


        public LayerSettingsViewModel()
        {
            _model = ModelProvider.Instance.GetLayerSettingsModel();

            Layers = new ObservableCollection<LayerItemViewModel>();
            ModuleItems = new List<string>(_model.GetModules());
            Screens = new List<string>(_model.GetScreens());
            EffectItems = new List<string>(_model.GetEffects());

            foreach (var item in _model.GetLayerItems())
            {
                Layers.Add(item);
            }
            _selectedLayerIndex = 0;

            _layerName = "layer";
            _layerDescription = "";
            _selectedModuleIndex = 0;
            _enabled = true;

            _selectedScreenIndex = 0;
            _absPositioning = false;
            _marginalPositioning = false;

            _xPosition = "0";
            _yPosition = "0";
            _wPosition = "1";
            _zPosition = "1";
            _tintColor = Colors.White;
            _selectedEffectIndex = 0;
            _idText = "0";
            _updating = false;

            if (Layers.Count == 0)
            {
            }
            else
            {
                OnSelectedLayerChange();
            }
        }

        public void OnSelectedLayerChange()
        {
            _model.SetActiveLayer(_selectedLayerIndex);
            UpdateForLayerChange();
        }

        private void UpdateForLayerChange()
        {
            //Updates all properties using data from _model.x
            _updating = true;
            var curLayer = _model.GetCurrentLayer();
            LayerName = curLayer.Name;
            LayerDescription = curLayer.Description;
            Enabled = curLayer.Enabled;
            SelectedModuleIndex = curLayer.ModuleIndex;
            SelectedScreenIndex = curLayer.ScreenIndex;
            AbsPositioning = curLayer.AbsToggle;
            MarginalPositioning = curLayer.MarginsToggle;
            XPosition = curLayer.CurrentPosition.X.ToString();
            YPosition = curLayer.CurrentPosition.Y.ToString();
            ZPosition = curLayer.CurrentPosition.Z.ToString();
            WPosition = curLayer.CurrentPosition.W.ToString();
            TintColor = Color.FromArgb(curLayer.TintColor.A, curLayer.TintColor.R, curLayer.TintColor.G, curLayer.TintColor.B);
            SelectedEffectIndex = curLayer.EffectIndex;
            ID = curLayer.ID.ToString();
            _updating = false;
        }

        private void UpdateModel()
        {
            if(_updating)
            {
                return;
            }
            //Updates all properties in _model.x
            var curLayer = _model.GetCurrentLayer();

            if (!int.TryParse(_xPosition, out var x) || !int.TryParse(_yPosition, out var y)
                || !int.TryParse(_zPosition, out var z) || !int.TryParse(_wPosition, out var w))
            {
                ErrorText = "Invalid position specified";
                return;
            }

            curLayer.Name = _layerName;
            curLayer.Description = _layerDescription;
            curLayer.ModuleIndex = _selectedModuleIndex;
            curLayer.ModuleName = ModuleItems[_selectedModuleIndex];
            curLayer.Enabled = _enabled;
            curLayer.ScreenIndex = _selectedScreenIndex;
            curLayer.AbsToggle = _absPositioning;
            curLayer.MarginsToggle = _marginalPositioning;
            curLayer.CurrentPosition = (x, y, z, w);
            curLayer.TintColor = (_tintColor.A, _tintColor.R, _tintColor.G, _tintColor.B);
            curLayer.EffectIndex = _selectedEffectIndex;

            var valid = _model.UpdateCurrentLayer();
            ErrorText = valid.Message;

            Layers[_selectedLayerIndex] = _model.GetCurrentLayerUI();
            RaisePropertyChange(nameof(Layers), false);
        }

        private void RaisePropertyChange(string property, bool updateModel)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
            if(updateModel)
            {
                UpdateModel();
            }
        }

        private void OnMoveUp(object args)
        {

        }

        private void OnMoveDown(object args)
        {

        }

        private void OnAdd(object args)
        {
            if(!string.IsNullOrWhiteSpace(ErrorText))
            {
                MessageBox.Show($"Resolve the following layer error before adding another layer:{Environment.NewLine}{ErrorText}.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            Layers.Add(_model.AddNewLayerItem());
            RaisePropertyChange(nameof(Layers), true);
            SelectedLayerIndex = Layers.Count - 1;
        }

        private void OnRemove(object args)
        {
            if (SelectedLayerIndex < 0 || SelectedLayerIndex >= Layers.Count)
            {
                return;
            }
            Layers.RemoveAt(SelectedLayerIndex);
            SelectedLayerIndex = _model.RemoveSelectedLayer();
            OnSelectedLayerChange();
        }

        private void OnTintChange(object args)
        {

        }

        private void OnOk(object args)
        {
            _model.Exit(true);
        }

        private void OnCancel(object args)
        {
            _model.Exit(false);
        }

        private void LoadModuleView()
        {
            ModuleView = _model.GetLayerSettingsView();
        }
    }
}
