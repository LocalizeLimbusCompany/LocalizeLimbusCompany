param(
    [string]$version
	)
$Path = "Release"
if (Test-Path $Path)
	{
	Remove-Item -Path "$Path" -Recurse
	}
# ----------- BepinEx -----------
dotnet build src/LimbusLocalize.sln -c BIE
# Full
$BIE_LLC_Path = "$Path/LimbusLocalize/BepInEx/plugins/LLC"
New-Item -Path "$BIE_LLC_Path" -Name "Localize" -ItemType "directory" -Force
Copy-Item -Path Localize/CN $BIE_LLC_Path/Localize -Force -Recurse
Copy-Item -Path Localize/Readme $BIE_LLC_Path/Localize -Force -Recurse
Copy-Item -Path $Path/LimbusLocalize_BIE.dll -Destination $BIE_LLC_Path -Force
if ($version)
	{
	 Set-Location "$Path/LimbusLocalize"
	 7z a -t7z "../LimbusLocalize_BIE_$version.7z" "BepInEx/" -mx=9 -ms
	 Set-Location "../../"
	}
# OTA
$tag=$(git describe --tags --abbrev=0)
$changedFiles=$(git diff --name-only HEAD $tag -- Localize/CN/)
$changedFiles2=$(git diff --name-only HEAD $tag -- Localize/Readme/)
$BIE_OTA_LLC_Path = "$Path/LimbusLocalize_OTA/BepInEx/plugins/LLC"
New-Item -Path "$BIE_OTA_LLC_Path" -Name "Localize" -ItemType "directory" -Force
New-Item -Path "$BIE_OTA_LLC_Path/Localize" -Name "Readme" -ItemType "directory" -Force
New-Item -Path "$BIE_OTA_LLC_Path/Localize" -Name "CN" -ItemType "directory" -Force
Copy-Item -Path $Path/LimbusLocalize_BIE.dll -Destination $BIE_OTA_LLC_Path -Force
# Copy the changed files to the release directory
$changedFilesList = $changedFiles -split " "
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
$changedFilesList2 = $changedFiles2 -split " "
foreach ($file2 in $changedFilesList2) {
	if (Test-Path $file2){
		 Copy-Item -Path $file2 $BIE_OTA_LLC_Path/Localize/Readme -Force
		}
	}