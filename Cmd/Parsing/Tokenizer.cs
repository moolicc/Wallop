using System;
using System.Collections.Generic;
using System.Text;
using Wallop.Cmd.Parsing.Tokens;

namespace Wallop.Cmd.Parsing
{
    public class Tokenizer
    {
        public bool AllowMultipleCommands { get; private set; }
        public bool AllowPipingCommands { get; private set; }

        public List<string> Selectors { get; private set; }

        private int _index;

        public Tokenizer()
        {
            Selectors = new List<string>();
        }

        public IEnumerable<Token> GetTokens(string source)
        {
            source = source.Trim();
            _index = 0;
            bool eocHit = false;
            foreach (var tok in ReadCommandTokens(source))
            {
                if(eocHit && !AllowMultipleCommands)
                {
                    //TODO: Error
                }
                if(tok is EOCToken)
                {
                    eocHit = true;
                }
                yield return tok;
            }
        }

        private IEnumerable<Token> ReadCommandTokens(string input)
        {
            void InitToken(Token token, string source, int position)
            {
                token.SetPosition(position);
                token.SetSource(source);
            }

            Token lastToken = null;
            int consumed = 0;
            var commandName = ReadWord(input, 0, out consumed);

            if (consumed == 0)
            {
                //TODO: Error
            }
            _index += consumed;
            //TODO: This is a bad way to perform this sanity-check.
            //What if a command actually has no arguments?
            if (input[_index] != ' ')
            {
                //TODO: Error
            }
            lastToken = new CommandToken(commandName);
            InitToken(lastToken, commandName, _index);
            yield return lastToken;

            SkipSpace(input, _index, out consumed);
            _index += consumed;

            for (int i = _index; i < input.Length; i++)
            {
                SkipSpace(input, i, out consumed);
                i += consumed;
                _index = i;
                if(input[i] == ';')
                {
                    // We're at the end of the current command.
                    _index++;
                    break;
                }
                if(input[i] == '-')
                {
                    // Skip leading hyphens.
                    // There'll be one single hyphen for short names, and two hyphens for long names.
                    if (PeekChar(input, i) == '-')
                    {
                        i++;
                    }
                    i++;
                    string argName = ReadWord(input, i, out consumed);
                    i += consumed;

                    lastToken = new ArgNameToken(argName);
                    InitToken(lastToken, argName, i);

                    yield return lastToken;
                    continue;
                }
                if(char.IsWhiteSpace(input[i]))
                {
                    continue;
                }

                bool startsWithQuote = input[i] == '"';

                string word = ReadWord(input, i, out consumed);
                i += consumed;

                if (!startsWithQuote && Selectors.Contains(word))
                {
                    lastToken = new SelectorToken(word);
                    InitToken(lastToken, word, i);
                    yield return lastToken;
                    continue;
                }

                //TODO: Tuples and arrays.
                if(!startsWithQuote && long.TryParse(word, out long resultI))
                {
                    lastToken = new ArgValueToken<long>(word, typeof(long), resultI);
                }
                else if(!startsWithQuote && double.TryParse(word, out double resultN))
                {
                    lastToken = new ArgValueToken<double>(word, typeof(double), resultN);
                }
                else if(!startsWithQuote && bool.TryParse(word, out bool resultB))
                {
                    lastToken = new ArgValueToken<bool>(word, typeof(bool), resultB);
                }
                else
                {
                    lastToken = new ArgValueToken<string>(word, typeof(string), word);
                }
                InitToken(lastToken, word, i);
                yield return lastToken;
            }


            yield return new EOCToken(_index);
        }

        private string ReadWord(string source, int start, out int count)
        {
            bool inQuote = false;
            bool expectEnd = false;
            string result = "";
            count = 0;
            for (int i = start; i < source.Length; i++)
            {
                var curChar = source[i];
                count++;
                if(curChar == '"')
                {
                    inQuote = !inQuote;
                    if (!inQuote)
                    {
                        expectEnd = true;
                    }
                    continue;
                }
                if((curChar == ' ' || curChar == ';') && !inQuote)
                {
                    count--;
                    break;
                }
                else if(expectEnd)
                {
                    //TODO: Error
                }
                if(curChar == '\\')
                {
                    if (source.Length > i + 1 && source[i + 1] == '"')
                    {
                        result += '"';
                        count++;
                        i++;
                        continue;
                    }
                }
                result += curChar;
            }
            return result;
        }

        private bool Expect(string source, int index, char character)
            => source[index] == character;

        private void SkipSpace(string source, int start, out int count)
        {
            count = 0;
            for (int i = start; i < source.Length; i++)
            {
                if (char.IsWhiteSpace(source[i]))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
        }

        private char PeekChar(string source, int start)
        {
            if(source.Length > start + 1)
            {
                return source[start + 1];
            }
            return '\0';
        }
    }
}
