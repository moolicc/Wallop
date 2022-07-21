using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop
{
    internal static class EngineLog
    {
        private static NLog.LogFactory _logFactory;

        static EngineLog()
        {
            var configuration = new NLog.Config.LoggingConfiguration();
            var stdoutTarget = new NLog.Targets.ColoredConsoleTarget("stdout");

            stdoutTarget.UseDefaultRowHighlightingRules = true;
            stdoutTarget.Layout = "[ ${logger} : ${level} @ ${date:format=yyyy-MM-dd HH\\:MM\\:ss} ] >> ${message}";

            configuration.AddTarget(stdoutTarget);

#if DEBUG
            configuration.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, stdoutTarget);
#else
            configuration.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, stdoutTarget);
#endif

            _logFactory = new NLog.LogFactory();
            _logFactory.Configuration = configuration;
        }


        public static NLog.Logger For<T>()
            => For(typeof(T).Name);

        public static NLog.Logger For(string typeName)
            => _logFactory.GetLogger(typeName);

    }
}
