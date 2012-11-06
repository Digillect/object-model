@echo off

setlocal enableextensions
set PATH=%~dp0\tools;%PATH%
set BuildTargets=%~dp0\packages\Digillect.Build.Tasks\1.0.0\tools\Build.targets
set EnableNuGetPackageRestore=true

if not exist "%BuildTargets%" (
	nuget install -o packages .\packages.config
)

if not errorlevel 1 (
	msbuild build.proj %*
)
