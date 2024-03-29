﻿using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Scripting;

namespace Scripting.IronPython
{
    public class PythonScriptContext : IScriptContext
    {
        private ScriptScope _scope;
        private ScriptEngine _engine;

        private List<string> _trackedKeys;

        public PythonScriptContext()
        {
            _trackedKeys = new List<string>();
        }

        internal void SetScope(ScriptScope pyScope)
        {
            _scope = pyScope;
        }

        internal void SetEngine(ScriptEngine pyEngine)
        {
            _engine = pyEngine;
        }


        public void SetDelegate(ExposedDelegate exposedDelegate)
            => SetDelegate(exposedDelegate.MemberName, exposedDelegate.Action);

        public void SetDelegate(string name, Delegate method)
        {
            _scope.SetVariable(FormatFunctionName(name), method);
        }


        public void AddImport(Import import)
            => AddImport(import.Assembly, import.Namespace);

        public void AddImport(string assembly, string ns)
        {
            _engine.Execute($"from {assembly} import {ns}", _scope);
        }

        public void AddImport(string ns)
        {
            _engine.Execute($"from {ns} import *", _scope);
        }

        public void AddReference(Assembly assembly)
        {
            _engine.Runtime.LoadAssembly(assembly);
        }

        public void AddReference(string assembly)
        {
            _engine.Execute($"clr.AddReference(\"{assembly}\")", _scope);
        }


        public void SetValue(ExposedValue valueInfo)
            => SetValue(valueInfo.MemberName, valueInfo.Value);

        public void SetValue(string name, object? value)
        {
            _scope.SetVariable(FormatValueName(name), value);
        }


        public bool ContainsValue(string name)
            => _scope.ContainsVariable(FormatValueName(name));


        public T GetDelegateAs<T>(string memberName)
        {
            return _scope.GetVariable<T>(FormatFunctionName(memberName));
        }

        public object? GetValue(string name)
        {
            var value = _scope.GetVariable(FormatValueName(name));
            return value;
        }

        public T? GetValue<T>(string name)
            => _scope.GetVariable<T>(FormatValueName(name));

        public bool ContainsDelegate(string name)
            => _scope.ContainsVariable(FormatFunctionName(name));

        public IEnumerable<KeyValuePair<string, object?>> GetValues()
        {
            return _scope.GetVariableNames().Select(name => new KeyValuePair<string, object?>(name, _scope.GetVariable(name)));
        }

        public IEnumerable<KeyValuePair<string, object?>> GetAddedValues()
        {
            foreach (var name in _scope.GetVariableNames())
            {
                if(name == "__builtins__" || name == "__file__" || name == "__name__" || name == "__doc__")
                {
                    continue;
                }
                
                var value = _scope.GetVariable<object>(name);
                if(value != null && (value.GetType().FullName ?? "").Contains("IronPython"))
                {
                    continue;
                }
                yield return new KeyValuePair<string, object?>(name, value);
            }
        }

        public IEnumerable<string> GetTrackedMembers()
        {
            return _trackedKeys.AsReadOnly();
        }

        public void SetTrackedMember(string name, bool track = true)
        {
            if(track)
            {
                _trackedKeys.Add(name);
            }
            else
            {
                _trackedKeys.Remove(name);
            }
        }

        public string FormatValueName(string name)
            => FormatFunctionName(name);

        public string FormatGetterName(string name)
        {
            if(name.StartsWith("get", StringComparison.OrdinalIgnoreCase))
            {
                return FormatFunctionName(name);
            }
            return FormatFunctionName("get_" + name);
        }

        public string FormatSetterName(string name)
        {
            if (name.StartsWith("set", StringComparison.OrdinalIgnoreCase))
            {
                return FormatFunctionName(name);
            }
            return FormatFunctionName("set_" + name);
        }

        public string FormatFunctionName(string name)
        {
            var builder = new StringBuilder();
            bool lastWasUpper = false;

            for (int i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i]) && !lastWasUpper)
                {
                    lastWasUpper = true;
                    builder.Append("_");
                }
                else if (!char.IsUpper(name[i]))
                {
                    lastWasUpper = false;
                }

                builder.Append(char.ToLower(name[i]));
            }
            return builder.ToString();
        }
    }
}
