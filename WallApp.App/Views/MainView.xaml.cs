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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WallApp.App.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public event EventHandler<RoutedEventArgs> NewLayoutClick;
        public event EventHandler<SelectionChangedEventArgs> SelectedLayoutChanged;

        public MainView()
        {
            InitializeComponent();
        }

        private void NewLayoutClicked(object sender, RoutedEventArgs e)
        {
            NewLayoutClick?.Invoke(sender, e);
        }

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedLayoutChanged?.Invoke(sender, e);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var width = Window.GetWindow(this).Width;
            DoubleAnimation animation = new DoubleAnimation(-width - 3, 0, new Duration(TimeSpan.FromMilliseconds(500)));
            _transform.BeginAnimation(TranslateTransform.XProperty, animation);
        }
        public void AnimateOff()
        {
            var width = Window.GetWindow(this).Width;
            DoubleAnimation animation = new DoubleAnimation(0, width + 3, new Duration(TimeSpan.FromMilliseconds(500)));
            animation.Completed += SetInvisible;
            IsEnabled = false;
            _transform.BeginAnimation(TranslateTransform.XProperty, animation);

        }

        public void AnimateOn()
        {
            Visibility = Visibility.Visible;

            var width = Window.GetWindow(this).Width;
            DoubleAnimation animation = new DoubleAnimation(width + 3, 0, new Duration(TimeSpan.FromMilliseconds(500)));
            animation.Completed += SetVisible;
            IsEnabled = true;
            _transform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void SetInvisible(object sender, EventArgs args)
        {
            ((AnimationClock)sender).Completed -= SetInvisible;
            Visibility = Visibility.Collapsed;
        }

        private void SetVisible(object sender, EventArgs args)
        {
            ((AnimationClock)sender).Completed -= SetVisible;
            Visibility = Visibility.Visible;
        }
    }
}
