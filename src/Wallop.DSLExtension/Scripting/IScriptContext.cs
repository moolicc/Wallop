using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Scripting
{
    public record Import(string Assembly, string Namespace);
    public record ExposedValue(string MemberName, object? Value);
    public record ExposedDelegate(string MemberName, Delegate Action);

    public interface IScriptContext
    {
        /// <summary>
        /// Formats the specified member name as a variable and returns the results.
        /// </summary>
        /// <param name="name">The member name to format.</param>
        /// <remarks>
        /// It is the responsibility of the Implementor to make use of this function when adding and retrieving values.
        /// </remarks>
        string FormatValueName(string name);

        /// <summary>
        /// Formats the specified member name as a getter function and returns the results.
        /// </summary>
        /// <param name="name">The member name to format.</param>
        /// <remarks>
        /// It is the responsibility of the Implementor to make use of this function when adding and retrieving getter functions.
        /// </remarks>
        string FormatGetterName(string name);


        /// <summary>
        /// Formats the specified member name as a setter function and returns the results.
        /// </summary>
        /// <param name="name">The member name to format.</param>
        /// <remarks>
        /// It is the responsibility of the Implementor to make use of this function when adding and retrieving setter functions.
        /// </remarks>
        string FormatSetterName(string name);

        /// <summary>
        /// Formats the specified member name as a function and returns the results.
        /// </summary>
        /// <param name="name">The member name to format.</param>
        /// <remarks>
        /// It is the responsibility of the Implementor to make use of this function when adding and retrieving functions.
        /// </remarks>
        string FormatFunctionName(string name);


        bool ContainsValue(string name);
        void SetValue(string name, object? value);
        void SetValue(ExposedValue valueInfo);

        object? GetValue(string name);
        T? GetValue<T>(string name);

        IEnumerable<KeyValuePair<string, object?>> GetValues();
        IEnumerable<KeyValuePair<string, object?>> GetAddedValues();

        /// <summary>
        /// Returns the values that should be tracked (saved on StateSave).
        /// </summary>
        IEnumerable<string> GetTrackedMembers();

        /// <summary>
        /// Specifies whether or not the specified member should be tracked.
        /// All members should default to not being tracked.
        /// </summary>
        void SetTrackedMember(string name, bool track = true);

        T GetDelegateAs<T>(string memberName);

        void AddReference(Assembly assembly);
        //public void AddReference(string assembly);

        void AddImport(string ns);
        void AddImport(Import import);

        bool ContainsDelegate(string name);
        void SetDelegate(string name, Delegate method);
        void SetDelegate(ExposedDelegate exposedDelegate);
    }
}
