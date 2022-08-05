namespace Wallop.Shared.ECS.ActorQuerying.FilterMachine
{
    public static class Extensions
    {
        public static Machine AddObjectMembers(this Machine machine, object obj, bool expand, string rootName)
        {
            machine.AddObjectMethods(obj, expand, rootName);
            machine.AddObjectVariables(obj, expand, rootName);
            return machine;
        }


        public static Machine RemoveObjectMembers(this Machine machine, string rootName)
        {
            for (int i = 0; i < machine.Members.Count; i++)
            {
                var member = machine.Members[i];
                if (member is MemberBase memBase)
                {
                    if (memBase.Qualifier == rootName)
                    {
                        machine.Members.RemoveAt(i);
                        i--;
                    }
                }
            }
            return machine;
        }

        public static Machine AddObjectMethods(this Machine machine, object obj, bool expand, string rootName)
        {
            var type = obj.GetType();
            foreach (var func in type.GetMethods())
            {
                // Ensure parameters are valid
                bool paramsValid = true;
                foreach (var param in func.GetParameters())
                {
                    if (param.ParameterType != typeof(string) &&
                        param.ParameterType != typeof(int) &&
                        param.ParameterType != typeof(double) &&
                        param.ParameterType != typeof(bool))
                    {
                        paramsValid = false;
                        break;
                    }
                }
                if (!paramsValid)
                {
                    continue;
                }
                if (func.ReturnType != typeof(void) &&
                    func.ReturnType != typeof(string) &&
                    func.ReturnType != typeof(int) &&
                    func.ReturnType != typeof(double) &&
                    func.ReturnType != typeof(bool))
                {
                    continue;
                }
                var member = new MethodInfoMember(func.Name, rootName, func, obj);
                member.RequireQualifier = !expand;
                machine.Members.Add(member);
            }

            return machine;
        }

        public static Machine AddObjectVariables(this Machine machine, object obj, bool expand, string rootName)
        {
            var type = obj.GetType();
            foreach (var prop in type.GetProperties())
            {
                if (prop.PropertyType != typeof(void) &&
                    prop.PropertyType != typeof(string) &&
                    prop.PropertyType != typeof(int) &&
                    prop.PropertyType != typeof(double) &&
                    prop.PropertyType != typeof(bool))
                {
                    continue;
                }

                if (expand)
                {
                    machine.Members.Add(new PropertyInfoMember(prop.Name, "", prop, obj));
                }
                if (!expand || !string.IsNullOrEmpty(rootName))
                {
                    machine.Members.Add(new PropertyInfoMember(prop.Name, rootName, prop, obj));
                }
            }

            return machine;
        }
    }
}