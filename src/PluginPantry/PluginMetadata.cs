using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PluginPantry
{
    public class PluginMetadata
    {
        public PluginContext? OwningContext { get; internal set; }

        public string Name { get; private set; }
        public Version Version { get; private set; }
        public Type EntryType { get; private set; }
        public MethodInfo EntryPoint { get; private set; }
        public string AssemblyPath { get; private set; }

        public string Id => CreateId(this);


        internal PluginMetadata(string name, Version version, Type entryType, MethodInfo entryPoint, string assemblyPath)
        {
            Name = name;
            Version = version;
            EntryType = entryType;
            EntryPoint = entryPoint;
            AssemblyPath = assemblyPath;
        }

        private static string CreateId(PluginMetadata plugin)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < plugin.Name.Length; i++)
            {
                char curChar = plugin.Name[i];

                if (char.IsWhiteSpace(curChar)
                    || char.IsSymbol(curChar))
                {
                    curChar = '.';
                }
                builder.Append(curChar);
            }

            builder.Append(plugin.Version.Major).Append('.');
            builder.Append(plugin.Version.Minor).Append('.');
            builder.Append(plugin.Version.Build).Append('.');
            builder.Append(plugin.Version.Revision);

            return builder.ToString();
        }
    }
}
