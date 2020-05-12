using System;
using System.Collections.Generic;
using System.Text;
using Wallop.Cmd.Parsing.Tokens;
using System.Linq;

namespace Wallop.Cmd.Parsing
{
    public class Parser
    {
        public Tokenizer Tokenizer { get; private set; }
        public string Source { get; private set; }
        public CommandTable CommandTable { get; private set; }

        private Token[] _tokens;

        public Parser(CommandSet commandSet, string source)
            : this(CommandTable.FromSet(commandSet), source)
        {
        }

        public Parser(CommandTable commandTable, string source)
        {
            Tokenizer = new Tokenizer();
            CommandTable = commandTable;
            Source = source;

            _tokens = Tokenizer.GetTokens(source, CommandTable.Selectors.ToArray()).ToArray();
        }

        public ParseResults Parse()
        {
            NamedTokenResults currentCommand = NamedTokenResults.Empty;
            NamedTokenResults currentSelector = NamedTokenResults.Empty;
            List<ArgResults> currentArgs = new List<ArgResults>();

            void SetCommand(CommandToken token)
            {
                if (!currentCommand.IsEmpty())
                {
                    AddCommand();
                }
                currentCommand = new NamedTokenResults(token.Name, token);
            }

            void SetSelector(SelectorToken token)
            {
                if(!currentSelector.IsEmpty())
                {
                    // We got a BIG problem...
                }
                currentSelector = new NamedTokenResults(token.Name, token);
            }

            void AddCommand()
            {
                currentCommand = NamedTokenResults.Empty;
                currentSelector = NamedTokenResults.Empty;
            }

            T Peek<T>(int index) where T : Token
            {
                if(_tokens.Length >= index + 1)
                {
                    return null;
                }
                return _tokens[index + 1] as T;
            }

            for(int i = 0; i < _tokens.Length; i++)
            {
                switch (_tokens[i])
                {
                    case CommandToken cmdToken:
                        SetCommand(cmdToken);
                        break;
                    case ArgNameToken argNametoken:
                        if(Peek<ArgValueToken>(i) is ArgValueToken valueToken)
                        {
                            currentArgs.Add(new ArgResults(argNametoken.Name, valueToken.Value, argNametoken, valueToken));
                            i++;
                        }
                        else
                        {
                            currentArgs.Add(new ArgResults(argNametoken.Name, null, argNametoken, null));
                        }
                        break;
                    case ArgValueToken argValueToken:
                        currentArgs.Add(new ArgResults(null, argValueToken.Value, null, argValueToken));
                        break;
                    case SelectorToken selectorToken:
                        SetSelector(selectorToken);
                        break;
                }
            }

            return null;
        }

        private ResolvedCommand ResolveCommand(NamedTokenResults parsedCommand, string selector, ArgResults[] args)
        {
            ResolvedCommand results = ResolvedCommand.Empty;
            Command command = null;


            // TODO: Resolve command from command table.
            // Denote the correct command by its index, and store the index in ResolvedCommand.Index.

            // Take args and transform them into ResolvedCommand.ArgValues.


            return results;
        }

        private struct ResolvedCommand
        {
            public static readonly ResolvedCommand Empty
                = new ResolvedCommand() { Index = -1, ArgValues = new Dictionary<string, object>() };

            public int Index;
            public Dictionary<string, object> ArgValues;
        }

        private struct NamedTokenResults
        {
            public static readonly NamedTokenResults Empty
                = new NamedTokenResults { Name = null, Token = null, };

            public string Name;
            public Token Token;

            public NamedTokenResults(string name, Token token)
            {
                Name = name;
                Token = token;
            }

            public bool IsEmpty()
            {
                return Name == null && Token == null;
            }

            public static bool operator ==(NamedTokenResults a, NamedTokenResults b)
            {
                return a.Name == b.Name && a.Token == b.Token;
            }

            public static bool operator !=(NamedTokenResults a, NamedTokenResults b)
            {
                return a.Name != b.Name || a.Token != b.Token;
            }
        }

        private struct ArgResults
        {
            public static readonly ArgResults Empty
                   = new ArgResults { Name = null, Value = null, NameToken = null, ValueToken = null, };

            public string Name;
            public string Value;
            public Token NameToken;
            public Token ValueToken;

            public ArgResults(string name, string value, Token nameToken, Token valueToken)
            {
                Name = name;
                Value = value;
                NameToken = nameToken;
                ValueToken = valueToken;
            }

            public bool IsEmpty()
            {
                return Name == null && Value == null && NameToken == null && ValueToken == null;
            }

            public static bool operator ==(ArgResults a, ArgResults b)
            {
                return a.Name == b.Name && a.Value == b.Value && a.NameToken == b.NameToken && a.ValueToken == b.ValueToken;
            }

            public static bool operator !=(ArgResults a, ArgResults b)
            {
                return a.Name != b.Name || a.Value != b.Value || a.NameToken != b.NameToken || a.ValueToken != b.ValueToken;
            }
        }
    }
}
