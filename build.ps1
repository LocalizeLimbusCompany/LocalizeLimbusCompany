param(
    [string]$version
)
# ----------- MelonLoader IL2CPP Interop (net6) -----------
dotnet build src/LimbusLocalize_ml_ilcpp.sln -c Release_ML_Cpp_net6_interop
$Path = "Release"
# (cleanup and move files)
Remove-Item $Path/LimbusLocalize.deps.json
Remove-Item $Path/Tomlet.dll
Remove-Item $Path/Iced.dll
Remove-Item $Path/Il2CppInterop.Common.dll
Remove-Item $Path/Il2CppInterop.Runtime.dll
Remove-Item $Path/Microsoft.Extensions.Logging.Abstractions.dll
# Full
New-Item -Path "$Path" -Name "LimbusLocalize" -ItemType "directory" -Force
New-Item -Path "$Path/LimbusLocalize" -Name "Mods" -ItemType "directory" -Force
New-Item -Path "$Path/LimbusLocalize/Mods" -Name "Localize" -ItemType "directory" -Force
Copy-Item -Path assets/Localize/CN $Path/LimbusLocalize/Mods/Localize -Force -Recurse
Copy-Item -Path assets/Localize/Readme $Path/LimbusLocalize/Mods/Localize -Force -Recurse
Copy-Item -Path $Path/LimbusLocalize.dll -Destination $Path/LimbusLocalize/Mods -Force
7z a -t7z "$Path/LimbusLocalize_$version.7z" "./$Path/LimbusLocalize/*" -mx=9 -ms
$tag=$(git describe --tags --abbrev=0)
$changedFiles=$(git diff --name-only HEAD $tag -- assets/Localize/CN/)
$changedFiles2=$(git diff --name-only HEAD $tag -- assets/Localize/Readme/)
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
        $destination = $destination.Replace("assets/Localize/CN/", "")
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
7z a -t7z "$Path/LimbusLocalize_OTA_$version.7z" "./$Path/LimbusLocalize_OTA/*" -mx=9 -ms