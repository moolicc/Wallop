using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wallop.Cmd
{
    public class Parser
    {
        //TODO: Refactorings.
        // Option -> Argument.
        // Different kind of arguments need to be inherited.
        // Write tests


        public CommandSet CommandSet { get; private set; }

        public Parser(CommandSet commandSet)
        {
            CommandSet = commandSet;
        }

        public ParseResults Parse(string source)
        {
            int IndexOr(IEnumerable<string> input, Predicate<string> predicate)
            {
                for (int i = 0; i < input.Count(); i++)
                {
                    if (predicate(input.ElementAt(i)))
                    {
                        return i;
                    }
                }
                return -1;
            }


            var result = new ParseResults();
            var words = ParseWords(source);

            Command command = null;
            foreach (var item in CommandSet.Commands)
            {
                if(string.Equals(words.First(), item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    command = item;
                    break;
                }
            }

            var optionGroups = FindGroups(command.Options, words);
            var selectedGroup = "";
            bool skip = false;

            foreach (var option in command.Options)
            {
                if(skip)
                {
                    skip = false;
                    continue;
                }
                var wordIndex = IndexOr(words, s => string.Equals(option.Name, s, StringComparison.OrdinalIgnoreCase));
                var validGroup = IsValidGroup(option.ExclusionGroup, optionGroups);
                if(string.IsNullOrWhiteSpace(selectedGroup))
                {
                    if((!string.IsNullOrWhiteSpace(option.ExclusionGroup) || option.GroupsForValues.Length > 0) && wordIndex != -1)
                    {
                        //TODO: Appropriately handle option.Values and option.GroupsForValues.

                        //If next word is valid value
                        //group = option.groupsforvalues[index of value in option.validvalues]


                        selectedGroup = option.ExclusionGroup;
                    }
                }
                if (!validGroup && wordIndex != -1)
                {
                    //TODO: Return error result. Not in exclusion group.
                }
                else if (validGroup && wordIndex == -1 && option.IsRequired)
                {
                    //TODO: Return error result. Option is required but not provided.
                }
                else if (validGroup && wordIndex == -1)
                {
                    result.Options.Add(option.Name, option.DefaultValue);
                }
                else if (validGroup && wordIndex != -1)
                {
                    int nextIndex = wordIndex + 1;
                    if (option.IsFlag)
                    {
                        result.Options.Add(option.Name, bool.TrueString);
                    }
                    else if (nextIndex < command.Options.Count)
                    {
                        skip = true;
                        result.Options.Add(option.Name, words.ElementAt(nextIndex));
                    }
                }
                else if (!validGroup && wordIndex == -1)
                { }
                else
                {
                    //TODO: Return error result. Missing Value.
                }
            }

            command.InvocationTarget?.Invoke(result);
            return result;
        }

        private IEnumerable<string> FindGroups(IEnumerable<Option> input, IEnumerable<string> words)
        {
            foreach (var item in words)
            {
                var option = input.FirstOrDefault(o => string.Equals(o.Name, item, StringComparison.OrdinalIgnoreCase));
                var options = (option.ExclusionGroup + '|').Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var subitem in options)
                {
                    yield return subitem;
                }
            }
        }

        private bool IsValidGroup(string group, IEnumerable<string> groups)
        {
            if(string.IsNullOrWhiteSpace(group))
            {
                return true;
            }
            var targetGroups = (group + "|").Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in groups)
            {
                if(targetGroups.Any(g => string.Equals(g, item, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }

        private IEnumerable<string> ParseWords(string source)
        {
            var results = new ParseResults();
            var wordBuilder = new StringBuilder();

            int index = 0;
            void SkipWhiteSpace()
            {
                int i = 0;
                for (i = index; i < source.Length; i++)
                {
                    if (!char.IsWhiteSpace(source[i]))
                    {
                        break;
                    }
                }
                index = i;
            }

            string ParseWord()
            {
                wordBuilder.Clear();
                bool inQuote = false;
                int i = 0;
                for (i = index; i < source.Length; i++)
                {
                    var curChar = source[i];
                    if (char.IsWhiteSpace(curChar))
                    {
                        break;
                    }

                    if (curChar == '\\')
                    {
                        if (i + 1 < source.Length)
                        {
                            if (source[i + 1] == '"')
                            {
                                wordBuilder.Append('"');
                                i++;
                                continue;
                            }
                        }
                    }
                    else if (curChar == '"')
                    {
                        inQuote = !inQuote;
                        continue;
                    }

                    wordBuilder.Append(curChar);
                }

                index = i;
                return wordBuilder.ToString();
            }

            while (index < source.Length)
            {
                SkipWhiteSpace();
                yield return ParseWord();
            }
        }
    }
}
