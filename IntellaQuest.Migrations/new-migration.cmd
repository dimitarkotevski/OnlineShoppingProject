@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0new-migration.ps1" %*
