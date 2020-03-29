using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    public class Option
    {
        // TODO: When we get to the UI part, we might consider allowing this to provide a target Type
        // as well as an array of valid values.
        public string Name { get; set; }
        public char ShortName { get; set; }
        public bool IsRequired { get; set;  }
        public string DefaultValue { get; set; }
        public bool IsFlag { get; set; }
        public bool IsSelector { get; set; }
        public string GroupSelection { get; set; }
        public string HelpText { get; set; }
        public string[] ValidValues { get; set; }
        public string[] GroupsForValues { get; set; }

        public Option()
        {
            Name = "";
            ShortName = '\0';
            IsRequired = false;
            DefaultValue = "";
            IsFlag = false;
            IsSelector = false;
            GroupSelection = "";
            HelpText = "";
            ValidValues = new string[0];
            GroupsForValues = new string[0];
        }

        public static Option Create(string name = "", char? shortName = null, bool? required = null, string defaultValue = null, bool? flag = null, bool? groupSelector = null, string groupSelection = null, string helpText = null)
        {
            Option option = new Option();
            return option.Set(name, shortName, required, defaultValue, groupSelector, flag, groupSelection, helpText);
        }

        public Option Set(string name = "", char? shortName = null, bool? required = null, string defaultValue = null, bool? flag = null, bool? groupSelector = null, string groupSelection = null, string helpText = null)
        {
            if(name != null)
            {
                SetName(name);
            }
            if(shortName.HasValue)
            {
                SetShortName(shortName.Value);
            }
            if(required.HasValue)
            {
                SetRequired(required.Value);
            }
            if(defaultValue != null)
            {
                SetDefaultValue(defaultValue);
            }
            if(flag.HasValue)
            {
                SetIsFlag(flag.Value);
            }
            if(groupSelector.HasValue)
            {
                SetIsGroupSelector(groupSelector.Value);
            }
            if(groupSelection != null)
            {
                SetGroupSelection(groupSelection);
            }
            if(helpText != null)
            {
                SetHelpText(helpText);
            }
            return this;
        }

        public Option SetName(string name)
        {
            Name = name;
            return this;
        }

        public Option SetShortName(char shortName)
        {
            ShortName = shortName;
            return this;
        }

        public Option SetRequired(bool isRequired)
        {
            IsRequired = isRequired;
            return this;
        }

        public Option SetDefaultValue(string value)
        {
            DefaultValue = value;
            return this;
        }

        public Option SetIsFlag(bool isFlag)
        {
            IsFlag = isFlag;
            return this;
        }

        public Option SetIsGroupSelector(bool isSelector)
        {
            IsSelector = isSelector;
            return this;
        }

        public Option SetGroupSelection(string group)
        {
            GroupSelection = group;
            return this;
        }

        public Option SetHelpText(string text)
        {
            HelpText = text;
            return this;
        }
    }
}
