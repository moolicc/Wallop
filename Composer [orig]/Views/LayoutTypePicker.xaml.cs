using System;
using System.Windows;

namespace Wallop.Composer.Views
{
    /// <summary>
    /// Interaction logic for LayoutTypePicker.xaml
    /// </summary>
    public partial class LayoutTypePicker : MahApps.Metro.Controls.Dialogs.CustomDialog
    {
        public event Action<LayoutTypePicker> RequestClose;

        public bool Result { get; private set; }
        public string LayoutName => _textBox.Text;
        public Layout.ScriptKind SelectedScriptKind => (Layout.ScriptKind)_comboBox.SelectedIndex;
        public bool RememberSelection => _checkBox.IsChecked.Value;

        public LayoutTypePicker()
        {
            InitializeComponent();
            Result = false;
        }

        private void OkClicked(object sender, RoutedEventArgs e)
        {
            Result = true;
            RequestClose?.Invoke(this);
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Result = false;
            RequestClose?.Invoke(this);
        }
    }
}
