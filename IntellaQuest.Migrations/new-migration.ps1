#Requires -Version 5.1
<#
.SYNOPSIS
    Scaffolds a new versioned, dated SQL migration.

.DESCRIPTION
    Creates Migrations\<Version>\<yyyyMMddHHmm>\ containing:

        <Version>_<Timestamp>[_Name].cs   migration class (Version + Timestamp)
        up.sql                            create script (run by "up", the default)
        down.sql                          remove script (run by "down")

    Example: -Version 0.0.0.1 with the default timestamp on 2026-10-12 20:30 gives

        Migrations\0.0.0.1\202610122030\
            0.0.0.1_202610122030.cs
            up.sql
            down.sql

    The class is discovered automatically by the IntellaQuest.Migrations runner —
    no registration is needed.

.PARAMETER Version
    Version folder, e.g. 0.0.0.1.

.PARAMETER Name
    Optional PascalCase label added to the class/file name (does not affect the
    migration's identity, which is Version + Timestamp).

.PARAMETER Timestamp
    Optional yyyyMMddHHmm timestamp. Defaults to now.

.EXAMPLE
    .\new-migration.ps1 -Version 0.0.0.1 -Name AddOrdersTable

.EXAMPLE
    .\new-migration.cmd 0.0.0.1 AddOrdersTable
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidatePattern('^\d+(\.\d+){1,3}$')]
    [string]$Version,

    [Parameter(Position = 1)]
    [ValidatePattern('^[A-Za-z][A-Za-z0-9]*$')]
    [string]$Name = '',

    [ValidatePattern('^\d{12}$')]
    [string]$Timestamp = (Get-Date -Format 'yyyyMMddHHmm'),

    [string]$Root = $PSScriptRoot
)

$ErrorActionPreference = 'Stop'

$migrationsRoot = Join-Path $Root 'Migrations'
$versionRoot = Join-Path $migrationsRoot $Version
if (-not (Test-Path -LiteralPath $versionRoot)) {
    New-Item -ItemType Directory -Path $versionRoot | Out-Null
}

# Bump by one minute if that timestamp is already taken.
$folder = Join-Path $versionRoot $Timestamp
while (Test-Path -LiteralPath $folder) {
    $Timestamp = ([datetime]::ParseExact($Timestamp, 'yyyyMMddHHmm', $null)).AddMinutes(1).ToString('yyyyMMddHHmm')
    $folder = Join-Path $versionRoot $Timestamp
}

New-Item -ItemType Directory -Path $folder | Out-Null

$safeVersion = $Version -replace '\.', '_'
$suffix = if ($Name) { "_$Name" } else { '' }
$className = "_${safeVersion}_${Timestamp}${suffix}"
$fileName = "${Version}_${Timestamp}${suffix}.cs"

$classContent = @"
namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Migration $Version / $Timestamp.
    /// up.sql   - create
    /// down.sql - remove
    /// </summary>
    public sealed class $className : Migration
    {
        public override string Version => "$Version";

        public override string Timestamp => "$Timestamp";
    }
}
"@

$upContent = @"
-- Migration $Version / $Timestamp  (up = create)
-- Applied by:  dotnet run --project IntellaQuest.Migrations
--
-- Add the CREATE statements below. Separate batches with a line containing GO.

"@

$downContent = @"
-- Migration $Version / $Timestamp  (down = remove)
-- Applied by:  dotnet run --project IntellaQuest.Migrations -- down
--
-- Add the DROP/undo statements below. Separate batches with a line containing GO.

"@

$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText((Join-Path $folder $fileName), $classContent, $utf8)
[System.IO.File]::WriteAllText((Join-Path $folder 'up.sql'), $upContent, $utf8)
[System.IO.File]::WriteAllText((Join-Path $folder 'down.sql'), $downContent, $utf8)

Write-Host ''
Write-Host "Created migration $Version\$Timestamp" -ForegroundColor Green
Write-Host "  $Version\$Timestamp\$fileName"
Write-Host "  $Version\$Timestamp\up.sql"
Write-Host "  $Version\$Timestamp\down.sql"
Write-Host ''
Write-Host 'Next:'
Write-Host '  1. Put the CREATE SQL in up.sql and the DROP/undo SQL in down.sql'
Write-Host '  2. dotnet run --project IntellaQuest.Migrations'
