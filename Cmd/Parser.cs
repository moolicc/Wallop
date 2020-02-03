using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wallop.Cmd
{
    public class Parser
    {
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

            // NOTE: We might need to loop over the words/options in a first-pass to find this.
            // Then go over all the options later with this knowledge before-hand.
            var optionGroup = "";
            bool skip = false;

            foreach (var option in command.Options)
            {
                if(skip)
                {
                    skip = false;
                    continue;
                }
                var wordIndex = IndexOr(words, s => string.Equals(option.Name, s, StringComparison.OrdinalIgnoreCase));
                bool isInExclusion = string.Equals(optionGroup, option.ExclusionGroup, StringComparison.OrdinalIgnoreCase);

                if(string.IsNullOrWhiteSpace(option.ExclusionGroup))
                {
                    isInExclusion = true;
                }

                if (wordIndex == -1 && option.IsRequired && isInExclusion)
                {
                    //TODO: Return error result. Option is required but not provided.
                }
                else if(wordIndex == -1 && isInExclusion)
                {
                    result.Options.Add(option.Name, option.DefaultValue);
                    continue;
                }
                else if(wordIndex == -1 && !isInExclusion)
                {
                    continue;
                }

                if (!isInExclusion)
                {
                    //TODO: Return error result. Not in exclusion group.
                }

                if (!string.IsNullOrWhiteSpace(option.ExclusionGroup))
                {
                    optionGroup = option.ExclusionGroup;
                }

                int nextIndex = wordIndex + 1;
                if (option.IsFlag)
                {
                    result.Options.Add(option.Name, bool.TrueString);
                }
                else if(nextIndex < command.Options.Count())
                {
                    skip = true;
                    result.Options.Add(option.Name, words.ElementAt(nextIndex));
                }

                //TODO: Return error result. Missing Value.
            }

            command.InvocationTarget?.Invoke(result);
            return result;
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
