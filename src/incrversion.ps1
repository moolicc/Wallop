param(
    [parameter(Mandatory=$true)]
    [Alias("file")]
    $projectFile,

    [switch]
    [Alias("minor")]
    $incrMinor,

    [switch]
    [Alias("patch")]
    $incrPatch
)

Set-Location -Path $PSScriptRoot


# Get the current product version information from the text file.
$friendly = Get-Content Version.txt -First 1
$major = Get-Content Version.txt | Select-Object -Skip 1 -First 1

# Write version product information.
Write-Output "Product version: ${friendly} ${major}.*.*"

# Load the csproj.
$xdoc = new-object System.Xml.XmlDocument
$file = resolve-path($projectFile)
$xdoc.load($file)

# Get the current version of the csproj.
$projVer = [version]$xdoc.Project.PropertyGroup.Version
$projFriendly = $xdoc.Project.PropertyGroup.InformationalVersion
Write-Output ("Current version: {0} {1}" -f $projFriendly, $projVer.ToString(3))


$projMajor = $projVer.Major
$projMinor = $projVer.Minor
$projPatch = $projVer.Build

# Increment minor and patch numbers as specified in the command line args.
if ($incrMinor) {
    $projMinor++
}

if ($incrPatch) {
    $projPatch++
}

# If the major version is changing, reset minor and patch numbers to zero.
if($projMajor -ne $major -or $friendly -ne $projFriendly) {
    $projMinor = 0
    $projPatch = 0
}


$projMajor = $major
$projFriendly = $friendly

# Create a new version object representing the new version.
$projVer = New-Object System.Version($projMajor, $projMinor, $projPatch, 0)

# Write the new version.
Write-Output ("New version: {0} {1}" -f $projFriendly, $projVer.ToString(3))

$xdoc.Project.PropertyGroup.Version = $projVer.ToString(3)
$xdoc.Project.PropertyGroup.InformationalVersion = [string]$projFriendly

# Save the csproj.
$scriptFolder = $MyInvocation.MyCommand.Path | Split-Path -Parent

if(Test-Path -Path $projectFile)
{
    $xdoc.Save($projectFile)
}
else
{
    $path = Join-Path -Path $scriptFolder -ChildPath $projectFile
    $xdoc.Save($path)
}