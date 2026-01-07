$url = "http://localhost:5000/api/terminal/status"
Write-Host "Checking API Status..."
try {
    $response = Invoke-RestMethod -Uri $url -Method Get -ErrorAction Stop
    Write-Host "API is Running!"
    $response | Format-List
} catch {
    Write-Host "API is NOT reachable. Is the server running?"
    Write-Host "Error: $_"
}
