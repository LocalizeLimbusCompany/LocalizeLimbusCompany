param(
    [string]$version
)
$Path = "Release"
if (Test-Path $Path)
	{
	Remove-Item -Path "$Path" -Recurse
	}
# ----------- MelonLoader -----------
dotnet build src/LimbusLocalize.sln -c ML
# Full
New-Item -Path "$Path" -Name "LimbusLocalize" -ItemType "directory" -Force
New-Item -Path "$Path/LimbusLocalize" -Name "Mods" -ItemType "directory" -Force
New-Item -Path "$Path/LimbusLocalize/Mods" -Name "Localize" -ItemType "directory" -Force
Copy-Item -Path Localize/CN $Path/LimbusLocalize/Mods/Localize -Force -Recurse
Copy-Item -Path Localize/Readme $Path/LimbusLocalize/Mods/Localize -Force -Recurse
Copy-Item -Path $Path/LimbusLocalize.dll -Destination $Path/LimbusLocalize/Mods -Force
Set-Location "$Path/LimbusLocalize"
7z a -t7z "../LimbusLocalize_$version.7z" "Mods/" -mx=9 -ms
Set-Location "../../"
$tag=$(git describe --tags --abbrev=0)
$changedFiles=$(git diff --name-only HEAD $tag -- Localize/CN/)
$changedFiles2=$(git diff --name-only HEAD $tag -- Localize/Readme/)
# OTA
New-Item -Path "$Path" -Name "LimbusLocalize_OTA" -ItemType "directory" -Force
New-Item -Path "$Path/LimbusLocalize_OTA" -Name "Mods" -ItemType "directory" -Force
Copy-Item -Path $Path/LimbusLocalize.dll -Destination $Path/LimbusLocalize_OTA/Mods -Force
New-Item -Path "$Path/LimbusLocalize_OTA/Mods" -Name "Localize" -ItemType "directory" -Force
New-Item -Path "$Path/LimbusLocalize_OTA/Mods/Localize" -Name "Readme" -ItemType "directory" -Force
New-Item -Path "$Path/LimbusLocalize_OTA/Mods/Localize" -Name "CN" -ItemType "directory" -Force
# Copy the changed files to the release directory
$changedFilesList = $changedFiles -split " "
foreach ($file in $changedFilesList) {
    if (Test-Path -Path $file) {
        $destination = "$Path/LimbusLocalize_OTA/Mods/Localize/CN/$file"
        $destination = $destination.Replace("Localize/CN/Localize/CN/", "Localize/CN/")
        $destinationDirectory = Split-Path -Path $destination -Parent
        if (!(Test-Path -Path $destinationDirectory)) {
            New-Item -ItemType Directory -Force -Path $destinationDirectory
        }
        Copy-Item -Path $file -Destination $destination -Force -Recurse
    }
}
$changedFilesList2 = $changedFiles2 -split " "
foreach ($file2 in $changedFilesList2) {
	if(Test-Path $file2){
		Copy-Item -Path $file2 $Path/LimbusLocalize_OTA/Mods/Localize/Readme -Force
    }
}
Set-Location "$Path/LimbusLocalize_OTA"
7z a -t7z "../LimbusLocalize_OTA_$version.7z" "Mods/" -mx=9 -ms
Set-Location "../../"
# ----------- BepinEx -----------
dotnet build src/LimbusLocalize.sln -c BIE
# Full
New-Item -Path "$Path/LimbusLocalize" -Name "BepInEx" -ItemType "directory" -Force
New-Item -Path "$Path/LimbusLocalize/BepInEx" -Name "plugins" -ItemType "directory" -Force
New-Item -Path "$Path/LimbusLocalize/BepInEx/plugins" -Name "LLC" -ItemType "directory" -Force
$BIE_LLC_Path = "$Path/LimbusLocalize/BepInEx/plugins/LLC"
New-Item -Path "$BIE_LLC_Path" -Name "Localize" -ItemType "directory" -Force
Copy-Item -Path Localize/CN $BIE_LLC_Path/Localize -Force -Recurse
Copy-Item -Path Localize/Readme $BIE_LLC_Path/Localize -Force -Recurse
Copy-Item -Path $Path/LimbusLocalize_BIE.dll -Destination $BIE_LLC_Path -Force
Set-Location "$Path/LimbusLocalize"
7z a -t7z "../LimbusLocalize_BIE_$version.7z" "BepInEx/" -mx=9 -ms
Set-Location "../../"
# OTA
New-Item -Path "$Path/LimbusLocalize_OTA" -Name "BepInEx" -ItemType "directory" -Force
New-Item -Path "$Path/LimbusLocalize_OTA/BepInEx" -Name "plugins" -ItemType "directory" -Force
New-Item -Path "$Path/LimbusLocalize_OTA/BepInEx/plugins" -Name "LLC" -ItemType "directory" -Force
$BIE_OTA_LLC_Path = "$Path/LimbusLocalize_OTA/BepInEx/plugins/LLC"
Copy-Item -Path $Path/LimbusLocalize_BIE.dll -Destination $BIE_OTA_LLC_Path -Force
New-Item -Path "$BIE_OTA_LLC_Path" -Name "Localize" -ItemType "directory" -Force
New-Item -Path "$BIE_OTA_LLC_Path/Localize" -Name "Readme" -ItemType "directory" -Force
New-Item -Path "$BIE_OTA_LLC_Path/Localize" -Name "CN" -ItemType "directory" -Force
# Copy the changed files to the release directory
foreach ($file in $changedFilesList) {
    if (Test-Path -Path $file) {
        $destination = "$BIE_OTA_LLC_Path/Localize/CN/$file"
        $destination = $destination.Replace("Localize/CN/Localize/CN/", "Localize/CN/")
        $destinationDirectory = Split-Path -Path $destination -Parent
        if (!(Test-Path -Path $destinationDirectory)) {
            New-Item -ItemType Directory -Force -Path $destinationDirectory
        }
        Copy-Item -Path $file -Destination $destination -Force -Recurse
    }
}
foreach ($file2 in $changedFilesList2) {
	if(Test-Path $file2){
		Copy-Item -Path $file2 $BIE_OTA_LLC_Path/Localize/Readme -Force
    }
}
Set-Location "$Path/LimbusLocalize_OTA"
7z a -t7z "../LimbusLocalize_BIE_OTA_$version.7z" "BepInEx/" -mx=9 -ms
Set-Location "../../"