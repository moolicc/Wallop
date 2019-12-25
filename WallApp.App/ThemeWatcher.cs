using Microsoft.Win32;
using System;
using System.Globalization;
using System.Management;
using System.Security.Principal;
using System.Windows;

//Original source: https://engy.us/blog/2018/10/20/dark-theme-in-wpf/
namespace WallApp.App
{
    public enum WindowsTheme
    {
        Light,
        Dark,
    }

    public enum AppTheme
    {
        Light,
        Dark,
        HighContrast,
    }

    public class ThemeWatcher
    {

        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private const string RegistryValueName = "AppsUseLightTheme";

        public event EventHandler ThemeChanged;

        public WindowsTheme WindowsTheme { get; private set; }
        public AppTheme CurrentTheme { get; private set; }

        private bool _isHighContrastEnabled;

        public void WatchTheme()
        {
            var currentUser = WindowsIdentity.GetCurrent();
            string query = string.Format(
                CultureInfo.InvariantCulture,
                @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
                currentUser.User.Value,
                RegistryKeyPath.Replace(@"\", @"\\"),
                RegistryValueName);

            try
            {
                var watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += (sender, args) =>
                {
                    CheckNewAppTheme();
                };

                // Start listening for events
                watcher.Start();
            }
            catch (Exception)
            {
                // This can fail on Windows 7
            }


            SystemParameters.StaticPropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(SystemParameters.HighContrast))
                {
                    CheckNewAppTheme();
                }
            };

            CheckNewAppTheme();
        }

        private void CheckNewAppTheme()
        {
            _isHighContrastEnabled = SystemParameters.HighContrast;
            WindowsTheme = GetWindowsTheme();
            if (_isHighContrastEnabled)
            {
                CurrentTheme = AppTheme.HighContrast;
            }
            else
            {
                //This is OK because Light and Dark have corresponding numerical values (0, and 1 respectively).
                CurrentTheme = (AppTheme)WindowsTheme;
            }
            ThemeChanged?.Invoke(this, new EventArgs());
        }

        private static WindowsTheme GetWindowsTheme()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
            {
                object registryValueObject = key?.GetValue(RegistryValueName);
                if (registryValueObject == null)
                {
                    return WindowsTheme.Light;
                }

                int registryValue = (int)registryValueObject;

                return registryValue > 0 ? WindowsTheme.Light : WindowsTheme.Dark;
            }
        }

    }
}
