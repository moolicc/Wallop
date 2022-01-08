using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cog.Sources
{
    public interface ICommandLineParser
    {
        bool ParseNext(out KeyValuePair<string, object> result);
    }
}
