using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    public class Option
    {
        public string Name { get; set; }
        public char ShortName { get; set; }
        public bool IsRequired { get; set;  }
        public string DefaultValue { get; set; }
        public bool IsFlag { get; set; }
        public string ExclusionGroup { get; set; }
        public string HelpText { get; set; }

        public Option()
        {
            Name = "";
            ShortName = '\0';
            IsRequired = false;
            DefaultValue = "";
            IsFlag = false;
            ExclusionGroup = "";
            HelpText = "";
        }

        public Option Set(string name, char? shortName, bool? required, string defaultValue, bool? flag, string group, string helpText)
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
            if(group != null)
            {
                SetExclusionGroup(group);
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

        public Option SetExclusionGroup(string group)
        {
            ExclusionGroup = group;
            return this;
        }

        public Option SetHelpText(string text)
        {
            HelpText = text;
            return this;
        }
    }
}
