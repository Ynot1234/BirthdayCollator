param(
    [int]$Top = 50
)

# Find the first .csproj under the current directory
$proj = Get-ChildItem -Recurse -Filter *.csproj | Select-Object -First 1
if (-not $proj) {
    Write-Error "No .csproj file found."
    exit
}

[xml]$xml = Get-Content $proj.FullName
$projDir = $proj.DirectoryName

# 1. All .cs files under the project directory (excluding bin/obj)
$allFiles = Get-ChildItem -Recurse -Path $projDir -Filter *.cs |
    Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\" }

# 2. Collect all <Compile Remove="..."> patterns
$removePatterns = $xml.Project.ItemGroup.Compile |
    Where-Object { $_.Remove } |
    ForEach-Object { $_.Remove }

# 3. Expand Remove patterns into actual files
$removedFiles = @()

foreach ($pattern in $removePatterns) {
    # Convert project-relative pattern to full path
    $fullPattern = Join-Path $projDir $pattern

    # Expand wildcards properly
    $removedFiles += Get-ChildItem -Recurse -Path $projDir -Filter *.cs |
        Where-Object { $_.FullName -like $fullPattern }
}

# Normalize removed file paths
$removedFullNames = $removedFiles |
    Select-Object -ExpandProperty FullName -Unique

# 4. Final set: all files minus removed ones
$finalFiles = $allFiles |
    Where-Object { $removedFullNames -notcontains $_.FullName }

# 5. Count lines
$results = $finalFiles | ForEach-Object {
    $count = (Get-Content $_.FullName).Count
    $relative = $_.FullName.Replace((Get-Location).Path, "").TrimStart("\\")

    [PSCustomObject]@{
        File  = $relative
        Lines = $count
    }
}

$results |
    Sort-Object Lines -Descending |
    Select-Object -First $Top
