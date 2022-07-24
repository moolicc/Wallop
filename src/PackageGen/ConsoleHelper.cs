using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageGen
{
    public readonly record struct PromptRun(string Text, ConsoleColor Color)
    {
        public static implicit operator string(PromptRun run)
        {
            return run.Text;
        }

        public static implicit operator PromptRun(string text)
        {
            return new PromptRun(text, Console.ForegroundColor);
        }
    }

    public readonly record struct Prompt
    {
        public List<PromptRun> Runs { get; init; }
        public int Length => Runs.Select(r => r.Text.Length).Aggregate((x, y) => x + y);

        public Prompt()
        {
            Runs = new List<PromptRun>();
        }
        public Prompt(params PromptRun[] runs)
        {
            Runs = new List<PromptRun>(runs);
        }

        public void Print()
        {
            var color = Console.ForegroundColor;
            foreach (var item in Runs)
            {
                Console.ForegroundColor = item.Color;
                Console.Write(item.Text);
            }
            Console.ForegroundColor = color;
        }

        public static implicit operator Prompt(string text)
        {
            var newP = new Prompt();
            newP.Runs.Add(new PromptRun(text, Console.ForegroundColor));
            return newP;
        }

        public static explicit operator string(Prompt text)
        {
            var builder = new StringBuilder();

            foreach (var item in text.Runs)
            {
                builder.Append(item.Text);
            }

            return builder.ToString();
        }
    }

    public readonly record struct Keyword(string Value, ConsoleColor Color);

    public class CompletionTree
    {
        public string TriggerWord { get; set; }
        public string[] Aliases { get; set; }
        public List<CompletionTree> Completions { get; private set; }

        public CompletionTree(string word, string[] aliases)
        {
            TriggerWord = word;
            Aliases = aliases;
            Completions = new List<CompletionTree>();
        }

        public string? FindCompletion(string start, int startingIndex, out bool wrapped)
        {
            wrapped = true;

            int found = 0;
            string? first = null;
            for (int i = 0; i < Completions.Count; i++)
            {
                if (Completions[i].TriggerWord.StartsWith(start, StringComparison.OrdinalIgnoreCase))
                {
                    if(found == startingIndex)
                    {
                        wrapped = false;
                        return Completions[i].TriggerWord;
                    }
                    else
                    {
                        if(first == null)
                        {
                            first = Completions[i].TriggerWord;
                        }
                        found++;
                    }
                }
                else
                {
                    foreach (var item in Completions[i].Aliases)
                    {
                        if(item.StartsWith(start, StringComparison.OrdinalIgnoreCase))
                        {
                            if (found == startingIndex)
                            {
                                wrapped = false;
                                return item;
                            }
                            else
                            {
                                if (first == null)
                                {
                                    first = item;
                                }
                                found++;
                            }
                        }
                    }
                }
            }

            return first;
        }
    }

    public class ConsoleHelper
    {
        private const string PROMPT = "> ";

        public Prompt Prompt { get; set; }

        public List<Keyword> Keywords { get; private set; }

        public List<CompletionTree> Completions => _completionRoot.Completions;

        private CompletionTree _completionRoot;
        private CompletionTree? _completionNode;
        private int _completionIndex;
        private bool _cyclingCompletions;
        private (string Word, int StartPos, int EndPos) _completionCycleKey;

        private List<string> _commandHistory;
        private int _commandHistoryIndex;

        public ConsoleHelper()
        {
            _completionRoot = new CompletionTree("", Array.Empty<string>());

            _completionNode = null;
            _completionIndex = 0;
            _cyclingCompletions = false;
            _completionCycleKey = ("", 0, 0);

            Keywords = new List<Keyword>();
            _commandHistory = new List<string>();
            _commandHistoryIndex = -1;

            Prompt = new Prompt();
            Prompt.Runs.Add(new PromptRun(PROMPT, Console.ForegroundColor));
        }

        public string ReadLine()
        {
            Prompt.Print();

            var current = "";
            var key = Console.ReadKey(true);
            while (key.Key != ConsoleKey.Enter)
            {
                var curPos = Console.CursorLeft - Prompt.Length;

                if(key.Key == ConsoleKey.LeftArrow && Console.CursorLeft > Prompt.Length)
                {
                    // We no longer support navigating caret position.
                    //Console.CursorLeft--;
                }
                else if (key.Key == ConsoleKey.RightArrow && curPos < current.Length)
                {
                    // We no longer support navigating caret position.
                    //Console.CursorLeft++;
                }
                else if(key.Key == ConsoleKey.UpArrow && _commandHistory.Count > 0 && _commandHistoryIndex + 1 < _commandHistory.Count)
                {
                    _commandHistoryIndex++;
                    current = HandleCommandHistory(current.Length);
                }
                else if (key.Key == ConsoleKey.DownArrow && _commandHistoryIndex > -1)
                {
                    _commandHistoryIndex--;
                    current = HandleCommandHistory(current.Length);
                }
                else if(key.Key == ConsoleKey.Backspace)
                {
                    if(Console.CursorLeft > Prompt.Length)
                    {
                        Console.CursorLeft--;
                        Console.Write(' ');
                        Console.CursorLeft--;
                        _cyclingCompletions = false;
                        current = current.Remove(current.Length - 1);

                        // We no longer support navigating caret position.
                        //Console.MoveBufferArea(Console.CursorLeft + 1, Console.CursorTop, Console.BufferWidth - Console.CursorLeft - 1, 1, Console.CursorLeft, Console.CursorTop);
                    }
                }
                else if(key.Key == ConsoleKey.Delete)
                {
                    // We no longer support navigating caret position.
                }
                else if (key.Key == ConsoleKey.Tab)
                {
                    if(!_cyclingCompletions)
                    {
                        _completionCycleKey = GetWord(current, curPos);
                        _cyclingCompletions = true;
                        _completionIndex = 0;
                        _completionNode = FindCompletionNode(current);
                    }

                    if(_completionNode != null)
                    {
                        // Find completion starting at index
                        var completionText = _completionNode.FindCompletion(_completionCycleKey.Word, _completionIndex, out var wrapped);
                        if(completionText != null)
                        {
                            //Console.CursorLeft = PROMPT.Length + _completionCycleKey.EndPos;

                            while (Console.CursorLeft > Prompt.Length + _completionCycleKey.EndPos)
                            {
                                Console.Write(' ');
                                Console.CursorLeft -= 2;
                            }
                            Console.Write(completionText.Substring(_completionCycleKey.Word.Length - 1));


                            current = current.Remove(_completionCycleKey.EndPos) + completionText.Substring(_completionCycleKey.Word.Length - 1);

                            _completionIndex++;
                            if(wrapped)
                            {
                                _completionIndex = 1;
                            }
                        }
                    }
                }
                else
                {
                    _cyclingCompletions = false;
                    current += key.KeyChar;
                    Console.Write(key.KeyChar);
                }

                current = current.Trim('\0');

                var curWord = GetWord(current, curPos);
                var kw = GetKeyword(curWord.Word);
                if (kw.HasValue)
                {
                    var curColor = Console.ForegroundColor;
                    var curLeft = Console.CursorLeft;

                    Console.ForegroundColor = kw.Value.Color;
                    Console.CursorLeft = curWord.Start + Prompt.Length;

                    Console.Write(kw.Value.Value);

                    Console.ForegroundColor = curColor;
                    Console.CursorLeft = curLeft;
                }
                else
                {
                    var curLeft = Console.CursorLeft;

                    Console.CursorLeft = curWord.Start + Prompt.Length;

                    Console.Write(curWord.Word);

                    Console.CursorLeft = curLeft;
                }

                key = Console.ReadKey(true);
            }

            var curIndex = _commandHistory.IndexOf(current);
            if (curIndex >= 0)
            {
                _commandHistory.RemoveAt(curIndex);
            }
            _commandHistory.Insert(0, current);
            _commandHistoryIndex = -1;
            Console.WriteLine();
            return current;
        }

        private string HandleCommandHistory(int curLength)
        {
            string item = "";
            Console.CursorLeft = Prompt.Length;
            if (_commandHistoryIndex == -1)
            {
                if(curLength < Console.BufferWidth - 1)
                {
                    curLength++;
                }
                Console.Write(new string(' ', curLength));
                Console.CursorLeft = Prompt.Length;
            }
            else
            {
                item = _commandHistory[_commandHistoryIndex];
                Console.CursorLeft = Prompt.Length + item.Length;
                Console.Write(new string(' ', curLength));
            }

            WriteHighlightedWholeRow(item);

            return item;
        }

        private void WriteHighlightedWholeRow(string row)
        {
            void WriteWord(string word, int start)
            {
                Console.CursorLeft = start + Prompt.Length;
                var kw = GetKeyword(word);
                if (kw.HasValue)
                {
                    var curColor = Console.ForegroundColor;

                    Console.ForegroundColor = kw.Value.Color;

                    Console.Write(kw.Value.Value);

                    Console.ForegroundColor = curColor;
                }
                else
                {
                    Console.Write(word);
                }
            }

            var wordStart = -1;
            var wordLen = 0;
            var curWord = "";
            for (int i = 0; i < row.Length; i++)
            {
                if (char.IsWhiteSpace(row[i]))
                {
                    if(wordStart != -1)
                    {
                        WriteWord(curWord, wordStart);

                        wordStart = -1;
                        curWord = "";
                        wordLen = 0;
                    }
                    Console.Write(row[i]);
                    continue;
                }
                else if (wordStart == -1)
                {
                    wordStart = i;
                }

                curWord += row[i];
                wordLen++;
            }

            if (wordStart != -1)
            {
                WriteWord(curWord, wordStart);
            }
        }

        private Keyword? GetKeyword(string text)
        {
            text = text.Trim();
            foreach (var item in Keywords)
            {
                if(string.Equals(text, item.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }
            return null;
        }

        private CompletionTree? FindCompletionNode(string input)
        {
            CompletionTree result = _completionRoot;
            if(!input.Contains(' '))
            {
                return result;
            }

            var split = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < split.Length - 1; i++)
            {
                var found = false;
                foreach (var node in result.Completions)
                {
                    if (string.Equals(split[i], node.TriggerWord, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        result = node;
                        break;
                    }

                    if(node.Aliases.Any(a => string.Equals(split[i], a, StringComparison.OrdinalIgnoreCase)))
                    {
                        found = true;
                        result = node;
                        break;
                    }
                }
                if(!found)
                {
                    break;
                }
            }

            return result;
        }

        private static (string Word, int Start, int End) GetWord(string input, int referencePosition)
        {


            int startIndex = 0;
            int endIndex = input.Length - 1;

            for (int i = referencePosition; i >= 0; i--)
            {
                if(i >= input.Length)
                {
                    continue;
                }
                if (input[i] == ' ' && i != referencePosition)
                {
                    if (i + 1 >= input.Length)
                    {
                        startIndex = i;
                    }
                    else
                    {
                        startIndex = i + 1;
                    }
                    break;
                }
            }

            for (int i = referencePosition; i < input.Length; i++)
            {
                if (input[i] == ' ' && i != referencePosition)
                {
                    if (i - 1 < 0)
                    {
                        endIndex = i;
                    }
                    else
                    {
                        endIndex = i - 1;
                    }
                    break;
                }
            }


            return (input.Substring(startIndex, endIndex - startIndex + 1), startIndex, endIndex);
        }
    }
}
