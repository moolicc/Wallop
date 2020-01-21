// At some point, we need this to increment the build number for associated projects.
// Also allow a way to increment the major and minor numbers.

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Debug");
var platform = Argument("platform", "Automatic");

var solution = "./WallApp.sln";

public void SetConfiguration(MSBuildSettings settings)
{
    settings.Configuration = configuration;
    settings.MSBuildPlatform = (MSBuildPlatform)Enum.Parse(typeof(MSBuildPlatform), platform);
}

Task("Restore").Does(() => NuGetRestore(solution));
Task("Rebuild").IsDependentOn("Clean").IsDependentOn("Build");
Task("Package").IsDependentOn("Rebuild").IsDependentOn("CopyOutput")
    .Does(() =>
{

});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    MSBuild(solution, SetConfiguration);
    RunTarget("CopyOutput");
});

Task("Clean")
    .Does(() =>
{
    var directories = new string[]
    {
        string.Format("./Engine/obj/{0}", configuration),
        string.Format("./Engine/bin/{0}", configuration),
        string.Format("./WallApp.App/obj/{0}", configuration),
        string.Format("./WallApp.App/bin/{0}", configuration)
    };

    foreach (var item in directories)
    {
        Information($"Cleaning '{item}'...");
        CleanDirectories(item);
    }

    Information($"Cleaned {directories.Length} directories");
});

Task("CopyOutput")
    .Does(() =>
{
    var engineDirectory = $"./Engine/bin/{configuration}/";
    var appDirectory = $"./WallApp.App/bin/{configuration}/";
    var libDirectory = $"{appDirectory}lib/";
    var modulesDirectory = $"./modules/";

    // Copy Engine's output to App output.
    Information($"Copying files from '{engineDirectory}' to '{appDirectory}'...");
    CopyFiles(engineDirectory + "*", appDirectory);

    // Create subfolder for third-party libraries.
    Information($"Recreating '{libDirectory}'...");
    if(DirectoryExists(libDirectory))
    {
        DeleteDirectory(libDirectory, new DeleteDirectorySettings() { Recursive = true, Force = true });
    }
    CreateDirectory(libDirectory);

    // Copy third-party libraries to sub-folder.
    var files = System.IO.Directory.GetFiles(MakeAbsolute(DirectoryPath.FromString(appDirectory)).ToString());
    Information("\r\nCopying libs======================");
    foreach(var file in files)
    {
        // Don't copy *our* files.
        if(file.EndsWith("WallApp.exe")
            || file.EndsWith("WallApp.pdb")
            || file.EndsWith("WallApp.exe.config"))
        {
            continue;
        }
        if(file.EndsWith("Engine.exe")
            || file.EndsWith("Engine.pdb")
            || file.EndsWith("Engine.exe.config"))
        {
            continue;
        }

        
        var withoutExtension = System.IO.Path.GetFileNameWithoutExtension(file);
        var name = System.IO.Path.GetFileName(file);

        // If the file is a dll or pdb, move it to the subfolder.
        if(file.EndsWith(".dll")
            || file.EndsWith(".pdb"))
        {
            Information($"Moving '{name}' to lib directory...");
            MoveFileToDirectory(file, libDirectory);
        }

        // If the file is an xml and also there is an accompanying library, then
        // this xml must be docs for the library -- move it.
        if(file.EndsWith(".xml")
            && files.Any((f) => f.EndsWith(withoutExtension + ".dll")))
        {
            Information($"Moving '{name}' to lib directory...");
            MoveFileToDirectory(file, libDirectory);
        }
    }
    Information("Finished copying libs======================\r\n");

    // To copy any subdirectories for libs (the c# scripting stuff seems to be localized).
    /*
    foreach(var item in GetSubDirectories(appDirectory))
    {
        if(item.FullPath.EndsWith("lib"))
        {
            continue;
        }
        MoveDirectory(item, libDirectory)
    }
    */

    // Copy the modules folder in the root of the solution to the modules sub directory.
    Information($"Copying '{modulesDirectory}' to '{appDirectory}modules/'...");
    CopyDirectory(modulesDirectory, appDirectory + "modules/");
});


RunTarget(target);