# TotalPos API - Documentación de Operaciones Retail

API REST para integración con terminal BBVA PAX usando SDK TotalPos.

## Servidor

```
URL Base: http://localhost:5000
```

## Endpoints Disponibles

### 1. **Inicializar SDK**
```http
POST /api/initialize
```

**Response:**
```json
{
  "success": true,
  "message": "SDK initialized successfully"
}
```

---

### 2. **Health Check**
```http
GET /api/health
```

**Response:**
```json
{
  "status": "healthy",
  "service": "TotalPos API"
}
```

---

### 3. **Venta (Sale)**
```http
POST /api/sale
Content-Type: application/json
```

#### Venta Normal
```json
{
  "Amount": 10.00,
  "Reference": "SALE-001"
}
```

#### Venta con Cashback
```json
{
  "Amount": 50.00,
  "Cashback": 20.00,
  "Reference": "SALE-CASHBACK-001"
}
```

#### Venta con Puntos
```json
{
  "Amount": 30.00,
  "Points": 100.00,
  "Reference": "SALE-POINTS-001"
}
```

#### Venta con Cashback y Puntos
```json
{
  "Amount": 75.00,
  "Cashback": 25.00,
  "Points": 50.00,
  "Reference": "SALE-COMBO-001"
}
```

**Response:**
```json
{
  "codigoRespuesta": "00",
  "leyenda": "APROBADA 623521",
  "autorizacion": "623521",
  "referenciaFinanciera": "513544661211",
  "aprobada": true
}
```

---

### 4. **Devolución (Refund)**
```http
POST /api/refund
Content-Type: application/json
```

**Request:**
```json
{
  "Amount": 15.00,
  "Reference": "REFUND-001",
  "FinancialReference": "513544661211"
}
```

**Parámetros:**
- `Amount` (requerido): Monto de la devolución
- `Reference` (opcional): Referencia del comercio
- `FinancialReference` (requerido): Referencia financiera de la venta original

**Response:**
```json
{
  "codigoRespuesta": "00",
  "leyenda": "APROBADA",
  "autorizacion": "123456",
  "referenciaFinanciera": "513544661212",
  "aprobada": true
}
```

---

### 5. **Consulta de Puntos**
```http
POST /api/consultpoints
```

**Request:** (vacío)
```json
{}
```

**Response:**
```json
{
  "codigoRespuesta": "00",
  "leyenda": "Puntos disponibles: 1500",
  "success": true
}
```

---

### 6. **Cancelación de Venta (sin tarjeta)**
```http
POST /api/cancelsale
Content-Type: application/json
```

**Request:**
```json
{
  "FinancialReference": "513544661211"
}
```

**Parámetros:**
- `FinancialReference` (requerido): Referencia financiera de la venta a cancelar

**Response:**
```json
{
  "codigoRespuesta": "00",
  "leyenda": "CANCELADA",
  "cancelled": true
}
```

---

### 7. **Cancelación de Venta con Tarjeta**
```http
POST /api/cancelsalecard
Content-Type: application/json
```

**Request:**
```json
{
  "Amount": 100.00,
  "FinancialReference": "513544661211"
}
```

**Parámetros:**
- `Amount` (requerido): Monto de la venta original
- `FinancialReference` (requerido): Referencia financiera de la venta a cancelar

**Note:** Requiere lectura de tarjeta en el terminal.

**Response:**
```json
{
  "codigoRespuesta": "00",
  "leyenda": "CANCELADA",
  "cancelled": true
}
```

---

### 8. **Cancelación de Devolución (sin tarjeta)**
```http
POST /api/cancelrefund
Content-Type: application/json
```

**Request:**
```json
{
  "FinancialReference": "513544661212"
}
```

**Parámetros:**
- `FinancialReference` (requerido): Referencia financiera de la devolución a cancelar

**Response:**
```json
{
  "codigoRespuesta": "00",
  "leyenda": "CANCELADA",
  "cancelled": true
}
```

---

### 9. **Cancelación de Devolución con Tarjeta**
```http
POST /api/cancelrefundcard
Content-Type: application/json
```

**Request:**
```json
{
  "Amount": 15.00,
  "FinancialReference": "513544661212"
}
```

**Parámetros:**
- `Amount` (requerido): Monto de la devolución original
- `FinancialReference` (requerido): Referencia financiera de la devolución a cancelar

**Note:** Requiere lectura de tarjeta en el terminal.

**Response:**
```json
{
  "codigoRespuesta": "00",
  "leyenda": "CANCELADA",
  "cancelled": true
}
```

---

### 10. **Carga de Llaves**
```http
POST /api/loadkeys
```

**Request:** (vacío)
```json
{}
```

**Response:**
```json
{
  "success": false,
  "codigoRespuesta": "96",
  "leyenda": "No se encontraron llaves para el Pin Pad",
  "message": "LoadKeys failed: No se encontraron llaves para el Pin Pad"
}
```

**Notes:** 
- Requiere autorización del banco para completarse exitosamente
- **Ejecución Automática:** Se ejecuta automáticamente durante `/api/initialize` si la bandera `llaves=1` en `pinpad.config`
- Si es exitosa (código 00), actualiza automáticamente la bandera a `llaves=0`
- Puede ejecutarse manualmente con este endpoint si es necesario

---

## Códigos de Respuesta

| Código | Descripción |
|--------|-------------|
| 00 | Aprobada |
| 01 | Referir a emisor |
| 05 | No autorizada |
| 14 | Número de tarjeta inválido |
| 51 | Fondos insuficientes |
| 96 | Error en el sistema |

---

## Comandos para Copiar y Pegar

### 1. Health Check
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/health" -Method GET
```

### 2. Inicializar SDK
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/initialize" -Method POST
```

### 3. Venta Normal - 10 pesos
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method POST -ContentType "application/json" -Body '{"Amount":10.00,"Reference":"SALE-001"}'
```

### 4. Venta Normal - 50 pesos
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method POST -ContentType "application/json" -Body '{"Amount":50.00,"Reference":"SALE-002"}'
```

### 5. Venta con Cashback - 100 pesos + 30 cashback
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method POST -ContentType "application/json" -Body '{"Amount":100.00,"Cashback":30.00,"Reference":"SALE-CASHBACK-001"}'
```

### 6. Venta con Puntos - 75 pesos + 150 puntos
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method POST -ContentType "application/json" -Body '{"Amount":75.00,"Points":150.00,"Reference":"SALE-POINTS-001"}'
```

### 7. Venta Combo - 200 pesos + 50 cashback + 100 puntos
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method POST -ContentType "application/json" -Body '{"Amount":200.00,"Cashback":50.00,"Points":100.00,"Reference":"SALE-COMBO-001"}'
```

### 8. Devolución - 20 pesos
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/refund" -Method POST -ContentType "application/json" -Body '{"Amount":20.00,"Reference":"REFUND-001","FinancialReference":"513544661211"}'
```

### 9. Consulta de Puntos
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/consultpoints" -Method POST
```

### 10. Cancelar Venta (sin tarjeta)
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/cancelsale" -Method POST -ContentType "application/json" -Body '{"FinancialReference":"513544661211"}'
```

### 11. Cancelar Venta (con tarjeta)
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/cancelsalecard" -Method POST -ContentType "application/json" -Body '{"Amount":100.00,"FinancialReference":"513544661211"}'
```

### 12. Cancelar Devolución (sin tarjeta)
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/cancelrefund" -Method POST -ContentType "application/json" -Body '{"FinancialReference":"513544661212"}'
```

### 13. Cancelar Devolución (con tarjeta)
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/cancelrefundcard" -Method POST -ContentType "application/json" -Body '{"Amount":20.00,"FinancialReference":"513544661212"}'
```

### 14. Carga de Llaves
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/loadkeys" -Method POST
```

---

## Ejemplos con Variables (para personalizar)

### Venta con monto variable
```powershell
$body = @{ Amount = 10.00; Reference = "SALE-001" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method POST -ContentType "application/json" -Body $body
```

### Venta con Cashback personalizado
```powershell
$body = @{ Amount = 50.00; Cashback = 20.00; Reference = "SALE-002" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method POST -ContentType "application/json" -Body $body
```

### Devolución con monto variable
```powershell
$body = @{ Amount = 15.00; Reference = "REFUND-001" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/refund" -Method POST -ContentType "application/json" -Body $body
```

### Cancelar Venta con datos reales
```powershell
$body = @{ AuthCode = "623521"; TransactionDate = "20260107" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/cancelsale" -Method POST -ContentType "application/json" -Body $body
```

---

## Logs

Todas las transacciones se registran en:
```
logs/transactions_YYYYMMDD.log
```

Formato de log:
```
Timestamp|Type|Amount|Reference|ResponseCode|Message|AuthCode|FinancialRef
2026-01-07 02:41:30|SALE|10.00|VENTA-TEST-003|00|APROBADA 623521|623521|513544661211
```

---

## Scripts de Prueba

### Prueba Rápida (sin transacciones)
```powershell
.\quick_test.ps1
```

### Prueba Completa (con transacciones en terminal)
```powershell
.\test_all_endpoints.ps1
```

---

## Notas Importantes

1. **Inicializar primero:** Ejecutar `/api/initialize` antes de cualquier operación
2. **Carga de Llaves Automática:** Si `llaves=1` en `pinpad.config`, Initialize ejecuta LoadKeys automáticamente
3. **Terminal requerido:** Terminal PAX conectado y encendido
4. **Simulador:** Simulador Java debe estar corriendo en `localhost:8080`
5. **Timeouts:** Las transacciones pueden tardar 30-60 segundos
6. **Carga de Llaves:** Solo funciona con autorización del banco (error 96 es esperado durante pruebas)
7. **Logs:** Revisa `logs/` para historial de transacciones
