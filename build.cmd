@echo off

for /d %%F in ("%ProgramFiles(x86)%\Microsoft Visual Studio\2017\*") do (
 set MSBuild="%%F\MSBuild\15.0\Bin\MSBuild.exe"
 goto validate
)

:validate

if exist %MSBuild% goto build
echo ERROR: You need Visual Studio 2017 to build.
exit

:build

if not exist bin mkdir bin

:: Note: We've disabled node reuse because it causes file locking issues.
::       The issue is that we extend the build with our own targets which
::       means that that rebuilding cannot successfully delete the task
::       assembly.

%MSBuild% /nologo /m /v:m /nr:false /flp:verbosity=normal;LogFile=bin\msbuild.log %*
