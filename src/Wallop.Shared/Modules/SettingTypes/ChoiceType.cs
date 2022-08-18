using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Modules.SettingTypes
{
    public readonly record struct Option(int Index, string Label, string Value);

    public class ChoiceType : ISettingType
    {
        // Choices are defined in the format:
        // <option>label:value</option>
        public const char LABEL_VALUE_DELIMITER = '|';

        public string Name => "choice";



        public string Serialize(object value, IEnumerable<KeyValuePair<string, string>>? args)
        {
            var choices = GetOptions(args);

            if (value is int i)
            {
                if (i > choices.Length - 1 || i < 0)
                {
                    throw new InvalidOperationException("Invalid choice index for option list.");
                }
                return i.ToString();
            }
            else if (value is string s)
            {
                Option? selected = null;
                foreach (var item in choices)
                {
                    if (s.Equals(item.Label, StringComparison.OrdinalIgnoreCase)
                        || s.Equals(item.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        selected = item;
                        break;
                    }
                }
                if (selected.HasValue)
                {
                    return selected.Value.Value;
                }
                throw new InvalidOperationException("Invalid choice value for option list.");
            }
            else if (value is Enum e)
            {
                // Find e.name:value match
                // or e.name:label match
                var enumNames = Enum.GetNames(e.GetType());
                Option? selected = null;

                foreach (var choice in choices)
                {
                    foreach (var item in enumNames)
                    {
                        if(choice.Value.Equals(item, StringComparison.OrdinalIgnoreCase)
                            || choice.Label.Equals(item, StringComparison.OrdinalIgnoreCase))
                        {
                            selected = choice;
                            break;
                        }
                    }

                    if(selected.HasValue)
                    {
                        break;
                    }
                }

                if(selected.HasValue)
                {
                    return selected.Value.Value;
                }

                // Find e.value:index match
                var enumValue = (int?)Convert.ChangeType(e, typeof(int));
                if(enumValue == null)
                {
                    throw new InvalidOperationException("Error determining choice value.");
                }

                if(enumValue.Value > choices.Length - 1 || enumValue.Value < 0)
                {
                    throw new InvalidOperationException("Invalid enum backing value for choice.");
                }
                return enumValue.Value.ToString();
            }

            throw new InvalidOperationException("Failed to serialize choice.");
        }

        public bool TrySerialize(object value, [NotNullWhen(true)] out string? result, IEnumerable<KeyValuePair<string, string>>? args)
        {
            result = null;
            if (value == null || (value is not int && value is not string && value is not Enum))
            {
                return false;
            }

            try
            {
                result = Serialize(value, args);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool TryDeserialize(string value, [NotNullWhen(true)] out object? result, IEnumerable<KeyValuePair<string, string>>? args)
        {
            var choices = GetOptions(args);

            if(int.TryParse(value, out int index))
            {
                result = choices[index];
                return true;
            }

            foreach (var choice in choices)
            {
                if(choice.Value.Equals(value, StringComparison.OrdinalIgnoreCase)
                    || choice.Label.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    result = choice;
                    return true;
                }
            }

            result = null;
            return false;
        }


        public static Option[] GetOptions(IEnumerable<KeyValuePair<string, string>>? args)
        {
            if(args == null)
            {
                return Array.Empty<Option>();
            }

            var choices = new List<Option>();
            int optionIndex = 0;
            foreach (var arg in args)
            {
                if(arg.Key == "option")
                {
                    var label = arg.Value;
                    var value = arg.Value;

                    if (arg.Value.Contains(LABEL_VALUE_DELIMITER))
                    {
                        var split = arg.Value.Split(LABEL_VALUE_DELIMITER, 2);
                        label = split[0];
                        value = split[1];
                    }

                    choices.Add(new Option(optionIndex, label, value));
                    optionIndex++;
                }
            }
            return choices.ToArray();
        }
    }
}
