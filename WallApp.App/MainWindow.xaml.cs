using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls.Dialogs;

namespace WallApp.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private ViewModels.MainViewModel _mainViewModel;
        private ViewModels.VisualEditViewModel _editViewModel;


        public MainWindow()
        {
            InitializeComponent();
            SetupMainView();
            _editViewModel = new ViewModels.VisualEditViewModel(_ =>
            {
                SwitchToMainView();
            });
        }

        private void SetupMainView()
        {
            _visualEditView._transform.X = -Width - 3;
            _mainViewModel = new ViewModels.MainViewModel();
            DataContext = _mainViewModel;
        }


        private async void NewLayoutClicked(object sender, RoutedEventArgs e)
        {
            var view = new Views.LayoutTypePicker();
            view.RequestClose += LayoutTypePickerRequestedClose;
            await this.ShowMetroDialogAsync(view);
        }

        private async void LayoutTypePickerRequestedClose(Views.LayoutTypePicker sender)
        {
            sender.RequestClose -= LayoutTypePickerRequestedClose;

            await this.HideMetroDialogAsync(sender);
            if(!sender.Result)
            {
                return;
            }

            SwitchToEditView();
        }

        private void SwitchToMainView()
        {
            _visualEditView.AnimateOff();
            _mainView.AnimateOn();
            DataContext = _mainViewModel;
        }

        private void SwitchToEditView()
        {
            _visualEditView.AnimateOn();
            _mainView.AnimateOff();
            DataContext = _editViewModel;
        }
    }
}
