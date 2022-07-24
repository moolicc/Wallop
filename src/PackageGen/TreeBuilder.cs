using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageGen
{
    public static class TreeExtensions
    {
        public static TreeNode SetRoot(this TreeBuilder builder, string rootText)
        {
            return builder.SetRoot(new TreeNode(rootText));
        }

        public static TreeNode AddChild(this ITreeNode parent, string text)
        {
            var node = new TreeNode(text);
            parent.Children.Add(node);
            return node;
        }

        public static CheckedTreeNode AddChild(this ITreeNode parent, string text, bool isChecked)
        {
            var node = new CheckedTreeNode(text, isChecked);
            parent.Children.Add(node);
            return node;
        }

        public static void AddChildren(this ITreeNode parent, IEnumerable<string> childrenText)
        {
            foreach (var child in childrenText)
            {
                AddChild(parent, child);
            }
        }
    }

    public interface ITreeNode
    {
        List<ITreeNode> Children { get; init; }

        void Print();
        string GetText();
    }

    public readonly record struct CheckedTreeNode : ITreeNode
    {
        public ConsoleColor BackColor { get; init; }
        public ConsoleColor ForeColor { get; init; }
        public bool IsChecked { get; init; }

        public string Text { get; init; }
        public List<ITreeNode> Children { get; init; }

        public CheckedTreeNode(string text, bool isChecked)
            : this(text, isChecked, Console.BackgroundColor, Console.ForegroundColor)
        {
        }

        public CheckedTreeNode(string text, bool isChecked, ConsoleColor backColor, ConsoleColor foreColor)
        {
            Text = text;
            IsChecked = isChecked;
            BackColor = backColor;
            ForeColor = foreColor;
            Children = new List<ITreeNode>();
        }

        public void Print()
        {
            var curBack = Console.BackgroundColor;
            var curFore = Console.ForegroundColor;

            Console.BackgroundColor = BackColor;
            Console.ForegroundColor = ForeColor;

            Console.Write(GetText());

            Console.BackgroundColor = curBack;
            Console.ForegroundColor = curFore;
        }

        public string GetText()
            => string.Format("[{0}] {1}", IsChecked ? "X" : " ", Text);
    }

    public readonly record struct TreeNode : ITreeNode
    {
        public ConsoleColor BackColor { get; init; }
        public ConsoleColor ForeColor { get; init; }

        public string Text { get; init; }
        public List<ITreeNode> Children { get; init; }

        public TreeNode(string text)
            : this(text, Console.BackgroundColor, Console.ForegroundColor)
        {
        }

        public TreeNode(string text, ConsoleColor backColor, ConsoleColor foreColor)
        {
            Text = text;
            Children = new List<ITreeNode>();
            BackColor = backColor;
            ForeColor = foreColor;
        }

        public void Print()
        {
            var curBack = Console.BackgroundColor;
            var curFore = Console.ForegroundColor;

            Console.BackgroundColor = BackColor;
            Console.ForegroundColor = ForeColor;

            Console.Write(Text);

            Console.BackgroundColor = curBack;
            Console.ForegroundColor = curFore;
        }

        public string GetText()
            => Text;
    }

    public class TreeBuilder
    {
        public ITreeNode? RootNode { get; set; }


        public T SetRoot<T>(T node)
            where T : ITreeNode
        {
            RootNode = node;
            return node;
        }

        public void Print()
        {
            if(RootNode == null)
            {
                throw new ArgumentNullException(nameof(RootNode));
            }

            var rootLevel = new TreeLevel();
            RootNode.Print();
            Console.WriteLine();

            for(int i = 0; i < RootNode.Children.Count; i++)
            {
                AppendNode(null, i == RootNode.Children.Count - 1, RootNode.Children[i], RootNode, rootLevel);
            }
        }

        public override string ToString()
        {
            if (RootNode == null)
            {
                throw new ArgumentNullException(nameof(RootNode));
            }

            var builder = new StringBuilder();

            var rootLevel = new TreeLevel();
            builder.AppendLine(RootNode.GetText());

            for(int i = 0; i < RootNode.Children.Count; i++)
            {
                AppendNode(builder, i == RootNode.Children.Count - 1, RootNode.Children[i], RootNode, rootLevel);
            }

            return builder.ToString();
        }

        private void AppendNode(StringBuilder? builder, bool isParentsLastChild, ITreeNode node, ITreeNode parentNode, TreeLevel parentLevel)
        {
            var level = new TreeLevel(parentLevel, isParentsLastChild, parentNode.Children.Count == 1, true, node.Children.Count > 0);

            level.AppendLevel(builder);

            if(builder == null)
            {
                node.Print();
                Console.WriteLine();
            }
            else
            {
                builder.AppendLine(node.GetText());
            }

            if (level.Parent != null && isParentsLastChild)
            {
                level.BeyondLastChild = true;
            }

            level.LevelIsChild = false;

            for (int i = 0; i < node.Children.Count; i++)
            {
                AppendNode(builder, i == node.Children.Count - 1, node.Children[i], node, level);
            }

        }


        private class TreeLevel
        {
            private const string BEGIN_LAST_CHILD = "└── ";
            private const string BEGIN_CHILD = "├── ";
            private const string SUB_CHILD = "│   ";
            private const string INDENTION = "   ";
            private const string BEYOND_CHILDREN_INDENTION = "    ";

            public TreeLevel? Parent;
            public bool IsLastChild;
            public bool BeyondLastChild;
            public bool LevelHasChild;
            public bool LevelIsChild;

            public TreeLevel()
            {
                Parent = null;
                IsLastChild = true;
                LevelHasChild = false;
            }

            public TreeLevel(TreeLevel parent, bool isLastChild, bool beyondLastChild, bool levelIsChild, bool hasSubChild)
            {
                Parent = parent;
                IsLastChild = isLastChild;
                BeyondLastChild = beyondLastChild;
                LevelIsChild = levelIsChild;
                LevelHasChild = hasSubChild;
            }

            public void AppendLevel(StringBuilder? builder)
            {
                if(Parent == null)
                {
                    return;
                }
                Parent.AppendLevel(builder);


                if (IsLastChild && LevelIsChild)
                {
                    AppendOrPrint(builder, BEGIN_LAST_CHILD);
                }
                else if (!IsLastChild && LevelIsChild)
                {
                    AppendOrPrint(builder, BEGIN_CHILD);
                }
                else if (!BeyondLastChild && LevelHasChild)
                {
                    AppendOrPrint(builder, SUB_CHILD);
                }
                else if(!BeyondLastChild)
                {
                    AppendOrPrint(builder, SUB_CHILD);
                    AppendOrPrint(builder, INDENTION);
                }
                else
                {
                    AppendOrPrint(builder, BEYOND_CHILDREN_INDENTION);
                }
            }

            private void AppendOrPrint(StringBuilder? builder, string text)
            {
                if(builder == null)
                {
                    Console.Write(text);
                }
                else
                {
                    builder.Append(text);
                }
            }
        }
    }
}
