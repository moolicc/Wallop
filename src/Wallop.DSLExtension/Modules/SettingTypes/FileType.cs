using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.DSLExtension.Modules.SettingTypes
{
    public class FileType : ISettingType
    {
        public string Name => "file";

        public string Serialize(object value, IEnumerable<KeyValuePair<string, string>>? args)
        {
            if(value is FileInfo fi)
            {
                return fi.FullName;
            }
            else if(value is FileStream fs)
            {
                return fs.Name;
            }
            throw new ArgumentException("File setting only supports serializing FileInfo and FileStream objects.", nameof(value));
        }

        public bool TryDeserialize(string value, out object? result, IEnumerable<KeyValuePair<string, string>>? args)
        {
            result = null;

            try
            {
                result = new FileInfo(value);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public bool TrySerialize(object value, out string? result, IEnumerable<KeyValuePair<string, string>>? args)
        {
            result = null;
            if (value is FileInfo fi)
            {
                result = fi.FullName;
            }
            else if (value is FileStream fs)
            {
                result = fs.Name;
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
