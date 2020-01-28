using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;

namespace Wallop.Composer
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

            // TODO: Probably a better way to do this, but I haven't slept in a while :(.
            _visualEditView.OnAddModule = _editViewModel.AddModule;
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
            if (!sender.Result)
            {
                return;
            }

            var layout = new Layout.LayoutInfo()
            {
                Author = "Me",
                Title = sender.LayoutName,
                ScriptIsData = sender.SelectedScriptKind == Layout.ScriptKind.Data,
                Script = "",
                Thumbnail = ""
            };

            SwitchToEditView(new Layout.LayoutInfo());
        }

        private void SwitchToMainView()
        {
            _visualEditView.AnimateOff();
            _editViewModel.StopEdit();
            _mainView.AnimateOn();
            DataContext = _mainViewModel;
        }

        private void SwitchToEditView(Layout.LayoutInfo layout)
        {
            _visualEditView.AnimateOn();
            _mainView.AnimateOff();
            _editViewModel.StartEdit(layout);
            DataContext = _editViewModel;
        }
    }
}
