@echo off
setlocal enabledelayedexpansion
cd %~dp0

for %%f in (*) do (
  set "filename=%%~nf"
  set "extension=%%~xf"
  set "newname=!filename:KR=CN!!extension!"
  if not "!newname!"=="%%~nxf" (
    ren "%%~f" "!newname!"
  )
)

echo All files renamed.