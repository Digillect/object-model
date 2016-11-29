@echo off

setlocal enableextensions enabledelayedexpansion

pushd %~dp0

call :ResolveNuGet nuget.exe || exit /b %ERRORLEVEL%

call :FindMSBuild 14.0

if errorlevel 1 (
	echo Could not find any applicable MSBuild version
	exit /b %ERRORLEVEL%
)

if exist .nuget\packages.config (
	echo Restoring packages from !CD!\.nuget\packages.config
	"%NuGetExe%" restore .nuget\packages.config -PackagesDirectory packages -NonInteractive -Verbosity quiet || exit /b !ERRORLEVEL!
)

"%MSBUILDDIR%MSBuild.exe" build.proj /nologo /v:m /p:NuGetExecutable="%NuGetExe%" /p:NuGetVerbosity=quiet %*

popd
goto :EOF

:ResolveNuGet
if exist %CD%\.nuget\%1 (
	set NuGetExe=!CD!\.nuget\%1
	goto :EOF
)
set NuGetExe=%~$PATH:1
if not "%NuGetExe%"=="" goto :EOF
echo NuGet.Exe was not found either in .nuget subfolder or PATH
exit /b 1

:FindMSBuild
rem http://stackoverflow.com/a/20431996/393672
reg.exe query "HKLM\SOFTWARE\WOW6432Node\Microsoft\MSBuild\ToolsVersions\%1" /v MSBuildToolsPath > nul 2>&1
if errorlevel 1 exit /b %ERRORLEVEL%
for /f "skip=2 tokens=2,*" %%A in ('reg.exe query "HKLM\SOFTWARE\WOW6432Node\Microsoft\MSBuild\ToolsVersions\%1" /v MSBuildToolsPath') do SET MSBUILDDIR=%%B
if exist "%MSBUILDDIR%MSBuild.exe" goto :EOF
exit /b 1
