$ErrorActionPreference='Stop'
Set-Location $PSScriptRoot
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { Write-Host 'Установите .NET 8 SDK'; exit 1 }
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false -o .\release
Write-Host "`nГОТОВО: $PSScriptRoot\release\LOVINGCOOL Studio.exe" -ForegroundColor Green