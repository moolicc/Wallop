using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using System.Text;
using Wallop.DSLExtension.Modules;

namespace Wallop.Engine
{
    [LayoutRenderer("modulelogger")]
    class ModuleLoggerLayout : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if(logEvent.Properties.TryGetValue("module", out var module0) && module0 is Module module)
            {
                builder.Append(module.ModuleInfo.Id).Append('+').Append(logEvent.LoggerName);
            }
        }
    }


    static class ModuleLog
    {
        private static NLog.LogFactory _logFactory;

        //TODO: Modules should be able to log to files.

        static ModuleLog()
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("modulelogger", typeof(ModuleLoggerLayout));

            var configuration = new NLog.Config.LoggingConfiguration();
            var stdoutTarget = new NLog.Targets.ColoredConsoleTarget("stdout");


            stdoutTarget.UseDefaultRowHighlightingRules = true;
            stdoutTarget.Layout = "[*MODULE* ${modulelogger} : ${level} @ ${date:format=yyyy-MM-dd HH\\:MM\\:ss} ] >> ${message}";


            configuration.AddTarget(stdoutTarget);

#if DEBUG
            configuration.AddRule(LogLevel.Trace, LogLevel.Fatal, stdoutTarget);
#else
            configuration.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, stdoutTarget);
#endif

            _logFactory = new NLog.LogFactory();
            _logFactory.Configuration = configuration;
        }

        public static Logger For(Module module, string instanceName)
        {
            var logger = _logFactory.GetLogger(instanceName);
            logger.Properties["module"] = module;
            return logger;
        }

    }
}
