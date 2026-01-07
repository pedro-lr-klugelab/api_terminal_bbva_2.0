# TotalPos API - Guía de Instalación y Configuración

API REST para integración con terminal BBVA PAX usando SDK TotalPos.

---

## 📋 Requisitos Previos

### Software Necesario

1. **Java JDK 8 o superior**
   - Descargar desde: https://www.oracle.com/java/technologies/downloads/
   - Verificar instalación: `java -version`

2. **.NET Framework 4.8**
   - Incluido en Windows 10/11 o descargar desde: https://dotnet.microsoft.com/download/dotnet-framework

3. **Git** (para clonar el repositorio)
   - Descargar desde: https://git-scm.com/downloads

4. **Terminal PAX** (requerido)
   - Conectado al puerto COM configurado
   - Verificar puerto en Administrador de dispositivos de Windows

---

## 📥 Instalación

### 1. Clonar el Repositorio

```powershell
# Navegar a la carpeta donde deseas clonar el proyecto
cd C:\Projects

# Clonar el repositorio
git clone <URL_DEL_REPOSITORIO> api_bbva

# Entrar al directorio del proyecto
cd api_bbva
```

---

## ⚙️ Configuración Inicial

### 2. Verificar Configuración

Los archivos de configuración ya vienen incluidos en el repositorio:

- `config/pinpad.config` - Configuración del terminal PAX y SDK
- `config/bines_20210520181704.config` - BINs de tarjetas
- `config/corresponsales.config` - Corresponsales
- `config/Local.config` - Configuración local

---

## 🚀 Inicialización del Proyecto

### 3. Iniciar el Simulador Java (BBVA Host)

El simulador emula el servidor del banco para validar transacciones de forma simulada. **Trabaja en conjunto con el terminal PAX real** para procesar las transacciones.

```powershell
# Desde la raíz del proyecto
cd simulador\Servidor\main\webapp\WEB-INF\views

# Iniciar el simulador (escucha en puerto 8080)
java -jar servidor-totalpos.jar
```

**Mantener esta terminal abierta** - El simulador debe estar corriendo durante las pruebas.

---

### 4. Compilar y Ejecutar la API TotalPos

Abrir una **nueva terminal PowerShell**:

```powershell
# Navegar a la carpeta de la API
cd src\TotalPosApi

# Compilar el proyecto (primera vez)
dotnet build

# O si usas MSBuild directamente
msbuild TotalPosApi.csproj /p:Configuration=Debug

# Ejecutar la API
.\bin\Debug\net48\TotalPosApi.exe
```

**Salida esperada:**
```
TotalPos API Server
===================
Listening on: http://localhost:5000
Press Ctrl+C to stop...
```

**Mantener esta terminal abierta** - La API debe estar corriendo para procesar peticiones.

---

## ✅ Verificación de la Instalación

### 5. Pruebas Iniciales

Abrir una **tercera terminal PowerShell** para ejecutar pruebas:

#### a) Health Check
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/health" -Method GET
```

**Respuesta esperada:**
```powershell
status  service
------  -------
healthy TotalPos API
```

#### b) Inicializar SDK
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/initialize" -Method POST
```

**Respuesta esperada:**
```powershell
success message
------- -------
   True SDK initialized successfully
```

#### c) Venta de Prueba (10 pesos)
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method POST -ContentType "application/json" -Body '{"Amount":10.00,"Reference":"TEST-001"}'
```

**Respuesta esperada:**
```powershell
codigoRespuesta leyenda           autorizacion referenciaFinanciera aprobada
--------------- -------           ------------ --------------------- --------
00              APROBADA 623521   623521       513544661211          True
```

---

## 📖 Documentación Completa

Para la documentación detallada de todos los endpoints, ver:
- [API_DOCUMENTATION.md](API_DOCUMENTATION.md)

Incluye:
- Todos los endpoints disponibles
- Ejemplos de request/response
- Comandos PowerShell para copiar y pegar
- Códigos de respuesta
- Información de logs


---


## 📝 Logs de Transacciones

Todas las transacciones se registran automáticamente en:
```
src/TotalPosApi/Logs/transactions_YYYYMMDD.log
```

Formato:
```
Timestamp|Type|Amount|Reference|ResponseCode|Message|AuthCode|FinancialRef
2026-01-07 10:30:45|SALE|10.00|TEST-001|00|APROBADA 623521|623521|513544661211
```

---

## 🚦 Flujo de Trabajo Típico

1. **Iniciar Simulador** (una vez)
   ```powershell
   cd simulador\Servidor\main\webapp\WEB-INF\views
   java -jar servidor-totalpos.jar
   ```

2. **Iniciar API** (cada vez que se desarrolle)
   ```powershell
   cd src\TotalPosApi
   .\bin\Debug\net48\TotalPosApi.exe
   ```

3. **Inicializar SDK** (al comenzar sesión)
   ```powershell
   Invoke-RestMethod -Uri "http://localhost:5000/api/initialize" -Method POST
   ```

4. **Realizar transacciones**
   ```powershell
   # Ejemplo: Venta de 50 pesos
   Invoke-RestMethod -Uri "http://localhost:5000/api/sale" -Method POST -ContentType "application/json" -Body '{"Amount":50.00,"Reference":"VENTA-001"}'
   ```

5. **Revisar logs** (si es necesario)
   ```powershell
   Get-Content src\TotalPosApi\Logs\transactions_*.log -Tail 20
   ```

---
