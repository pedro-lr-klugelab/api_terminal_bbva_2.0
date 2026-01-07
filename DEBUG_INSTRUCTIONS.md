# Instructions for Testing with Debug Logging

## 1. Start the API Server
```powershell
.\bin\Debug\net48\SimpleConnectionTest.exe --api
```

## 2. Initialize Terminal (in another PowerShell window)
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/initialize" -Method Post
```

## 3. Test with Importe Parameter
```powershell
$body = @{ Importe = "10.00" } | ConvertTo-Json
Write-Host "Sending JSON: $body"
Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method Post -ContentType "application/json" -Body $body
```

## What to Look For

In the API server console, you should see:
- `[DEBUG] Received JSON Body: ...` - Shows exact JSON received
- `[DEBUG] Parsed Importe value: '...'` - Shows parsed amount
- `[DEBUG] Adding to SDK parameters - Importe: '...'` - Shows value being passed to SDK

This will help us identify where the issue occurs.
