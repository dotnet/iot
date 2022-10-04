@echo off
setlocal

for %%x in (%*) do (
    if "%%x"=="-pack" set _packArg=/p:BuildPackages=true
)

powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0eng\common\Build.ps1""" -restore -build %* %_packArg%"
exit /b %ErrorLevel%
