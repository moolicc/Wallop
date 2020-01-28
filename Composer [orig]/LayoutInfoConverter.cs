using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Wallop.Composer
{
    class LayoutInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = (Layout.LayoutInfo)value;
            var bitmap = new BitmapImage(new Uri(item.Thumbnail, UriKind.Absolute));
            var panel = new Grid();
            var image = new Image();
            image.Stretch = Stretch.None;
            image.Source = bitmap;
            panel.Background = new SolidColorBrush(Colors.ForestGreen);
            panel.Children.Add(image);
            return panel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
