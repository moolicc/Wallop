using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WallApp.App.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<Layout.LayoutInfo> Layouts { get; private set; }

        public string FlipViewBannerText
        {
            get { return _flipViewBannerText; }
            private set
            {
                if(_flipViewBannerText == value)
                {
                    return;
                }
                _flipViewBannerText = value;
                OnPropertyChanged(nameof(FlipViewBannerText));
            }
        }

        public bool ProgressRingActive
        {
            get { return _progressRingActive; }
            private set
            {
                if(value == _progressRingActive)
                {
                    return;
                }
                _progressRingActive = value;
                OnPropertyChanged(nameof(ProgressRingActive));
                OnPropertyChanged(nameof(ControlsEnabled));
            }
        }
        public int SelectedLayoutIndex
        {
            get => _selectedLayoutIndex;
            set
            {
                if (_selectedLayoutIndex == value)
                {
                    return;
                }
                _selectedLayoutIndex = value;
                FlipViewBannerText = Layouts[value].Title;
                OnPropertyChanged(nameof(SelectedLayoutIndex));
            }
        }
        public bool ControlsEnabled => !ProgressRingActive;

        public ICommand AddLayoutCommand => _addLayoutCommand;
        public ICommand RemoveLayoutCommand => _removeLayoutCommand;
        public ICommand ImportLayoutCommand => _importLayoutCommand;
        public ICommand ExportLayoutCommand => _exportLayoutCommand;
        public ICommand ApplyLayoutCommand => _applyLayoutCommand;
        public ICommand SettingsCommand => _settingsCommand;

        private string _flipViewBannerText;
        private bool _progressRingActive;
        private int _selectedLayoutIndex;
        private ICommand _addLayoutCommand;
        private ICommand _removeLayoutCommand;
        private ICommand _importLayoutCommand;
        private ICommand _exportLayoutCommand;
        private ICommand _applyLayoutCommand;
        private ICommand _settingsCommand;

        public MainViewModel()
        {
            _addLayoutCommand = new RelayCommand(o =>
            {

            }, _ => ControlsEnabled);
            _removeLayoutCommand = new RelayCommand((o) =>
            {

            }, _ => ControlsEnabled);
            _importLayoutCommand = new RelayCommand((o) =>
            {

            }, _ => ControlsEnabled);
            _exportLayoutCommand = new RelayCommand((o) =>
            {

            }, _ => ControlsEnabled);
            _applyLayoutCommand = new RelayCommand((o) =>
            {

            }, _ => ControlsEnabled);
            _settingsCommand = new RelayCommand((o) =>
            {

            });
            _flipViewBannerText = "";

            var startup = Services.ServiceLocator.Locate<Services.StartupService>();
            startup.StartupComplete += StartupComplete;

            Layouts = new List<Layout.LayoutInfo>(Services.ServiceLocator.Locate<Services.LayoutService>().LoadLayouts());
        }

        private void OnPropertyChanged(string property)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        private void StartupComplete(object sender, EventArgs e)
        {
            var service = (Services.StartupService)sender;

            ProgressRingActive = false;

            service.StartupComplete -= StartupComplete;
        }
    }
}
