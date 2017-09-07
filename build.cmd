@echo off

setlocal enableextensions enabledelayedexpansion
set EnableNuGetPackageRestore=true
set DOTNET_CLI_TELEMETRY_OPTOUT=1

pushd %~dp0

call :ResolveNuGet NuGet.exe || exit /b !ERRORLEVEL!

call :FindMSBuild 14 || (
	echo Could not find any applicable MSBuild version
	exit /b !ERRORLEVEL!
)

if exist .nuget\packages.config (
	echo Restoring packages from !CD!\.nuget\packages.config
	"%NuGetExe%" restore .nuget\packages.config -OutputDirectory packages -NonInteractive -Verbosity quiet || exit /b !ERRORLEVEL!
)

"%MSBUILDDIR%\MSBuild.exe" build.proj /nologo /v:m /p:SolutionDir="%~dp0\" /p:NuGetExecutable="%NuGetExe%" /p:NuGetVerbosity=quiet %*

popd
goto :EOF

:ResolveNuGet filename
if exist "%CD%\.nuget\%1" (
	set NuGetExe=!CD!\.nuget\%1
	goto :EOF
)
set NuGetExe=%~$PATH:1
if not "%NuGetExe%"=="" goto :EOF
echo %1 was not found either in .nuget subfolder or PATH
exit /b 1

:: http://stackoverflow.com/a/20431996/393672
:FindMSBuild
if "%1"=="" exit /b 1
reg.exe query "HKLM\SOFTWARE\WOW6432Node\Microsoft\MSBuild\ToolsVersions\%1.0" /v MSBuildToolsPath > nul 2>&1
if not errorlevel 1 (
	for /f "skip=2 tokens=2,*" %%A in ('reg.exe query "HKLM\SOFTWARE\WOW6432Node\Microsoft\MSBuild\ToolsVersions\%1.0" /v MSBuildToolsPath') do (
		if exist "%%BMSBuild.exe" (
			set MSBUILDDIR=%%B
			set MSBUILDDIR=!MSBUILDDIR:~0,-1!
			set MSBUILDVER=%1
			goto :EOF
		)
	)
)
shift
goto :FindMSBuild
