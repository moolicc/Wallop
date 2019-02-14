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

namespace WallApp.UI.Views
{
    /// <summary>
    /// Interaction logic for LayerEditorWindow.xaml
    /// </summary>
    public partial class LayerEditorWindow : Window
    {
        public LayerEditorWindow()
        {
            InitializeComponent();
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
    }
}
