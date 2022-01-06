using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginPantry
{
    public class PluginContext
    {
        internal Exposed Exposed { get; set; }


        private List<PluginMetadata> _loadedPlugins;

        public PluginContext()
        {
            Exposed = new Exposed(this);
            _loadedPlugins = new List<PluginMetadata>();
        }

        ~PluginContext()
        {

        }

        public void IncludePlugin(PluginMetadata plugin)
        {
            if(plugin.OwningContext != null)
            {
                throw new KeyNotFoundException("Plugin already has an associated context.");
            }
            plugin.OwningContext = this;
            _loadedPlugins.Add(plugin);
        }

        public void RemovePluginsByAssembly(string assembly)
        {
            for (int i = 0; i < _loadedPlugins.Count; i++)
            {
                if (_loadedPlugins[i].AssemblyPath == assembly)
                {
                    RemovePlugin(_loadedPlugins[i]);
                    i--;
                }
            }
        }

        public void RemovePlugin(string pluginId)
        {
            var plugin = CheckPluginId(pluginId);
            RemovePlugin(plugin);
        }

        public void RemovePlugin(PluginMetadata plugin)
        {
            EndPointRunner.StopPluginTasks(plugin.Id);
            EndPointTable.RemovePlugin(plugin.Id);
            _loadedPlugins.Remove(plugin);
            plugin.OwningContext = null;
        }

        public void BeginPluginExecution<TEntryPointContext>(TEntryPointContext context)
            where TEntryPointContext : Extending.EntryPointContext
        {
            foreach (var plugin in _loadedPlugins)
            {
                BeginPluginExecution(plugin, context);
            }
        }

        public void BeginPluginExecution<TEntryPointContext>(Func<TEntryPointContext> contextFactory)
            where TEntryPointContext : Extending.EntryPointContext
        {
            foreach (var plugin in _loadedPlugins)
            {
                BeginPluginExecution(plugin, contextFactory());
            }
        }

        public void BeginPluginExecution<TEntryPointContext>(string pluginId, TEntryPointContext context)
            where TEntryPointContext : Extending.EntryPointContext
        {
            var plugin = CheckPluginId(pluginId);
            BeginPluginExecution(plugin, context);
        }

        public void BeginPluginExecution<TEntryPointContext>(PluginMetadata plugin, TEntryPointContext context)
            where TEntryPointContext : Extending.EntryPointContext
        {
            CheckPluginOwner(plugin);
            object? createdInstance = null;
            if(plugin.EntryType.GetConstructors().Any(c => !c.IsStatic))
            {
                Activator.CreateInstance(plugin.EntryType);
            }

            if (createdInstance == null && !plugin.EntryPoint.IsStatic)
            {
                throw new EntryPointNotFoundException();
            }

            if (context.Exposed != null && context.Exposed != Exposed)
            {
                throw new InvalidOperationException("The same entry point exposure point cannot be shared across plugin contexts.");
            }
            context.Exposed = Exposed;
            context.PluginObject = createdInstance;
            context.PluginType = plugin.EntryType;
            context.PluginId = plugin.Id;

            var invocationResult = Util.TryInvokeMatchingMethod(plugin.EntryPoint, createdInstance, context);
            if (invocationResult == MethodInvocationResults.Failed)
            {
                // TODO
            }
            else if (invocationResult == MethodInvocationResults.ExpectedStaticMethod)
            {
                // TODO
            }
        }

        public async Task ExecuteEndPoint<TEndPointContext>(TEndPointContext? context)
        {
            await EndPointRunner<TEndPointContext>.ForPluginContext(this).InvokeEndPointAsync(context);
        }

        public async Task ExecuteEndPoint<TEndPointContext>(Func<TEndPointContext?> contextCreator)
        {
            await EndPointRunner<TEndPointContext>.ForPluginContext(this).InvokeEndPointAsync(contextCreator);
        }


        private PluginMetadata CheckPluginId(string id)
        {
            var plugin = _loadedPlugins.FirstOrDefault(p => p.Id == id);
            if (plugin == null)
            {
                throw new KeyNotFoundException("The specified plugin could not be found or is not associated with this context.");
            }
            return plugin;
        }

        private void CheckPluginOwner(PluginMetadata plugin)
        {
            if (plugin.OwningContext != this)
            {
                throw new KeyNotFoundException("The specified plugin is associated with this context.");
            }
        }
    }
}
