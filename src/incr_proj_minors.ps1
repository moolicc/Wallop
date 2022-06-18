
# Designed to be run with the relative paths to the files that have changed
# eg: src/Wallop.Engine/ModuleLog.cs src/Wallop.Engine/Program.cs

for($i = 0; $i -lt $args.Length; $i++) {
    $args[$i] = $args[$i].Replace("src/", "")
}


foreach($_ in Get-ChildItem -Path "*.csproj" -Recurse) {
    # Get directory: Split-Path -Path ($_)
    $dir = Split-Path -Path $_

    $count = ($args | where { (Get-Item $_ | Resolve-Path).ProviderPath.StartsWith($dir) }).Count
    if($count -ge 1)
    {
        write-host "Calling version increment script on " $_
        & "./incrversion.ps1" $_ -minor
    }
}