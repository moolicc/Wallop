using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for VisualEditView.xaml
    /// </summary>
    public partial class VisualEditView : UserControl
    {
        internal Action<Modules.Module> OnAddModule { get; set; }

        public VisualEditView()
        {
            InitializeComponent();
        }

        public void AnimateOff()
        {
            var width = Window.GetWindow(this).Width;
            DoubleAnimation animation = new DoubleAnimation(0, -width - 3, new Duration(TimeSpan.FromMilliseconds(500)));
            animation.Completed += SetInvisible;
            IsEnabled = false;
            _transform.BeginAnimation(TranslateTransform.XProperty, animation);

            Services.ServiceLocator.Locate<Services.BridgeService>().SetEditMode(false);
        }

        public void AnimateOn()
        {
            var width = Window.GetWindow(this).Width;
            DoubleAnimation animation = new DoubleAnimation(-width - 3, 0, new Duration(TimeSpan.FromMilliseconds(500)));
            animation.Completed += SetVisible;
            IsEnabled = true;
            _transform.BeginAnimation(TranslateTransform.XProperty, animation);

            Services.ServiceLocator.Locate<Services.BridgeService>().SetEditMode(true);
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

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            //OR
            /*
             * var item = ListView.SelectedItem as Track;
             * if (item != null)
             * {
             * MessageBox.Show(item.ToString()+" Double Click handled!");
             * }
            */

            var item = ((FrameworkElement)e.OriginalSource).DataContext as Modules.Module;
            if (item == null || OnAddModule == null)
            {
                return;
            }
            OnAddModule(item);
        }
    }
}
