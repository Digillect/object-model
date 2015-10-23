@echo off

setlocal enableextensions
set EnableNuGetPackageRestore=true

pushd %~dp0

call :ResolveNuGet nuget.exe

if errorlevel 1 (
	echo NuGet.Exe was not found either in .nuget subfolder or PATH
	exit /b %ERRORLEVEL%
)

if exist .nuget\packages.config (
	"%NuGetExe%" restore .nuget\packages.config -PackagesDirectory packages -Verbosity quiet -NonInteractive
)

if not errorlevel 1 (
	%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe build.proj /nologo /v:m %*
)

popd
goto :EOF

:ResolveNuGet
set NuGetExe=%~$PATH:1
if not "%NuGetExe%"=="" goto :EOF
if exist %CD%\.nuget\%1 (
	set NuGetExe=%CD%\.nuget\%1
	goto :EOF
)
exit /b 1
