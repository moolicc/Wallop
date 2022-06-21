# Call with labels as command line arguments.
#eg. ./pr_label_checker.ps1 bug patch


# First, find the associated projects.
$engine = ($args | Where { $_ -eq "engine" }) -gt 0

$projectArg = ""
if($engine)
{
    $projectArg = "engine"
}

# Second, find the version incrementation information
$major = ($args | Where { $_ -eq "breaking-change"-or $_ -eq "major" }) -gt 0
$minor = ($args | Where { $_ -eq "feature" -or $_ -eq "minor" }) -gt 0
$patch = ($args | Where { $_ -eq "bug" -or $_ -eq "patch" }) -gt 0

$dev = ($args | Where { $_ -eq "develop" }) -gt 0
$beta = ($args | Where { $_ -eq "beta" }) -gt 0
$release = ($args | Where { $_ -eq "main" }) -gt 0

$majorArg = "-major:`$false"
if($major)
{
    $majorArg = "-major"
}


$minorArg = "-minor:`$false"
if($minor)
{
    $minorArg = "-minor"
}

$patchArg = "-patch:`$false"
if($patch)
{
    $patchArg = "-patch"
}

$suffix = ""
if($dev)
{
    $suffix = "dev"
}
elseif($beta)
{
    $suffix = "beta"
}
elseif($release)
{
    $suffix = ""
}


# Finally, call the incrementation script.
$scriptFile = Join-Path -Path $PSScriptRoot -ChildPath "incrversion.ps1"
$command = "{0} {1} {2} {3} {4} {5}" -f $scriptFile, $projectArg, $suffix, $majorArg, $minorArg, $patchArg
Write-Output ("Running {0}" -f $command)
Invoke-Expression $command