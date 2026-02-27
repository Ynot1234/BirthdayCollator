param(
    [string]$Extension = "*.cs",
    [int]$Top = 20
)

$root = (Get-Location).Path

Get-ChildItem -Recurse -Include $Extension |
    Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\" } |
    ForEach-Object {
        $count = (Get-Content $_.FullName).Count
        $relative = $_.FullName.Replace($root, "").TrimStart("\\")

        [PSCustomObject]@{
            File  = $relative
            Lines = $count
        }
    } |
    Sort-Object Lines -Descending |
    Select-Object -First $Top
