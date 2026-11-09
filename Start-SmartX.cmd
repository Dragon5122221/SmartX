@echo off
setlocal
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 (
  echo Install the .NET 10 SDK, then run this file again.
  pause
  exit /b 1
)
dotnet build SmartX.slnx -m:1 --nologo
if errorlevel 1 (
  echo Build failed. Read the errors above before starting the application.
  pause
  exit /b 1
)
set SmartX__AllowHttp=true
set SmartXApi__BaseUrl=http://localhost:5267/
start "Smart-X API - keep open" cmd /k "dotnet run --project SmartXWebApi --no-build --no-launch-profile --urls http://localhost:5267"
start "Smart-X Client - keep open" cmd /k "dotnet run --project SmartXMVC --no-build --no-launch-profile --urls http://localhost:5080"
echo Open http://localhost:5080 when both windows report Now listening.
echo Close both server windows when finished.
pause
