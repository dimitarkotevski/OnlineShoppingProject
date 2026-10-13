#Requires -Version 5.1
<#
.SYNOPSIS
    Scaffolds a new folder-based SQL migration.

.DESCRIPTION
    Creates Migrations\<yyyyMMddHHmmss>_<Name>\ containing:

        <yyyyMMddHHmmss>_<Name>.cs   migration class (Id + Name)
        up.sql                       forward script  (run by "up", the default)
        down.sql                     rollback script (run by "down")

    The class is discovered automatically by the IntellaQuest.Migrations runner —
    no registration is needed.

.PARAMETER Name
    PascalCase migration name, e.g. AddOrdersTable.

.PARAMETER Root
    Project folder to scaffold into. Defaults to the folder holding this script.

.EXAMPLE
    .\new-migration.ps1 AddOrdersTable

.EXAMPLE
    .\new-migration.cmd AddOrdersTable
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidatePattern('^[A-Za-z][A-Za-z0-9]*$')]
    [string]$Name,

    [string]$Root = $PSScriptRoot
)

$ErrorActionPreference = 'Stop'

$migrationsRoot = Join-Path $Root 'Migrations'
if (-not (Test-Path -LiteralPath $migrationsRoot)) {
    New-Item -ItemType Directory -Path $migrationsRoot | Out-Null
}

# Dated id; bump by one second if two migrations are scaffolded in the same second.
$id = [long](Get-Date -Format 'yyyyMMddHHmmss')
$folderName = "${id}_$Name"
$folder = Join-Path $migrationsRoot $folderName
while (Test-Path -LiteralPath $folder) {
    $id = $id + 1
    $folderName = "${id}_$Name"
    $folder = Join-Path $migrationsRoot $folderName
}

New-Item -ItemType Directory -Path $folder | Out-Null

$className = "_${id}_$Name"

$classContent = @"
namespace IntellaQuest.Migrations
{
    /// <summary>
    /// Migration $folderName.
    /// Forward script:  up.sql
    /// Rollback script: down.sql
    /// </summary>
    public sealed class $className : Migration
    {
        public override long Id => $id;

        public override string Name => "$Name";
    }
}
"@

$upContent = @"
-- Migration $folderName  (forward)
-- Applied by:  dotnet run --project IntellaQuest.Migrations
--
-- Add the forward DDL/DML below. Separate batches with a line containing GO.

"@

$downContent = @"
-- Migration $folderName  (rollback)
-- Applied by:  dotnet run --project IntellaQuest.Migrations -- down
--
-- Undo everything up.sql does. Separate batches with a line containing GO.

"@

$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText((Join-Path $folder "$className.cs"), $classContent, $utf8)
[System.IO.File]::WriteAllText((Join-Path $folder 'up.sql'), $upContent, $utf8)
[System.IO.File]::WriteAllText((Join-Path $folder 'down.sql'), $downContent, $utf8)

Write-Host ''
Write-Host "Created migration $folderName" -ForegroundColor Green
Write-Host "  $folderName\$className.cs"
Write-Host "  $folderName\up.sql"
Write-Host "  $folderName\down.sql"
Write-Host ''
Write-Host 'Next:'
Write-Host '  1. Put the forward SQL in up.sql and the rollback SQL in down.sql'
Write-Host '  2. dotnet run --project IntellaQuest.Migrations'
