# Test Sale with Debug Logging

# Show what JSON PowerShell generates
Write-Host "=== JSON being sent ===" -ForegroundColor Cyan
$body = @{ Importe = "10.00" } | ConvertTo-Json
Write-Host $body -ForegroundColor Yellow

# Send the request
Write-Host "`n=== Sending request ===" -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method Post -ContentType "application/json" -Body $body
    Write-Host "`n=== Response ===" -ForegroundColor Green
    $response | Format-List
} catch {
    Write-Host "`n=== Error ===" -ForegroundColor Red
    Write-Host $_.Exception.Message
}

Write-Host "`nCheck the API server console for [DEBUG] messages" -ForegroundColor Cyan
