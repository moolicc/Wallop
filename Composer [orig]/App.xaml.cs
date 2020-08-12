using System;
using System.Collections.Generic;
using System.Windows;

namespace Wallop.Composer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //TODO: Refactor: Rename Module to Manifest. DON'T FORGET XAML BINDINGS.


        public static List<string> TempFiles { get; private set; }
        public static string BaseDir => AppDomain.CurrentDomain.BaseDirectory;

        public static string CreateTempFile()
        {
            string result = System.IO.Path.GetTempFileName();
            TempFiles.Add(result);
            return result;
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            TempFiles = new List<string>();

            Services.StartupService ss = new Services.StartupService();
            Services.ServiceLocator.RegisterService(ss);
            ss.InitApp();

            base.OnStartup(e);
        }
    }
}
