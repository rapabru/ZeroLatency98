@echo off
cd /d "%~dp0"
if exist "ZeroLatency98.exe" (
    start "" "ZeroLatency98.exe"
) else if exist "src\DesktopPerformance\bin\Release\net9.0-windows\win-x64\publish\ZeroLatency98.exe" (
    start "" "src\DesktopPerformance\bin\Release\net9.0-windows\win-x64\publish\ZeroLatency98.exe"
) else if exist "src\DesktopPerformance\bin\Release\net9.0-windows\win-x64\publish\DesktopPerformance.exe" (
    start "" "src\DesktopPerformance\bin\Release\net9.0-windows\win-x64\publish\DesktopPerformance.exe"
) else (
    dotnet run --project src\DesktopPerformance -c Release
)
