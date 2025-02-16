param(
    [string]$version
	)
$Path = "Release"
if (Test-Path $Path)
	{
	Remove-Item -Path "$Path" -Recurse
	}
# ----------- BepinEx -----------
dotnet build LimbusLocalize.sln -c BIE
$BIE_LLC_Path = "$Path/BepInEx/plugins/LLC"
New-Item -Path "$BIE_LLC_Path" -Name "Localize" -ItemType "directory" -Force
Copy-Item -Path Localize/CN $BIE_LLC_Path/Localize -Force -Recurse
Copy-Item -Path Localize/Readme $BIE_LLC_Path/Localize -Force -Recurse
Copy-Item -Path .\TitleBgm.mp3 $BIE_LLC_Path/Localize -Force -Recurse
Copy-Item -Path .\lyrics.json $BIE_LLC_Path/Localize -Force -Recurse
if ($version)
	{
	 Set-Location "$Path"
	 ..\Patcher\7z.exe a -t7z "./LimbusLocalize_BIE_$version.7z" "BepInEx/" -mx=9 -ms
	}