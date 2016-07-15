@echo off

setlocal enableextensions enabledelayedexpansion

pushd %~dp0

call :ResolveNuGet nuget.exe || exit /b %ERRORLEVEL%

if exist .nuget\packages.config (
	echo Restoring packages from !CD!\.nuget\packages.config
	"%NuGetExe%" restore .nuget\packages.config -PackagesDirectory packages -NonInteractive -Verbosity quiet || exit /b !ERRORLEVEL!
)

%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe build.proj /nologo /v:m /p:NuGetExecutable="%NuGetExe%" /p:NuGetVerbosity=quiet %*

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
