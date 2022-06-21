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


# Finally, call the incrementation script.
$scriptFile = Join-Path -Path $PSScriptRoot -ChildPath "incrversion.ps1"
$command = "{0} {1} {2} {3} {4}" -f $scriptFile, $projectArg, $majorArg, $minorArg, $patchArg
Write-Output ("Running {0}" -f $command)
Invoke-Expression $command