using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PluginPantry
{
    internal enum MethodInvocationResults
    {
        Succeeded,
        ExpectedStaticMethod,
        Failed
    }

    internal static class Util
    {
        public static MethodInvocationResults TryInvokeMatchingMethod<TModel>(MethodInfo targetMethod, object? targetInstance, TModel? targetModel)
        {
            var modelType = typeof(TModel);
            var passedArgs = new List<object?>();
            bool signatureFound = true;

            if(targetInstance == null && !targetMethod.IsStatic)
            {
                return MethodInvocationResults.ExpectedStaticMethod;
            }

            foreach (var param in targetMethod.GetParameters())
            {
                object? value = null;
                if(modelType.GetProperty(param.Name ?? "THIS_SHOULD_NOT_BE_FOUND") is PropertyInfo property)
                {
                    value = property.GetValue(targetModel);
                }
                else
                {
                    bool found = false;
                    foreach (var modelProperty in modelType.GetProperties())
                    {
                        if(modelProperty.PropertyType == param.ParameterType)
                        {
                            value = modelProperty.GetValue(targetModel);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        if (param.IsOptional)
                        {
                            value = param.DefaultValue;
                        }
                        else
                        {
                            signatureFound = false;
                            break;
                        }
                    }
                }
                passedArgs.Add(value);
            }

            if (!signatureFound)
            {
                return MethodInvocationResults.Failed;
            }

            targetMethod.Invoke(targetInstance, passedArgs.ToArray());
            return MethodInvocationResults.Succeeded;
        }
    }
}
