
Remove-Item -Recurse -Force bin, obj -ErrorAction SilentlyContinue
Remove-Item *_wpftmp.csproj -Force -ErrorAction SilentlyContinue
dotnet build-server shutdown; dotnet clean; dotnet build
Write-Host "App is running" -ForegroundColor Green
dotnet run