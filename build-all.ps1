# ============================
# Cross-platform publish script
# ============================

Write-Host "Publishing Windows x64..."
dotnet publish BirthdayCollator.Server/BirthdayCollator.Server.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -o publish-win-x64

Write-Host "Publishing macOS ARM64 (Apple Silicon)..."
dotnet publish BirthdayCollator.Server/BirthdayCollator.Server.csproj `
    -c Release `
    -r osx-arm64 `
    --self-contained true `
    -o publish-mac-arm64

Write-Host "Publishing macOS x64 (Intel Macs)..."
dotnet publish BirthdayCollator.Server/BirthdayCollator.Server.csproj `
    -c Release `
    -r osx-x64 `
    --self-contained true `
    -o publish-mac-x64

Write-Host "All builds completed successfully."
