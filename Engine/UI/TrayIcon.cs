using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WallApp.Services;

namespace WallApp.UI
{
    [Service]
    class TrayIcon : InitializableService
    {
        public Exception LastException { get; private set; }

        private TaskbarIcon _taskbarIcon;
        private ImageSource _icon;
        private ImageSource _errorIcon;

        [ServiceReference(Key = "/main")]
        private Layout _layout;

        public TrayIcon()
        {
        }

        public void Init(TaskbarIcon taskbarIcon)
        {
            _taskbarIcon = taskbarIcon;
            _icon = (ImageSource)App.Current.FindResource("icon");
            _errorIcon = (ImageSource)App.Current.FindResource("erroricon");

            base.Initialize();
        }

        public void SetLayerError(int layerId, Exception exception, string message)
        {
            LastException = exception;
            App.Current.Dispatcher.Invoke(() =>
            {
                string name = "WallApp";
                if(layerId >= _layout.Layers.Count)
                {
                    name = "Unkown Layer";
                }
                else if(layerId >= 0)
                {
                    name = _layout.Layers[layerId].Name;
                }
                SetTooltip(_errorIcon, name, message);
                _taskbarIcon.IconSource = _errorIcon;
                _taskbarIcon.InvalidateVisual();
            });
        }

        public void RemoveLayerError()
        {
            LastException = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                SetTooltip(_icon, "WallApp", $"{_layout.Layers.Where(l => l.Enabled).Count()} enabled layers");
                _taskbarIcon.IconSource = _icon;
                _taskbarIcon.InvalidateVisual();
            });
        }

        private void SetTooltip(ImageSource image, string title, string text)
        {
            CheckInitialized();


            var tooltip = new Views.TrayIconToolTip();
            tooltip.ImageSource = image;
            tooltip.Title = title;
            tooltip.Text = text;
            _taskbarIcon.ToolTip = tooltip;
            _taskbarIcon.ToolTipText = $"{title}: {text}";
        }
    }
}
