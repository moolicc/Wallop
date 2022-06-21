param(
    [parameter(Mandatory=$true)]
    [Alias("project")]
    [string]$projectName,

    [switch]
    [Alias("major")]
    $incrMajor,

    [switch]
    [Alias("minor")]
    $incrMinor,

    [switch]
    [Alias("patch")]
    $incrPatch
)

$allFriendlies = @("Alef", "Bet", "Gimel", "Dalet", "He", "Vav", "Zayin", "Chet", "Tet", "Yod", "Kaf", "Lamed", "Mem", "Nun", "Samech", "Ayin", "Pe", "Tsadi", "Qof", "Resh", "Shin", "tav")
$allProjects = @{ "engine"="../src/Wallop.Engine/Wallop.Engine.csproj" }

Set-Location -Path $PSScriptRoot


# Get the current product version information from the text file.
$friendly = Get-Content Version.txt -First 1
$major = Get-Content Version.txt | Select-Object -Skip 1 -First 1

# If we're changing the major (and friendly), re-write the new information.
if($incrMajor)
{
    $major = [int]$major + 1
    $friendly = $allFriendlies[$major]
    Write-Output ("Changing major in version.txt: {0} {1}.*.*" -f $friendly, $major);
    Set-Content -Path Version.txt -Value "${friendly}`n${major}"
}

# Write version product information.
Write-Output "Product version: ${friendly} ${major}.*.*"

Write-Output "Finding project file..."
$projectFile = $allProjects[$projectName]
Write-Output "Working on project ${projectFile}"

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

if(Test-Path -Path $file)
{
    $xdoc.Save($file)
}
else
{
    Write-Error "Failed to save project xml."
}

Write-Output "NEW_VERSION=${projVer.ToString(3)}" | Out-File -FilePath $Env:GITHUB_ENV -Encoding utf-8 -Append