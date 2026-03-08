param(
    [string]$Extension = "*.ts,*.tsx,*.js,*.jsx",
    [int]$Top = 50
)

$clientRoot = Join-Path (Get-Location).Path "BirthdayCollator.Client"

Get-ChildItem -Path $clientRoot -Recurse -Include $Extension.Split(",") |
    Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\|\\dist\\|\\node_modules\\" } |
    ForEach-Object {
        $count = (Get-Content $_.FullName).Count
        $relative = $_.FullName.Replace($clientRoot, "").TrimStart("\\")

        [PSCustomObject]@{
            File  = $relative
            Lines = $count
        }
    } |
    Sort-Object Lines -Descending |
    Select-Object -First $Top
