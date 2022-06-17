using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens.Default;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens
{
    // TODO: Add ~= operator for comparing ints to floats.
    public class Tokenizer
    {
        public int Index { get; private set; }

        public Tokenizer()
        {
            Index = 0;
        }

        public IEnumerable<IToken> GetStream(string input)
        {
            IToken curToken = null;
            IToken? lastToken = null;
            for (int i = 0; i < input.Length; i++)
            {
                char cur = input[i];
                if(char.IsWhiteSpace(input[i]))
                {
                    continue;
                }
                Index = i;

                // Single character tokens
                if (cur == '$')
                {
                    curToken = new EditToken(i, "$");
                }
                else if(cur == '%')
                {
                    curToken = new FilterToken(i, "%");
                }
                else if (cur == '=')
                {
                    curToken = new ComparisonToken(i, "=", ComparisonModes.Equal);
                }
                else if (cur == '+')
                {
                    curToken = new AdditionToken(i);
                }
                else if (cur == '/')
                {
                    curToken = new DivideToken(i);
                }
                else if (cur == '^')
                {
                    curToken = new PowToken(i);
                }
                else if (cur == '(')
                {
                    curToken = new LParenToken(i);
                }
                else if (cur == ')')
                {
                    curToken = new RParenToken(i);
                }
                else if (cur == '.')
                {
                    curToken = new DotToken(i, ".");
                }
                else if (cur == ',')
                {
                    curToken = new CommaToken(i);
                }
                else if (cur == ':')
                {
                    curToken = new DotToken(i, ":");
                }
                else if (cur == '"')
                {
                    Index++;
                    curToken = new StringToken(i, AdvancePast(input, '"'));
                    i = Index;
                }
                else if (cur == '\'')
                {
                    Index++;
                    curToken = new StringToken(i, AdvancePast(input, '\''));
                    i = Index;
                }
                // Single/Double character tokens.
                else if (cur == '|')
                {
                    if(PeekNext(input) == '|')
                    {
                        curToken = new OrToken(i, "||");
                        Index++;
                        i++;
                    }
                    else
                    {
                        curToken = new PipeToken(i, "|");
                    }
                }
                else if (cur == '&')
                {
                    if(PeekNext(input) == '&')
                    {
                        curToken = new AndToken(i, "&&");
                        Index++;
                        i++;
                    }
                }
                else if(cur == '*')
                {
                    if(lastToken == null || lastToken is PipeToken)
                    {
                        curToken = new AllToken(i);
                    }
                    else if(PeekNext(input) == '*')
                    {
                        curToken = new PowToken(i);
                        Index++;
                        i++;
                    }
                    else
                    {
                        curToken = new ProductToken(i);
                    }
                }
                else if (cur == '?')
                {
                    if (PeekNext(input) == '?')
                    {
                        curToken = new LastToken(i);
                        Index++;
                        i++;
                    }
                    else
                    {
                        curToken = new FirstToken(i);
                    }
                }
                else if (cur == '-')
                {
                    if(PeekNext(input) == '>')
                    {
                        curToken = new PipeToken(i, "->");
                        Index++;
                        i++;
                    }
                    else
                    {
                        curToken = new SubtractionToken(i);
                    }
                }
                else if (cur == '!')
                {
                    if (PeekNext(input) == '=')
                    {
                        curToken = new ComparisonToken(i, "!=", ComparisonModes.NotEqual);
                        Index++;
                        i++;
                    }
                    else
                    {
                        curToken = new NotToken(i, "!");
                    }
                }
                else if(cur == '>')
                {
                    if (PeekNext(input) == '=')
                    {
                        curToken = new ComparisonToken(i, ">=", ComparisonModes.GreaterOrEqual);
                        Index++;
                        i++;
                    }
                    else
                    {
                        curToken = new ComparisonToken(i, ">", ComparisonModes.Greater);
                    }
                }
                else if (cur == '<')
                {
                    if (PeekNext(input) == '=')
                    {
                        curToken = new ComparisonToken(i, "<=", ComparisonModes.LessOrEqual);
                        Index++;
                        i++;
                    }
                    else
                    {
                        curToken = new ComparisonToken(i, "<", ComparisonModes.Less);
                    }
                }
                else if(char.IsNumber(cur))
                {
                    var word = ReadFloat(input);
                    if(word.Contains('.'))
                    {
                        curToken = new RealToken(i, word);
                    }
                    else
                    {
                        curToken = new IntToken(i, word);
                    }
                }
                else
                {
                    // Word-length tokens
                    var curWord = AdvanceWord(input);
                    if (curWord.Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        curToken = new AllToken(i, "all");
                    }
                    else if (curWord.Equals("first", StringComparison.OrdinalIgnoreCase))
                    {
                        curToken = new FirstToken(i, "first");
                    }
                    else if (curWord.Equals("last", StringComparison.OrdinalIgnoreCase))
                    {
                        curToken = new LastToken(i, curWord.ToString());
                    }
                    else if (curWord.Equals("filter", StringComparison.OrdinalIgnoreCase) ||
                        curWord.Equals("where", StringComparison.OrdinalIgnoreCase))
                    {
                        curToken = new FilterToken(i, curWord.ToString());
                    }
                    else if (curWord.Equals("edit", StringComparison.OrdinalIgnoreCase) ||
                        curWord.Equals("write", StringComparison.OrdinalIgnoreCase) ||
                        curWord.Equals("change", StringComparison.OrdinalIgnoreCase))
                    {
                        curToken = new EditToken(i, curWord.ToString());
                    }
                    else if (curWord.Equals("contains", StringComparison.OrdinalIgnoreCase) ||
                        curWord.Equals("has", StringComparison.OrdinalIgnoreCase))
                    {
                        curToken = new ContainsToken(i, curWord.ToString());
                    }
                    else if (curWord.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        curToken = new BoolToken(i, curWord.ToString(), true);
                    }
                    else if (curWord.Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        curToken = new BoolToken(i, curWord.ToString(), false);
                    }
                    else if (curWord.Equals("or", StringComparison.OrdinalIgnoreCase))
                    {
                        curToken = new OrToken(i);
                    }
                    else if (curWord.Equals("and", StringComparison.OrdinalIgnoreCase))
                    {
                        curToken = new AndToken(i);
                    }
                    else if (curWord.Equals("not", StringComparison.OrdinalIgnoreCase))
                    {
                        curToken = new NotToken(i);
                    }
                    else
                    {
                        curToken = new IdentifierToken(curWord, i);
                    }
                    i = Index;
                }

                 
                lastToken = curToken;
                yield return curToken;
            }

            yield return new EndOfStreamToken(Index);
        }

        private char PeekNext(string input)
        {
            return Index < input.Length - 1 ? input[Index + 1] : '\0';
        }

        private string AdvanceWord(string input)
        {
            int start = Index;
            int count = 0;
            for (int i = start; i < input.Length; i++)
            {
                char cur = input[i];
                if (char.IsWhiteSpace(cur) || cur == '(' || cur == ')' || cur == ',' || cur == '-')
                {
                    break;
                }
                count++;
                Index = i;
            }

            return input.Substring(start, count);
        }

        private string AdvancePast(string input, char target)
        {
            int start = Index;
            for (int i = Index + 1; i < input.Length; i++)
            {
                Index = i;
                if (input[i] == target)
                {
                    break;
                }
            }

            return input.Substring(start, Index - start);
        }

        private string ReadFloat(string input)
        {
            int start = Index;
            bool decimalReached = false;
            int count = 0;
            for (int i = Index; i < input.Length; i++)
            {
                if (!char.IsNumber(input[i]))
                {
                    if (input[i] == '.' && !decimalReached)
                    {
                        decimalReached = true;
                    }
                    else
                    {
                        break;
                    }
                }
                count++;
                Index++;
            }
            Index--;
            return input.Substring(start, count);
        }
    }
}
