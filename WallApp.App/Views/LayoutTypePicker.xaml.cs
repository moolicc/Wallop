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
using System.Windows.Shapes;

namespace WallApp.App.Views
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
