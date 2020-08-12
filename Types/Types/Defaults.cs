using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Types
{
    public enum DefaultImplementedLibraries
    {
        IPC,

    }

    public static class Defaults
    {
        public const string ACTIVE_LIBRARY_IMPL_CONFIG = "active_impl.ini";

        public static string IPCImplementation
        {
            get => _data.Global["ipcimpl"];
            set => _data.Global["ipcimpl"] = value;
        }

        private static IniFileParser.Model.IniData _data;

        static Defaults()
        {
            if(System.IO.File.Exists(ACTIVE_LIBRARY_IMPL_CONFIG))
            {
                var parser = new IniFileParser.IniFileParser();
                _data = parser.ReadFile(ACTIVE_LIBRARY_IMPL_CONFIG);
            }
            else
            {
                _data = new IniFileParser.Model.IniData();

                // Set default implemnting libraries here.
                IPCImplementation = "Wallop.IPC.dll";

                // Save the file.
                ApplyChanges();
            }
        }

        public static void ApplyChanges()
        {
            var parser = new IniFileParser.IniFileParser();
            parser.WriteFile(ACTIVE_LIBRARY_IMPL_CONFIG, _data);
        }

        public static string GetDefaultLibrary(DefaultImplementedLibraries library)
        {
            return library switch
            {
                DefaultImplementedLibraries.IPC => Defaults.IPCImplementation,
                _ => "",
            };
        }
    }
}
