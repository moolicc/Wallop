using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp
{
    static class Extensions
    {
        public static bool IsNull(this string input)
        {
            return string.IsNullOrWhiteSpace(input);
        }
    }
}
