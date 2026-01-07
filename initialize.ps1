$url = "http://localhost:5000/api/initialize"
Write-Host "Initializing Terminal Connection..." -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri $url -Method Post -ErrorAction Stop
    Write-Host "`nResponse:" -ForegroundColor Yellow
    $response | Format-List
    
    if ($response.success) {
        Write-Host "`nSUCCESS: Terminal is ready!" -ForegroundColor Green
    } else {
        Write-Host "`nFAILED: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "`nERROR: Unable to reach API. Is the server running?" -ForegroundColor Red
    Write-Host "Start the server with: .\bin\Debug\net48\SimpleConnectionTest.exe --api" -ForegroundColor Yellow
}
