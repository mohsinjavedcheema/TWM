param (
[string]$ConfuserPath,
[string]$ConfuserConfigurationFile,
[string]$ObjDir,
[string]$ModuleName,
[string]$ModuleNameChart,
[string]$ModuleNameCore,
[string]$ModuleNameDB,
[string]$ModuleNameModel,
[string]$BinDir,
[string]$ReleaseDir
)

function replace-file-content([string] $path, [string] $replace, [string] $replaceWith)
{
    (Get-Content $path) |
    Foreach-Object {$_ -replace $replace,$replaceWith}|
    Out-File $path
}

$ConfuserPath = $ConfuserPath.Substring(1,$ConfuserPath.Length-2)
$ConfuserConfigurationFile = $ConfuserConfigurationFile.Substring(1,$ConfuserConfigurationFile.Length-2)
$ObjDir = $ObjDir.Substring(1,$ObjDir.Length-2)
$ModuleName = $ModuleName.Substring(1,$ModuleName.Length-2)
$ModuleNameChart = $ModuleNameChart.Substring(1,$ModuleNameChart.Length-2)
$ModuleNameCore = $ModuleNameCore.Substring(1,$ModuleNameCore.Length-2)
$ModuleNameDB = $ModuleNameDB.Substring(1,$ModuleNameDB.Length-2)
$ModuleNameModel = $ModuleNameModel.Substring(1,$ModuleNameModel.Length-2)


echo "dddd..." $ConfuserPath

# Let's first copy the configuration file to a temporary directory

echo "Obfuscating..."
$tempFile = [string]::Concat($ObjDir, "confuser.temp.crproj")
echo "Copying " $ConfuserConfigurationFile " to " $tempFile
Copy-Item $ConfuserConfigurationFile -Destination $tempFile

echo "Replacing placeholders..."
replace-file-content $tempFile "{OutputDir}" $ObjDir
replace-file-content $tempFile "{ModuleName}" $ModuleName
replace-file-content $tempFile "{ModuleNameChart}" $ModuleNameChart
replace-file-content $tempFile "{ModuleNameCore}" $ModuleNameCore
replace-file-content $tempFile "{ModuleNameDB}" $ModuleNameDB
replace-file-content $tempFile "{ModuleNameModel}" $ModuleNameModel
replace-file-content $tempFile "{ReleaseDir}" $ReleaseDir

echo "Performing Obfuscation..."
$parameter = [string]::Concat("""",$tempFile,"""")
$ConfuserPath = [string]::Concat("""",$ConfuserPath,"""")
echo $parameter
#start-process -wait "C:\Programs\ConfuserEx\Confuser.CLI.exe" "$parameter"
start-process -NoNewWindow -wait "$ConfuserPath" "$parameter"
echo "Obfuscation complete."
echo "Copying to Bin Dir "
echo $ModuleName
echo $BinDir
Copy-Item $ModuleName $BinDir