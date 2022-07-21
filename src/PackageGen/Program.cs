
using System.CommandLine;

Console.WriteLine("Hello, World!");

var command = BuildCommandTree();

bool repl = true;
while (repl)
{
    var input = Console.ReadLine();


    repl = false;
}


RootCommand BuildCommandTree()
{
    var root = new RootCommand("");

    return root;
}