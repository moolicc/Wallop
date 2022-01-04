using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cog.Sources
{
    public class CommandLineSource// : ISettingsSource
    {
        public bool CanSave => false;

        public bool HandleOrphanSettings => false;

        public bool HandleUnsavableSettings => false;

        public ICommandLineParser CommandLineParser { get; private set; }

        public CommandLineSource(ICommandLineParser parser)
        {
            CommandLineParser = parser;
        }

        public async Task<IEnumerable<KeyValuePair<string, object>>> LoadSettingsAsync()
        {
            return await Task.Run(() =>
            {
                var results = new List<KeyValuePair<string, object>>(5);
                while (CommandLineParser.ParseNext(out var result))
                {
                    results.Add(result);
                }
                return results;
            });
        }

        public Task SaveSettingsAsync(IEnumerable<KeyValuePair<string, object>> settings)
        {
            throw new NotImplementedException();
        }
    }
}
