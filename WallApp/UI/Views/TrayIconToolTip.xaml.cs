using System;
using System.Collections.Generic;
using System.IO;
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

namespace WallApp.UI.Views
{
    /// <summary>
    /// Interaction logic for TrayIconToolTip.xaml
    /// </summary>
    public partial class TrayIconToolTip : UserControl
    {
        public ImageSource ImageSource
        {
            get => _image.Source;
            set
            {
                _image.Source = value;
            }
        }

        public string Title
        {
            get => _titleBlock.Text;
            set => _titleBlock.Text = value;
        }
        public string Text
        {
            get => _textBlock.Text;
            set => _textBlock.Text = value;
        }


        public TrayIconToolTip()
        {
            InitializeComponent();
        }
    }
}
