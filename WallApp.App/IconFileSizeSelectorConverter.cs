using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WallApp.App
{
    //Source: https://stackoverflow.com/a/54738646
    class IconFileSizeSelectorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var size = string.IsNullOrWhiteSpace(parameter?.ToString()) ? 0 : System.Convert.ToInt32(parameter);

            var uri = value?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(uri))
                return null;

            if (!uri.StartsWith("pack:"))
                uri = $"pack://application:,,,{uri}";

            var decoder = BitmapDecoder.Create(new Uri(uri), BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand);

            var result = decoder.Frames.Where(f => f.Width <= size).OrderByDescending(f => f.Width).FirstOrDefault()
                ?? decoder.Frames.OrderBy(f => f.Width).FirstOrDefault();

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
