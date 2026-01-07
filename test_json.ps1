# Test JSON generation from PowerShell
Write-Host "=== Testing JSON Generation ===" -ForegroundColor Cyan

# Test 1: Using Importe
$body1 = @{ Importe = "10.00" } | ConvertTo-Json
Write-Host "`nTest 1 - Using Importe:" -ForegroundColor Yellow
Write-Host $body1

# Test 2: Using amount
$body2 = @{ amount = "10.00" } | ConvertTo-Json
Write-Host "`nTest 2 - Using amount:" -ForegroundColor Yellow
Write-Host $body2

# Show byte representation
Write-Host "`nByte representation of 'Importe' JSON:" -ForegroundColor Yellow
[System.Text.Encoding]::UTF8.GetBytes($body1) | ForEach-Object { Write-Host -NoNewline "$_ " }
Write-Host ""
