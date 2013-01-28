@echo off

setlocal enableextensions
set PATH=%~dp0\tools;%PATH%
set BuildTargets=%~dp0\packages\Digillect.Build.Tasks\1.1.1\tools\Build.targets
set EnableNuGetPackageRestore=true

if not exist "%BuildTargets%" (
	nuget.exe install -o "%~dp0\packages" "%~dp0\packages.config"
)

if not errorlevel 1 (
	%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe build.proj %*
)
