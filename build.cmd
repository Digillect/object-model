@echo off

setlocal enableextensions
set PATH=%~dp0\tools;%PATH%
set BuildTargets=%~dp0\packages\Digillect.Build.Tasks\1.0.0\tools\Build.targets
set EnableNuGetPackageRestore=true

if exist %BuildTargets% goto :build

@nuget install -o packages .\packages.config
if %ERRORLEVEL% NEQ 0 goto :done

:build
@msbuild build.proj %*

:done
