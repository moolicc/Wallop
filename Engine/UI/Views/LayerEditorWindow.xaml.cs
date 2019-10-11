using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace WallApp.UI.Views
{
    /// <summary>
    /// Interaction logic for LayerEditorWindow.xaml
    /// </summary>
    public partial class LayerEditorWindow : MetroWindow
    {
        private ViewModels.LayerSettingsViewModel _viewModel
        {
            get => (ViewModels.LayerSettingsViewModel)DataContext;
        }

        private bool _prompted;
        
        public LayerEditorWindow()
        {
            InitializeComponent();
            _prompted = false;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            for (int i = 0; i < textbox.Text.Length; i++)
            {
                if (!char.IsNumber(textbox.Text[i]))
                {
                    textbox.Text = textbox.Text.Remove(i, 1);
                    i--;
                }
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var canAccept = _viewModel.OkCommand.CanExecute(null);

            if(!_prompted)
            {
                var value = ShowClosingMessage(canAccept);
                e.Cancel = true;
            }
            else
            {
                _prompted = false;
            }
        }

        private async Task ShowClosingMessage(bool canAccept)
        {
            const string VALID = "Would you like to accept the changes or revert to previous settings?";
            const string INVALID = "There is an invalid data entry and you cannot accept in the current state.";

            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Accept",
                NegativeButtonText = "Revert",
                FirstAuxiliaryButtonText = "Cancel",
                ColorScheme = MetroDialogOptions.ColorScheme,
                AnimateShow = true,
                AnimateHide = true,
            };

            var style = MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary;
            var secondLine = VALID;
            if (!canAccept)
            {
                mySettings.AffirmativeButtonText = "Revert";
                mySettings.NegativeButtonText = "Cancel";
                style = MessageDialogStyle.AffirmativeAndNegative;
                secondLine = INVALID;
            }

            var result = await this.ShowMessageAsync("WallApp", "You have made changes to one or more layers.\r\n"
                + secondLine,
                style, mySettings);

            if (result == MessageDialogResult.Affirmative)
            {
                if (canAccept)
                {
                    _viewModel.OkCommand.Execute(null);
                    _prompted = true;
                    Close();
                }
                else
                {
                    _viewModel.CancelCommand.Execute(null);
                    _prompted = true;
                    Close();
                }
            }
            else if (result == MessageDialogResult.Negative)
            {
                if (canAccept)
                {
                    _viewModel.CancelCommand.Execute(null);
                    _prompted = true;
                    Close();
                }
                else
                {
                }
            }
            else if (result == MessageDialogResult.Canceled)
            {
            }
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.IsSelected)
            {
                _viewModel.FlyoutOpen = true; // !_viewModel.FlyoutOpen;
            }
        }
    }
}
