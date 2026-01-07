# TotalPos API

A clean, modular REST API for BBVA TotalPos SDK integration (.NET Framework 4.8).

## Features

- ✅ Clean architecture with separated concerns
- ✅ HTTP REST API using `HttpListener`
- ✅ PostgreSQL database logging via Entity Framework Core
- ✅ Proper SDK initialization and configuration
- ✅ JSON request/response handling

## Project Structure

```
TotalPosApi/
├── Program.cs              # HTTP server & API endpoints
├── TotalPosService.cs      # SDK wrapper & business logic
├── Models/
│   ├── AppDbContext.cs     # EF Core database context
│   └── TransactionLog.cs   # Transaction log entity
└── TotalPosApi.csproj      # Project configuration
```

## Configuration

Edit the constants in [Program.cs](Program.cs):

```csharp
private const string ConnectionType = "USB";  // or "COM3", "Serial"
private const string Afiliacion = "0000001";
private const string Terminal = "00000001";
private const int Port = 5000;
```

Database connection string in [Models/AppDbContext.cs](Models/AppDbContext.cs):

```csharp
optionsBuilder.UseNpgsql("Host=localhost;Database=totalpos_retail;Username=admin;Password=password123");
```

## API Endpoints

### POST /api/sale
Process a sale transaction.

**Request:**
```json
{
  "amount": 10.00,
  "reference": "ORDER-123"
}
```

**Response:**
```json
{
  "codigoRespuesta": "00",
  "leyenda": "APROBADA",
  "autorizacion": "123456",
  "referenciaFinanciera": "REF123",
  "aprobada": true
}
```

### GET /api/transactions
Get last 50 transactions from database.

### GET /api/health
Health check endpoint.

## Running the API

```powershell
# Build
dotnet build

# Run
dotnet run
```

The API will start on `http://localhost:5000`

## Testing

```powershell
# Health check
Invoke-WebRequest http://localhost:5000/api/health

# Test sale
Invoke-RestMethod -Method Post -Uri http://localhost:5000/api/sale `
  -ContentType "application/json" `
  -Body '{"amount":10.50,"reference":"TEST-001"}'
```

## Dependencies

- .NET Framework 4.8
- EGlobal.TotalPosSDKNet (libs folder)
- Npgsql.EntityFrameworkCore.PostgreSQL 3.1.18
- Newtonsoft.Json 13.0.3

## Notes

- Platform target: x86 (required by EGlobal SDK)
- Requires config files in output directory (Local.config, pinpad.config, etc.)
- SDK automatically looks for configuration files in execution directory
