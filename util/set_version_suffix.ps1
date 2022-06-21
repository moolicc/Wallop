param(
    [parameter(Mandatory=$true)]
    [Alias("project")]
    [string]$projectName,

    [parameter(Mandatory=$true)]
    [AllowEmptyString()]
    [string]$suffix
)


Set-Location -Path $PSScriptRoot

$allProjects = @{ "engine"="../src/Wallop.Engine/Wallop.Engine.csproj" }
$projectFile = $allProjects[$projectName]

$xdoc = new-object System.Xml.XmlDocument
$file = resolve-path($projectFile)
$xdoc.load($file)

$xdoc.Project.PropertyGroup.VersionSuffix = $suffix

$xdoc.Save($file)