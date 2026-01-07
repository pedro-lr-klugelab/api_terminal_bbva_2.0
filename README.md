# API Terminal BBVA PAX SP30

API REST independiente para procesar pagos con la terminal BBVA PAX SP30. 

## Requisitos Previos

### Software Necesario

✅ **Windows 10 u 11**
- El API está diseñado para Windows

✅ **.NET Framework 4.8**
- Verifica si ya lo tienes instalado:
  ```powershell
  Get-ChildItem 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\' | Get-ItemPropertyValue -Name Release
  ```
- Si el número es 528040 o mayor, ya tienes .NET 4.8
- Si no, descarga desde: https://dotnet.microsoft.com/download/dotnet-framework/net48

✅ **PowerShell 5.1 o superior** (incluido en Windows 10/11)
- Verifica tu versión:
  ```powershell
  $PSVersionTable.PSVersion
  ```

### Hardware Necesario

**Opción 1: Terminal Física (Producción)**
- Terminal BBVA PAX SP30
- Cable USB (incluido con la terminal)
- Puerto USB disponible en la computadora

**Opción 2: Simulador (Desarrollo/Pruebas)**
- Java Runtime Environment (JRE) 8 o superior
- Archivos del simulador BBVA (proporcionados por BBVA)

---

## Inicio Rápido

### 1. Conecta la Terminal
- Conecta la terminal PAX SP30 por USB
- La API detectará automáticamente el puerto COM

### 2. Inicia el Servidor
```powershell
.\bin\Debug\net48\SimpleConnectionTest.exe --api
```

El servidor iniciará en `http://localhost:5000/`

Verás un mensaje como:
```
[SUCCESS] Detected PAX terminal on COM8
[INFO] Model: PXSP30, SN: PX00000000003L196146
```

### 3. Listo para Usar
El API está listo. Ahora puedes procesar pagos desde PowerShell, aplicaciones web, o cualquier cliente HTTP.

---

## Usar el Simulador (Para Desarrollo sin Terminal Física)

Si no tienes una terminal PAX SP30 física, puedes usar el **Simulador BBVA** para desarrollo y pruebas.

### Paso 1: Requisitos del Simulador

✅ **Java Runtime Environment (JRE) 8 o superior**

Verifica si tienes Java instalado:
```powershell
java -version
```

Si no está instalado, descarga desde: https://www.java.com/download/

✅ **Archivos del Simulador BBVA**

El simulador debe estar en la carpeta:
```
Simulador Version_4.3.0\EntregaSimulador\Servidor\
```

### Paso 2: Iniciar el Simulador

1. **Abre PowerShell** y navega a la carpeta del simulador:
   ```powershell
   cd "Simulador Version_4.3.0\EntregaSimulador\Servidor"
   ```

2. **Ejecuta el simulador:**
   ```powershell
   java -jar main\webapp\WEB-INF\views\Simulador-1.0.0-SNAPSHOT.jar
   ```

3. **Espera a que inicie** - Verás un mensaje como:
   ```
   Tomcat started on port(s): 8080
   Started SimuladorApplication
   ```

4. **Verifica que funcione** abriendo en el navegador:
   ```
   http://localhost:8080
   ```

### Paso 3: Configurar el API para Usar el Simulador

El API se configurará automáticamente para usar el simulador si:
- El simulador está corriendo en `localhost:8080`
- No hay una terminal física conectada

**No necesitas cambiar ninguna configuración** - el API detecta automáticamente el simulador.

### Paso 4: Probar con el Simulador

Una vez que tanto el **simulador** como el **API** estén corriendo:

```powershell
# Inicializar
Invoke-RestMethod -Uri "http://localhost:5000/api/initialize" -Method Post

# Procesar venta de prueba
$pago = @{ Importe = "10.00" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method Post -ContentType "application/json" -Body $pago
```

### Tarjetas de Prueba

El simulador incluye miles de tarjetas de prueba en el archivo `bines_*.config`. Algunos ejemplos:

| Tipo | BIN Ejemplo | Banco |
|------|-------------|-------|
| Visa Débito | 4152313XXXXXXXXX | Bancomer |
| Mastercard Crédito | 5256481XXXXXXXXX | Santander |
| American Express | 374101XXXXXXXXXX | American Express |

> 💡 **Tip:** Revisa el archivo `bines_*.config` en la carpeta del simulador para ver todas las tarjetas de prueba disponibles.

### Cerrar el Simulador

Para detener el simulador, presiona `Ctrl+C` en la ventana de PowerShell donde está corriendo.

---

## Ejemplos de Uso con PowerShell

### Flujo Completo: Inicializar y Procesar un Pago

```powershell
# 1. Verificar que el API esté funcionando
Invoke-RestMethod -Uri "http://localhost:5000/api/status" -Method Get

# 2. Inicializar la terminal
Invoke-RestMethod -Uri "http://localhost:5000/api/initialize" -Method Post

# 3. Procesar un pago de $50.00
$venta = @{ Importe = "50.00" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method Post -ContentType "application/json" -Body $venta
```

### Casos de Uso Prácticos

#### ✅ Cobrar $10 pesos
```powershell
$pago = @{ Importe = "10.00" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method Post -ContentType "application/json" -Body $pago
```

#### ✅ Cobrar $250.50 pesos
```powershell
$pago = @{ Importe = "250.50" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method Post -ContentType "application/json" -Body $pago
```

#### ✅ Cobrar un monto variable (desde tu aplicación)
```powershell
# Ejemplo: cobrar el total de una venta
$totalVenta = 1587.90
$pago = @{ Importe = $totalVenta.ToString("F2") } | ConvertTo-Json
$respuesta = Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method Post -ContentType "application/json" -Body $pago

# Verificar si fue aprobado
if ($respuesta.success) {
    Write-Host "✓ Pago aprobado - Código: $($respuesta.authCode)"
} else {
    Write-Host "✗ Pago rechazado - $($respuesta.message)"
}
```

#### ✅ Procesar múltiples pagos
```powershell
# Lista de pagos a procesar
$pagos = @("45.00", "120.50", "89.99", "200.00")

foreach ($monto in $pagos) {
    Write-Host "`nProcesando pago de $$monto..."
    $body = @{ Importe = $monto } | ConvertTo-Json
    $resultado = Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method Post -ContentType "application/json" -Body $body
    
    if ($resultado.success) {
        Write-Host "✓ Aprobado - Auth: $($resultado.authCode)"
    } else {
        Write-Host "✗ Rechazado"
    }
    
    Start-Sleep -Seconds 2  # Esperar entre transacciones
}
```

#### ✅ Integración con sistema de punto de venta
```powershell
function Procesar-Pago {
    param(
        [decimal]$Monto,
        [string]$NumeroTicket
    )
    
    $body = @{ 
        Importe = $Monto.ToString("F2")
    } | ConvertTo-Json
    
    try {
        $respuesta = Invoke-RestMethod -Uri "http://localhost:5000/api/sale" `
                                       -Method Post `
                                       -ContentType "application/json" `
                                       -Body $body
        
        return @{
            Exito = $respuesta.success
            CodigoAutorizacion = $respuesta.authCode
            Mensaje = $respuesta.message
            Ticket = $NumeroTicket
        }
    }
    catch {
        return @{
            Exito = $false
            Mensaje = "Error de conexión: $($_.Exception.Message)"
        }
    }
}

# Usar la función
$resultado = Procesar-Pago -Monto 155.00 -NumeroTicket "VENTA-001"
if ($resultado.Exito) {
    Write-Host "Pago autorizado: $($resultado.CodigoAutorizacion)"
} else {
    Write-Host "Error: $($resultado.Mensaje)"
}
```

---

## Endpoints del API

### `GET /api/status`
Verifica si el API está funcionando y si la terminal está inicializada.

**Respuesta:**
```json
{
  "status": "running",
  "initialized": true
}
```

**Ejemplo:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/status" -Method Get
```

---

### `POST /api/initialize`
Inicializa la conexión con la terminal. **Debes llamar esto una vez al iniciar el API**.

**Respuesta:**
```json
{
  "success": true,
  "message": "Terminal Initialized"
}
```

**Ejemplo:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/initialize" -Method Post
```

---

### `POST /api/sale`
Procesa una transacción de venta.

**Parámetros:**
| Campo | Tipo | Descripción | Ejemplo |
|-------|------|-------------|---------|
| `Importe` | string | Monto a cobrar en pesos | `"125.50"` |

**Petición:**
```json
{
  "Importe": "125.50"
}
```

**Respuesta Exitosa:**
```json
{
  "success": true,
  "message": "APROBADA 123456",
  "authCode": "123456",
  "responseCode": "00"
}
```

**Respuesta Rechazada:**
```json
{
  "success": false,
  "message": "RECHAZADA - Fondos insuficientes",
  "authCode": "",
  "responseCode": "51"
}
```

**Ejemplo:**
```powershell
$pago = @{ Importe = "125.50" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method Post -ContentType "application/json" -Body $pago
```

---

## Respuestas del Sistema

### Códigos de Respuesta Comunes

| Código | Significado |
|--------|-------------|
| `00` | Transacción aprobada |
| `51` | Fondos insuficientes |
| `05` | Transacción rechazada |
| `91` | Terminal no disponible |

### Manejo de Errores

```powershell
try {
    $pago = @{ Importe = "100.00" } | ConvertTo-Json
    $resultado = Invoke-RestMethod -Uri "http://localhost:5000/api/sale" `
                                   -Method Post `
                                   -ContentType "application/json" `
                                   -Body $pago
    
    if ($resultado.success) {
        Write-Host "✓ Pago aprobado"
        Write-Host "  Autorización: $($resultado.authCode)"
    } else {
        Write-Host "✗ Pago rechazado"
        Write-Host "  Razón: $($resultado.message)"
    }
}
catch {
    Write-Host "⚠ Error de comunicación con el API"
    Write-Host "  Detalles: $($_.Exception.Message)"
}
```

---

## Características

✅ **Detección Automática de Puerto COM**
- No necesitas configurar el puerto manualmente
- Funciona en cualquier computadora

✅ **Validación de Terminal**
- Solo se conecta a terminales PAX auténticas
- Mensajes claros si la terminal no está conectada

✅ **API REST Estándar**
- Compatible con cualquier lenguaje de programación
- Fácil de integrar en aplicaciones web, móviles o de escritorio

✅ **Portable y Autónomo**
- No requiere instalación
- Copia la carpeta y ejecuta

---

## Requisitos

- **Sistema Operativo:** Windows
- **.NET Framework:** 4.8 (incluido en Windows 10/11)
- **Terminal:** PAX SP30 conectada por USB
- **Simulador (opcional):** Para pruebas sin terminal física

---

## Solución de Problemas

### ❌ Error: "No USB Serial devices detected"
**Causa:** La terminal no está conectada o no está encendida.

**Solución:**
1. Conecta la terminal PAX SP30 por USB
2. Asegúrate de que esté encendida
3. Reinicia el servidor API

### ❌ Error: "Terminal Not Initialized"
**Causa:** No se llamó el endpoint `/api/initialize`.

**Solución:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/initialize" -Method Post
```

### ❌ Error: "Found N USB Serial device(s) but none responded"
**Causa:** Hay dispositivos USB Serial conectados pero ninguno es una terminal PAX.

**Solución:**
1. Desconecta otros dispositivos USB Serial
2. Verifica que la terminal PAX esté encendida
3. Reinicia el servidor API

---

## Scripts Incluidos

### `initialize.ps1`
Inicializa la terminal automáticamente.
```powershell
powershell -ExecutionPolicy Bypass -File .\initialize.ps1
```

### `sale_test.ps1`
Procesa una venta de prueba.
```powershell
powershell -ExecutionPolicy Bypass -File .\sale_test.ps1
```

---

## Modo Consola

También puedes usar el modo consola interactivo (sin API):

```powershell
.\bin\Debug\net48\SimpleConnectionTest.exe
```

Esto abrirá un menú con opciones para:
1. Inicializar SDK y probar conexión
2. Cargar llaves (obligatorio para terminales nuevas)
3. Probar venta

---

## Despliegue

Para desplegar en otra computadora:

1. **Copia la carpeta completa** a la ubicación deseada
2. **Conecta la terminal** PAX SP30 por USB  
3. **Ejecuta el servidor:**
   ```powershell
   .\bin\Debug\net48\SimpleConnectionTest.exe --api
   ```
4. **El API estará disponible** en `http://localhost:5000/`

---

## Soporte

Para más información sobre el SDK de BBVA o configuración avanzada, consulta la documentación oficial de BBVA o contacta a soporte técnico.
