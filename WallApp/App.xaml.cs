using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WallApp.Services;
using WallApp.UI;

namespace WallApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ThemeWatcher _themeWatcher;

        internal App()
        {
            _themeWatcher = new UI.ThemeWatcher();
            _themeWatcher.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            var accentTheme = "Blue";
            var baseTheme = "BaseLight";
            if(_themeWatcher.WindowsTheme == UI.WindowsTheme.Dark)
            {
                accentTheme = "Red";
                baseTheme = "BaseDark";
            }
            ThemeManager.ChangeAppStyle(Application.Current,
                ThemeManager.GetAccent(accentTheme),
                ThemeManager.GetAppTheme(baseTheme));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _themeWatcher.WatchTheme();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            ServiceProvider.KillReferences();
        }
    }
}
