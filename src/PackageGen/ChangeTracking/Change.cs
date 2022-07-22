using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Modules;
using Module = Wallop.DSLExtension.Modules.Module;

namespace PackageGen.ChangeTracking
{
    public class Change
    {
        public ChangeTypes ChangeType { get; init; }

        public string TargetField { get; init; }

        public object CurrentValue { get; init; }

        public object NewValue { get; init; }

        public bool CanRevert { get; init; }

        public Func<Change, bool>? ApplyCallback;


        public Change(ChangeTypes changeType, string targetField, object currentValue, object newValue, bool canRevert = true, Func<Change, bool> applyCallback = null)
        {
            ChangeType = changeType;
            TargetField = targetField;
            CurrentValue = currentValue;
            NewValue = newValue;
            CanRevert = canRevert;
            ApplyCallback = applyCallback;
        }

        public Change GetReversion()
        {
            if (!CanRevert)
            {
                throw new InvalidOperationException("Cannot revert this change.");
            }

            return new Change(ChangeType | ChangeTypes.Revert, TargetField, NewValue, CurrentValue);
        }

        public bool TryApply(List<Package> packages, List<Module> modules)
        {
            if (ApplyCallback != null)
            {
                return ApplyCallback(this);
            }
            var parsed = ParseChangeTarget();

            object? root;
            if(parsed.IsModule)
            {
                root = modules.FirstOrDefault(m => m.ModuleInfo.ScriptName == parsed.RootName);
            }
            else
            {
                root = packages.FirstOrDefault(p => p.Info.PackageName== parsed.RootName);
            }
            if(root == null)
            {
                return false;
            }

            var targetMember = "";
            var target = Traverse(root, parsed.Path, ref targetMember);

            if(target == null)
            {
                return false;
            }

            return SetValue(root, targetMember);
        }

        private object? Traverse(object? root, string[] path, ref string memberName, bool first = false)
        {
            // TODO: This is going to have to be more clever to handle collections and wildcards ($) in paths.

            if (root == null)
            {
                return null;
            }

            if(path.Length == 0)
            {
                return root;
            }

            var rootType = root.GetType();
            var splicedPath = path[1..];
            

            foreach (var pathItem in path)
            {
                foreach (var prop in rootType.GetProperties())
                {
                    if(prop.Name == pathItem)
                    {
                        memberName = prop.Name;
                        return Traverse(prop.GetValue(root), splicedPath, ref memberName);
                    }
                }

                foreach (var field in rootType.GetFields())
                {
                    if (field.Name == pathItem)
                    {
                        memberName = field.Name;
                        return Traverse(field.GetValue(root), splicedPath[1..], ref memberName);
                    }
                }
            }

            foreach (var prop in rootType.GetProperties())
            {
                var res = Traverse(prop.GetValue(root), path, ref memberName);
                if (res != null)
                {
                    return res;
                }
            }

            foreach (var field in rootType.GetFields())
            {
                var res = Traverse(field.GetValue(root), path, ref memberName);
                if (res != null)
                {
                    return res;
                }
            }

            return root;
        }

        private bool SetValue(object target, string targetMember)
        {
            var mappings = MutationExtensions.GetMappings().Where(m => m.Key == targetMember).ToArray();

            var chosenMapping = mappings[0];
            foreach (var item in mappings)
            {
                if(item.Method.Name.Contains("Add") && ChangeType.HasFlag(ChangeTypes.Create))
                {
                    chosenMapping = item;
                }
                else if(item.Method.Name.Contains("Remove") && ChangeType.HasFlag(ChangeTypes.Delete))
                {
                    chosenMapping = item;
                }
            }

            chosenMapping.Target.DynamicInvoke(target, NewValue);

            return true;
        }

        private (bool IsModule, string RootName, string[] Path) ParseChangeTarget()
        {
            var target = TargetField;
            var isModule = target.StartsWith("(M)");

            target = target.Remove(0, 4);
            var parts = target.Split(":", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var path = new string[parts.Length - 1];

            Array.Copy(parts, 1, path, 0, path.Length);

            return (isModule, parts[0], path);
        }
    }
}
