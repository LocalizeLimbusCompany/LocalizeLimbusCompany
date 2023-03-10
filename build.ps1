# ----------- MelonLoader IL2CPP Interop (net6) -----------
dotnet build src/LimbusLocalize_ml_ilcpp.sln -c Release_ML_Cpp_net6_interop
$Path = "Release"
# ILRepack
lib/ILRepack.exe /target:library /lib:lib/net6 /lib:lib/interop /lib:$Path /internalize /out:$Path/LimbusLocalize_ml_ilcpp.dll $Path/LimbusLocalize_ml_ilcpp.dll $Path/mcs.dll 
# (cleanup and move files)
Remove-Item $Path/LimbusLocalize.deps.json
Remove-Item $Path/Tomlet.dll
Remove-Item $Path/mcs.dll
Remove-Item $Path/Iced.dll
Remove-Item $Path/Il2CppInterop.Common.dll
Remove-Item $Path/Il2CppInterop.Runtime.dll
Remove-Item $Path/Microsoft.Extensions.Logging.Abstractions.dll
New-Item -Path "$Path" -Name "Mods" -ItemType "directory" -Force
Move-Item -Path $Path/LimbusLocalize.dll -Destination $Path/Mods -Force