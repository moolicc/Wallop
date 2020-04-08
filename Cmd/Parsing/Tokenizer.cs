using System;
using System.Collections.Generic;
using System.Text;
using Wallop.Cmd.Parsing.Tokens;
using System.Linq;

namespace Wallop.Cmd.Parsing
{
    public class Tokenizer
    {
        public bool AllowMultipleCommands { get; set; }
        public bool AllowPipingCommands { get; set; }

        private int _index;

        public Tokenizer()
        {
            AllowMultipleCommands = true;
            AllowPipingCommands = false;
            _index = -1;
        }

        public IEnumerable<Token> GetTokens(string source, params string[] selectors)
        {
            source = source.Trim();
            _index = 0;
            bool eocHit = false;
            foreach (var tok in ReadCommandTokens(source, selectors))
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

        private IEnumerable<Token> ReadCommandTokens(string input, params string[] selectors)
        {
            void InitToken(Token token, string source, int position)
            {
                token.SetPosition(position);
                token.SetSource(source);
            }
            //TODO: Handle tuples and arrays.

            bool isCommandName = true;
            Token lastToken = null;
            int consumed = 0;

            for (int i = _index; i < input.Length; i++)
            {
                SkipSpace(input, i, out consumed);
                i += consumed;
                _index = i;

                if(isCommandName)
                {
                    var commandName = ReadWord(input, 0, out consumed);

                    if (consumed == 0)
                    {
                        //TODO: Error
                    }
                    isCommandName = false;

                    i += consumed;
                    lastToken = new CommandToken(commandName);
                    InitToken(lastToken, commandName, _index);

                    yield return lastToken;
                    continue;
                }

                if(input[i] == ';')
                {
                    // We're at the end of the current command.
                    isCommandName = true;
                    yield return new EOCToken(_index);
                    continue;
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

                if (!startsWithQuote && selectors.Contains(word))
                {
                    lastToken = new SelectorToken(word);
                    InitToken(lastToken, word, i);
                    yield return lastToken;
                    continue;
                }

                lastToken = new ArgValueToken(word);
                InitToken(lastToken, word, i);

                yield return lastToken;

                if(i < input.Length && input[i] == ';')
                {
                    i--;
                }
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
