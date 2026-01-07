$amount = Read-Host "Enter Amount (e.g. 10.00)"
$url = "http://localhost:5000/api/sale"
$body = @{ amount = $amount } | ConvertTo-Json

Write-Host "`nSending Sale Request for $amount..." -ForegroundColor Cyan
Write-Host "Waiting for card on terminal..." -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri $url -Method Post -Body $body -ContentType "application/json" -ErrorAction Stop
    
    Write-Host "`n================================================" -ForegroundColor Cyan
    Write-Host "TRANSACTION RESPONSE" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan
    $response | Format-List
    
    if ($response.success -and $response.responseCode -eq "00") {
        Write-Host "TRANSACTION APPROVED!" -ForegroundColor Green
        Write-Host "Auth Code: $($response.authCode)" -ForegroundColor Green
    } else {
        Write-Host "TRANSACTION DECLINED/FAILED" -ForegroundColor Red
        Write-Host "Message: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "`nERROR: $($_.Exception.Message)" -ForegroundColor Red
}
