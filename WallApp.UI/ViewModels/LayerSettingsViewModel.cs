using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace WallApp.UI.ViewModels
{
    public class LayerSettingsViewModel : INotifyPropertyChanged
    {
        public List<LayerItemViewModel> Layers { get; private set; }
        
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

        public string LayerName
        {
            get => _layerName;
            set
            {
                if(_layerName == value)
                {
                    return;
                }
                _layerName = value;
                RaisePropertyChange(nameof(LayerName));
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
                RaisePropertyChange(nameof(LayerDescription));
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
                RaisePropertyChange(nameof(SelectedScreenIndex));
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
                RaisePropertyChange(nameof(AbsPositioning));
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
                RaisePropertyChange(nameof(MarginalPositioning));
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
                RaisePropertyChange(nameof(XPosition));
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
                RaisePropertyChange(nameof(YPosition));
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
                RaisePropertyChange(nameof(WPosition));
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
                RaisePropertyChange(nameof(ZPosition));
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
                RaisePropertyChange(nameof(TintColor));
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
                RaisePropertyChange(nameof(SelectedEffectIndex));
            }
        }

        public ICommand RevertCommand
        {
            get
            {
                if (_revertCommand == null)
                {
                    _revertCommand = new RelayCommand(OnRevert);
                }
                return _revertCommand;
            }
        }
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(OnClose);
                }
                return _closeCommand;
            }
        }


        private ICommand _moveUpCommand;
        private ICommand _moveDownCommand;
        private ICommand _newCommand;
        private ICommand _removeCommand;

        private string _layerName;
        private string _layerDescription;

        private int _selectedScreenIndex;
        private bool _absPositioning;
        private bool _marginalPositioning;
        private string _xPosition;
        private string _yPosition;
        private string _wPosition;
        private string _zPosition;

        private ICommand _tintCommand;
        private Color _tintColor;
        private int _selectedEffectIndex;

        private ICommand _revertCommand;
        private ICommand _closeCommand;

        public event PropertyChangedEventHandler PropertyChanged;


        public LayerSettingsViewModel(string[] screens, string[] effects)
        {
            Screens = new List<string>(screens);
            EffectItems = new List<string>(effects);

            _layerName = "layer";
            _layerDescription = "New layer";

            _selectedScreenIndex = 0;
            _absPositioning = false;
            _marginalPositioning = false;

            _xPosition = "0";
            _yPosition = "0";
            _wPosition = "1";
            _zPosition = "1";
            _tintColor = Colors.White;
            _selectedEffectIndex = 0;
        }

        public void OnSelectedLayerChange()
        {
             
        }

        private void RaisePropertyChange(string property)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
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

        }

        private void OnRemove(object args)
        {

        }

        private void OnTintChange(object args)
        {

        }

        private void OnRevert(object args)
        {

        }

        private void OnClose(object args)
        {

        }
    }
}
