using System;
using System.Collections;
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
                root = IDHelper.GetModuleById(parsed.RootId);
            }
            else
            {
                root = IDHelper.GetPackageById(parsed.RootId);
            }
            if(root == null)
            {
                return false;
            }

            var targetMember = "";
            var target = Traverse(root, parsed.Path, ref targetMember);


            if (target == null)
            {
                return false;
            }

            var singlePath = string.Join(":", parsed.Path);
            if(!string.Equals(singlePath, targetMember, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return SetValue(root, parsed.Path);
        }

        private object? Traverse(object? root, string[] path, ref string memberName, bool matchSelector = false)
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

            string additionalMemberName = "";
            if (matchSelector)
            {
                if (Equals(root, path[0]) || (root is KeyValuePair<string, string> kvp && kvp.Key == path[0]))
                {
                    memberName = path[0];
                    return root;
                }

                foreach (var prop in rootType.GetProperties())
                {
                    if (string.Equals(prop.GetValue(root)?.ToString(), path[0], StringComparison.OrdinalIgnoreCase))
                    {
                        var result = Traverse(root, splicedPath, ref additionalMemberName);
                        memberName = path[0];
                        if(!string.IsNullOrEmpty(additionalMemberName))
                        {
                            memberName += $":{additionalMemberName}";
                        }
                        return result;
                    }
                }

                foreach (var field in rootType.GetFields())
                {
                    if (string.Equals(field.GetValue(root)?.ToString(), path[0], StringComparison.OrdinalIgnoreCase))
                    {
                        var result = Traverse(root, splicedPath, ref additionalMemberName);
                        memberName = path[0];
                        if (!string.IsNullOrEmpty(additionalMemberName))
                        {
                            memberName += $":{additionalMemberName}";
                        }
                        return result;
                    }
                }

                return null;
            }




            if (root is IEnumerable<object> enumerable)
            {
                foreach (var item in enumerable)
                {
                    var result = Traverse(item, path, ref memberName, true);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }




            foreach (var prop in rootType.GetProperties())
            {
                if (prop.Name == path[0])
                {
                    object? result = null;
                    var propValue = prop.GetValue(root);
                    if (propValue is IEnumerable propEnumerable)
                    {
                        foreach (var item in propEnumerable)
                        {
                            result = Traverse(item, splicedPath, ref additionalMemberName, true);
                            if (result != null)
                            {
                                break;
                            }
                        }

                        if(result == null)
                        {
                            result = propValue;
                            memberName = $"{path[0]}:{path[1]}";
                            return result;
                        }
                    }

                    if (propValue is IEnumerable<object> propEnumerable2)
                    {
                        foreach (var item in propEnumerable2)
                        {
                            result = Traverse(item, splicedPath, ref additionalMemberName, true);
                            if (result != null)
                            {
                                break;
                            }
                        }

                        if (result == null)
                        {
                            result = propValue;
                            memberName = $"{path[0]}:{path[1]}";
                            return result;
                        }
                    }



                    if (result == null)
                    {
                        result = Traverse(propValue, splicedPath, ref additionalMemberName);
                    }

                    memberName = prop.Name;
                    if(!string.IsNullOrEmpty(additionalMemberName))
                    {
                        memberName += $":{additionalMemberName}";
                    }
                    return result;
                }
            }

            foreach (var field in rootType.GetFields())
            {
                if (field.Name == path[0])
                {
                    object? result = null;
                    var fieldValue = field.GetValue(root);
                    if (fieldValue is IEnumerable<object> propEnumerable)
                    {
                        foreach (var item in propEnumerable)
                        {
                            result = Traverse(item, path, ref memberName, true);
                            if (result != null)
                            {
                                break;
                            }
                        }
                    }

                    if (result == null)
                    {
                        result = Traverse(fieldValue, splicedPath, ref additionalMemberName);
                    }

                    memberName = field.Name;
                    if (!string.IsNullOrEmpty(additionalMemberName))
                    {
                        memberName += $":{additionalMemberName}";
                    }
                    return result;
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

        private bool SetValue(object target, string[] targetPath)
        {
            var mappings = new List<MutationMapping>();
            var allWildCardPositions = new List<(int Mapping, int Position)>();

            var allMappings = MutationExtensions.GetMappings();
            for(int i = 0; i < allMappings.Length; i++)
            {
                var mapping = allMappings[i];
                var mapKey = mapping.Key.Split(':');
                if(mapKey.Length != targetPath.Length)
                {
                    continue;
                }

                bool valid = true;
                var wildcardPos = new List<int>();
                for (int j = 0; j < mapKey.Length; j++)
                {
                    if (!string.Equals(mapKey[j], targetPath[j], StringComparison.OrdinalIgnoreCase) && mapKey[j] != "$")
                    {
                        valid = false;
                        break;
                    }
                    if (mapKey[j] == "$")
                    {
                        wildcardPos.Add(j);
                    }
                }
                if(valid)
                {
                    allWildCardPositions.AddRange(wildcardPos.Select(p => (mappings.Count, p)));
                    mappings.Add(mapping);
                }
            }

            var chosenMapping = mappings[0];
            int selected = 0;
            for (int i = 0; i < mappings.Count; i++)
            {
                MutationMapping item = mappings[i];
                if ((item.Method.Name.Contains("Add") || item.Method.Name.Contains("Set")) && ChangeType.HasFlag(ChangeTypes.Create))
                {
                    chosenMapping = item;
                    selected = i;
                }
                else if ((item.Method.Name.Contains("Add") || item.Method.Name.Contains("Set")) && ChangeType.HasFlag(ChangeTypes.Update))
                {
                    chosenMapping = item;
                    selected = i;
                }
                else if((item.Method.Name.Contains("Remove") || item.Method.Name.Contains("Delete")) && ChangeType.HasFlag(ChangeTypes.Delete))
                {
                    chosenMapping = item;
                    selected = i;
                }
            }
            var wildCardPositions = new List<int>(allWildCardPositions.Where(i => i.Mapping == selected).Select(i => i.Position));


            var args = new List<object>();
            args.Add(target);

            if(chosenMapping.Method.GetParameters().Length > 2 && wildCardPositions.Count > 0)
            {
                args.AddRange(wildCardPositions.Select(i => targetPath[i]));
            }

            if (NewValue is string s && s.Contains(":") && chosenMapping.Method.GetParameters().Length > args.Count() + 1)
            {
                var splitValues = s.Split(':');
                if(chosenMapping.Method.GetParameters().Length == 2 + splitValues.Length)
                {
                    args.AddRange(splitValues);
                }
            }
            else
            {
                args.Add(NewValue);
            }
            chosenMapping.Target.Method.Invoke(null, args.ToArray());

            return true;
        }

        private (bool IsModule, int RootId, string RootName, string[] Path) ParseChangeTarget()
        {
            //Prefix format: (M <number>) field:field:field:
            //Prefix format: (P <number>) field:field:field:

            var target = TargetField;
            var isModule = target.StartsWith("(M");

            // Remove the beginning "(M "
            target = target.Remove(0, 3);


            var spaceIndex = target.IndexOf(' ');

            // Remove the prefix's trailing paren.
            target = target.Remove(spaceIndex - 1, 1);
            spaceIndex = target.IndexOf(' ');

            // Parse the ID.
            var id = int.Parse(target.Substring(0, spaceIndex));

            // Remove the remaining of the prefix.
            target = target.Remove(0, spaceIndex + 1);

            var parts = target.Split(":", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var path = new string[parts.Length - 1];

            Array.Copy(parts, 1, path, 0, path.Length);

            return (isModule, id, parts[0], path);
        }
    }
}
