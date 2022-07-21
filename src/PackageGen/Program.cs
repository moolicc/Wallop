
using PackageGen;
using System.CommandLine;
using Wallop.DSLExtension.Modules;

Console.WriteLine("Hello, World!");

var package = PackageLoader.LoadPackages(@"C:\Users\joel\source\repos\Wallop\modules\squaretest").First();

Console.WriteLine(package.ToTreeString());

Console.ReadLine();


var command = BuildCommandTree();

bool repl = true;
while (repl)
{
    var input = Console.ReadLine();


    repl = false;
}


RootCommand BuildCommandTree()
{
    var root = new RootCommand("Package generator and editor.");

    return root;
}