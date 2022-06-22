param(
    [parameter(Mandatory=$true)]
    [Alias("project")]
    [string]$projectName,

    [Alias("version")]
    [string]$newPrefix,

    [AllowEmptyString()]
    [string]$newSuffix

)

Set-Location -Path $PSScriptRoot

#$allFriendlies = @("Alef", "Bet", "Gimel", "Dalet", "He", "Vav", "Zayin", "Chet", "Tet", "Yod", "Kaf", "Lamed", "Mem", "Nun", "Samech", "Ayin", "Pe", "Tsadi", "Qof", "Resh", "Shin", "tav")
$allProjects = @{ "engine"="../src/Wallop.Engine/Wallop.Engine.csproj" }


# Set the new version information in the text file.
Write-Output ("Changing version in version.txt: {0}-{1}" -f $newPrefix, $newSuffix);
Set-Content -Path Version.txt -Value "${newPrefix}`n${newSuffix}"

# Find the csproj.
Write-Output "Finding project file..."
$projectFile = $allProjects[$projectName]
Write-Output "Working on project ${projectFile}"

# Load the csproj.
$file = resolve-path($projectFile)
$xdoc = new-object System.Xml.XmlDocument
$xdoc.load($file)

# Get the current version of the csproj.
$oldPrefix = [version]$xdoc.Project.PropertyGroup.VersionPrefix
$oldSuffix = $xdoc.Project.PropertyGroup.VersionSuffix
Write-Output ("Current project version: {0}-{1}" -f $oldPrefix.ToString(3), $oldSuffix)

# Set the new version of the csproj
$xdoc.Project.PropertyGroup.VersionPrefix = $newPrefix
$xdoc.Project.PropertyGroup.VersionSuffix = $newSuffix

$xdoc.Save($file)