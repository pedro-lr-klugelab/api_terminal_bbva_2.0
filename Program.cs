using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using EGlobal.TotalPosSDKNet.Interfaz.Authorizer;
using EGlobal.TotalPosSDKNet.Interfaz.Catalog;
using EGlobal.TotalPosSDKNet.Interfaz.Util;

namespace SimpleConnectionTest
{
    class Program
    {
        // ---------------------------------------------------------
        // CONFIGURATION - EDIT HERE
        // ---------------------------------------------------------
        static string ConnectionType = DetectTerminalComPort(); // Auto-detect COM port
        static string Port = "";               // Empty for Serial
        static string Afiliacion = "0000001";  // Provided by BBVA
        static string Terminal = "00000001";   // Provided by BBVA

        public static bool IsInitialized = false;
        // ---------------------------------------------------------

        static void Main(string[] args)
        {
            // Check for API mode
            if (args.Length > 0 && args[0] == "--api")
            {
                var server = new HttpApiServer();
                server.Start();
                Console.WriteLine("Press ENTER to stop the API server...");
                Console.ReadLine();
                server.Stop();
                return;
            }

            // Original console menu mode
            while (true)
            {
                Console.Clear();
                Console.WriteLine("==========================================");
                Console.WriteLine("   BBVA Terminal Integration Tool         ");
                Console.WriteLine("   Terminal: PAX SP30                     ");
                Console.WriteLine("==========================================");
                Console.WriteLine($"Config: {ConnectionType} | Term: {Terminal} | Merch: {Afiliacion}");
                Console.WriteLine("------------------------------------------");
                Console.WriteLine("1. Initialize SDK & Test Connection");
                Console.WriteLine("2. Load Keys (Carga de Llaves) - MANDATORY for new terminals");
                Console.WriteLine("3. Test Sale (Venta) - Phase 3 Example");
                Console.WriteLine("4. Exit");
                Console.Write("\nSelect option: ");

                var key = Console.ReadLine();

                try
                {
                    if (key == "1") TestConnection();
                    else if (key == "2") LoadKeys();
                    else if (key == "3") TestSale();
                    else if (key == "4") break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nCRITICAL ERROR: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                    Pause();
                }
            }
        }

        static void InitializeSDK()
        {
            Console.WriteLine("\nInitializing SDK (Interfaz)...");
            
            string macAddress = GetMacAddress();
            Console.WriteLine($"Detected MAC Address: {macAddress}");

            var config = new Configuracion
            {
                PinPadConexion = ConnectionType,
                PinPadPuerto = Port,
                PinPadTimeOut = "60",
                PinPadMensaje = "SISTEMA LISTO",
                
                HostUrl = "http://localhost:8080/totalpos/ws/autorizaciones/transacciones",
                ComercioAfiliacion = Afiliacion,
                ComercioTerminal = Terminal,
                ComercioMac = macAddress,
                
                Logs = true,
                ClaveLogs = "clave-logs",
                IdAplicacion = "7e8e667438cb00903250ae3cda9b4bc9398f8084bb46aff9495286bf344a695f",
                ClaveSecreta = "609a8c80-0134-48ce-b63f-2b8031268b09",
                
                BinesUrl = "http://localhost:8080/totalpos/ws/recursos/bines/concentrado/actualizaciones",
                TokenUrl = "http://localhost:8080/totalpos/ws/autenticacion/oauth/token",
                TelecargaUrl = "http://localhost:8080/telecargas/descargas",
                HostTimeOut = "30",
                
                PinPadContactless = true
            };

            Interfaz.Instance.Configuracion = config;
            Interfaz.Instance.Inicializar();
            Console.WriteLine("SDK Initialized.");

            // REFLECTION HACK: If PinPad is still null, force it!
            if (Interfaz.Instance.PinPad == null)
            {
                Console.WriteLine("WARNING: Interfaz.PinPad is NULL. Attempting Reflection Injection...");
                try
                {
                    var pinPadUsb = new EGlobal.TotalPosSDKNet.JPinPad.PinPad.PinPadUsb(ConnectionType, 60);
                    
                    // Verify it works, then CLOSE it so the SDK can open it
                    pinPadUsb.Open();
                    if (pinPadUsb.IsOpened())
                    {
                        Console.WriteLine("Hack: PinPadUsb created and verified.");
                        pinPadUsb.Close(); // CRITICAL: Close it to release the COM port lock!
                        Console.WriteLine("Hack: Port closed to allow SDK to take over.");
                    }

                    var type = typeof(Interfaz);
                    var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                    var field = type.GetField("pinPad", flags) ?? type.GetField("_pinPad", flags) ?? type.GetField("m_pinPad", flags);

                    if (field != null)
                    {
                        field.SetValue(Interfaz.Instance, pinPadUsb);
                        Console.WriteLine($"SUCCESS: Injected PinPadUsb into '{field.Name}' field.");
                    }
                    else
                    {
                        var prop = type.GetProperty("PinPad", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (prop != null && prop.CanWrite)
                        {
                            prop.SetValue(Interfaz.Instance, pinPadUsb, null);
                            Console.WriteLine("SUCCESS: Set PinPad property directly.");
                        }
                        else
                        {
                            var fields = type.GetFields(flags);
                            foreach (var f in fields)
                            {
                                if (f.FieldType == typeof(EGlobal.TotalPosSDKNet.JPinPad.PinPad.IPinPad))
                                {
                                    f.SetValue(Interfaz.Instance, pinPadUsb);
                                    Console.WriteLine($"SUCCESS: Injected PinPadUsb into '{f.Name}' field (Type Match).");
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"REFLECTION FAILED: {ex.Message}");
                }
            }
        }

        static void TestConnection()
        {
            Console.WriteLine("\n--- STEP 1: Direct Driver Test ---");
            Console.WriteLine($"Attempting to connect directly to {ConnectionType} using PinPadUsb class...");
            
            EGlobal.TotalPosSDKNet.JPinPad.PinPad.PinPadUsb directPinPad = null;
            bool directSuccess = false;

            try
            {
                directPinPad = new EGlobal.TotalPosSDKNet.JPinPad.PinPad.PinPadUsb(ConnectionType, 60);
                Console.WriteLine("PinPadUsb Object Created.");
                
                Console.Write("Calling Open()... ");
                directPinPad.Open();
                Console.WriteLine("OK");

                Console.Write("Calling IsOpened()... ");
                if (directPinPad.IsOpened())
                {
                    Console.WriteLine("YES");
                    
                    Console.Write("Calling GetInfo()... ");
                    var info = directPinPad.GetInfo();
                    if (info != null)
                        Console.WriteLine($"Model: {info.Model}, SN: {info.SerialNumber}");
                    else
                        Console.WriteLine("NULL Info");

                    Console.Write("Calling ShowMessage()... ");
                    directPinPad.ShowMessage("DIRECT OK");
                    Console.WriteLine("Sent");

                    directSuccess = true;
                    directPinPad.Close();
                    Console.WriteLine("Connection Closed.");
                }
                else
                {
                    Console.WriteLine("NO (Open() didn't throw but IsOpened is false)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nDIRECT CONNECTION FAILED: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                if (ex.InnerException != null) Console.WriteLine($"Inner: {ex.InnerException.Message}");
                
                Console.WriteLine("\nPossible Causes:");
                Console.WriteLine("- Port is in use by another app (maybe previous instance?)");
                Console.WriteLine("- Driver is not installed correctly.");
                Console.WriteLine("- Missing native DLLs.");
            }

            if (!directSuccess)
            {
                Console.WriteLine("\nAborting SDK Initialization because Direct Test failed.");
                Pause();
                return;
            }

            Console.WriteLine("\n--- STEP 2: SDK Interfaz Test ---");
            try
            {
                InitializeSDK();
                
                if (Interfaz.Instance.PinPad == null)
                {
                    Console.WriteLine("CRITICAL ERROR: Interfaz.Instance.PinPad is NULL.");
                    Console.WriteLine("Direct connection worked, so the issue is in the 'Interfaz' configuration or singleton initialization.");
                }
                else
                {
                    Console.Write("Checking Interfaz.IsConnected()... ");
                    Console.WriteLine(Interfaz.Instance.PinPad.IsConnected() ? "OK" : "FAILED");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SDK INITIALIZATION ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            Pause();
        }

        static void LoadKeys()
        {
            InitializeSDK();
            Console.WriteLine("\n--- Loading Keys (Carga de Llaves) ---");
            Console.WriteLine("This is mandatory for new terminals.");
            Console.WriteLine("Please wait...");

            try
            {
                var peticion = new Peticion();
                peticion.SetAfiliacion(Afiliacion, Moneda.Pesos);
                peticion.SetTerminal(Terminal, GetMacAddress());
                
                var parametros = new Dictionary<ParametroOperacion, object>();
                peticion.SetOperacion(Operacion.CargaLlaves, parametros);

                Console.WriteLine("Sending Key Load Request...");
                var respuesta = peticion.Autorizar();

                Console.WriteLine($"Result: {respuesta.CodigoRespuesta} - {respuesta.Leyenda}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading keys: {ex.Message}");
            }
            Pause();
        }

        static void TestSale()
        {
            if (!System.IO.File.Exists("pinpad.config"))
            {
                Console.WriteLine("ERROR: 'pinpad.config' not found.");
                Pause();
                return;
            }

            InitializeSDK();
            Console.WriteLine("\n--- Test Sale (Venta) ---");
            
            if (Interfaz.Instance.PinPad == null)
            {
                Console.WriteLine("CRITICAL ERROR: PinPad is NULL. Cannot proceed.");
                Pause();
                return;
            }

            try
            {
                Console.Write("Enter Amount (e.g. 10.00): ");
                string amountStr = Console.ReadLine();
                if (!decimal.TryParse(amountStr, out decimal amount)) amount = 1.00m;

                var parametros = new Dictionary<ParametroOperacion, object>();
                parametros.Add(ParametroOperacion.Importe, amountStr);
                parametros.Add(ParametroOperacion.ReferenciaComercio, "TEST-" + DateTime.Now.Ticks.ToString().Substring(10));

                var peticion = new Peticion();
                peticion.SetAfiliacion(Afiliacion, Moneda.Pesos);
                peticion.SetTerminal(Terminal, GetMacAddress());
                peticion.SetOperacion(Operacion.Venta, parametros);
                
                peticion.Operador = "OPE001";
                peticion.Fecha = DateTime.Now.ToString("yyyyMMddHHmmss");

                Console.WriteLine("Follow instructions on Terminal...");
                Console.WriteLine("Reading Card (LeerTarjeta)...");
                
                var tarjeta = peticion.LeerTarjeta();
                Console.WriteLine("Card Read Complete.");
                
                if (tarjeta != null)
                {
                    Console.WriteLine("Card object received.");
                }

                Console.WriteLine("Authorizing (Autorizar)...");
                var respuesta = peticion.Autorizar();

                if (respuesta == null)
                {
                    Console.WriteLine("ERROR: Respuesta is NULL.");
                }
                else
                {
                    Console.WriteLine($"\nRESPONSE: {respuesta.CodigoRespuesta}");
                    Console.WriteLine($"Message: {respuesta.Leyenda}");
                    Console.WriteLine($"Auth Code: {respuesta.Autorizacion}");
                    
                    if (respuesta.CodigoRespuesta == "00")
                        Console.WriteLine("TRANSACTION APPROVED");
                    else
                        Console.WriteLine("TRANSACTION DECLINED");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Transaction Failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            Pause();
        }

        static string GetMacAddress()
        {
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .Select(nic => BitConverter.ToString(nic.GetPhysicalAddress().GetAddressBytes()))
                    .FirstOrDefault() ?? "00-00-00-00-00-00";
            }
            catch
            {
                return "00-00-00-00-00-00";
            }
        }

        static string DetectTerminalComPort()
        {
            Console.WriteLine("[INFO] Starting COM port auto-detection for terminal...");
            
            try
            {
                // Step 1: Find USB Serial Device Candidates using WMI
                var candidates = new List<string>();
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%COM%'");
                
                foreach (ManagementObject device in searcher.Get())
                {
                    string caption = device["Caption"]?.ToString() ?? "";
                    Console.WriteLine($"[DEBUG] Found device: {caption}");
                    
                    // Check if it's a USB Serial device (handles both English "Serial" and Spanish "Serie")
                    string captionUpper = caption.ToUpper();
                    if (captionUpper.Contains("USB") &&
                        (captionUpper.Contains("SERIAL") ||captionUpper.Contains("SERIE")))
                    {
                        // Extract COM port number
                        var match = System.Text.RegularExpressions.Regex.Match(caption, @"COM(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            string portName = "COM" + match.Groups[1].Value;
                            candidates.Add(portName);
                            Console.WriteLine($"[INFO] Found USB Serial device candidate: {portName}");
                        }
                    }
                }
                
                // Step 2: Validate each candidate with Terminal SDK
                foreach (var portName in candidates)
                {
                    Console.WriteLine($"[INFO] Validating {portName} with terminal SDK...");
                    
                    try
                    {
                        // Try to create and test connection with PinPadUsb
                        var testPinPad = new EGlobal.TotalPosSDKNet.JPinPad.PinPad.PinPadUsb(portName, 10); // Short timeout for testing
                        testPinPad.Open();
                        
                        if (testPinPad.IsOpened())
                        {
                            // Try to get device info as final validation
                            var info = testPinPad.GetInfo();
                            if (info != null)
                            {
                                Console.WriteLine($"[SUCCESS] Detected PAX terminal on {portName}");
                                Console.WriteLine($"[INFO] Model: {info.Model}, SN: {info.SerialNumber}");
                                testPinPad.Close();
                                return portName;
                            }
                        }
                        
                        testPinPad.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DEBUG] {portName} validation failed: {ex.Message}");
                    }
                }
                
                // Step 3: No valid terminal found, throw exception
                string errorMsg = candidates.Count > 0 
                    ? $"Found {candidates.Count} USB Serial device(s) but none responded as a PAX terminal. Please ensure the terminal is connected and powered on."
                    : "No USB Serial devices detected. Please connect the PAX terminal via USB.";
                
                Console.WriteLine($"[ERROR] {errorMsg}");
                throw new InvalidOperationException(errorMsg);
            }
            catch (InvalidOperationException)
            {
                // Re-throw our own exceptions
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] COM port detection failed: {ex.Message}");
                throw new InvalidOperationException("Failed to detect terminal COM port. Please ensure the terminal is connected.", ex);
            }
        }

        // ---------------------------------------------------------
        // API WRAPPER METHODS
        // ---------------------------------------------------------
        public static string ApiInitialize()
        {
            try
            {
                InitializeSDK();
                if (Interfaz.Instance.PinPad != null && Interfaz.Instance.PinPad.IsConnected())
                {
                    IsInitialized = true;
                    return "{\"success\":true,\"message\":\"Terminal Initialized\"}";
                }
                return "{\"success\":false,\"message\":\"PinPad Not Connected\"}";
            }
            catch (Exception ex)
            {
                return "{\"success\":false,\"message\":\"" + ex.Message.Replace("\"", "'") + "\"}";
            }
        }

        public static string ApiSale(string jsonBody)
        {
            if (!IsInitialized) return "{\"success\":false,\"message\":\"Not Initialized\"}";

            try
            {
                // LOG: Debug the incoming JSON
                Console.WriteLine($"[DEBUG] Received JSON Body: {jsonBody}");
                
                // Parse amount from JSON using Regex (handles whitespace from PowerShell's ConvertTo-Json)
                string amount = "1.00";
                var match = System.Text.RegularExpressions.Regex.Match(jsonBody, @"""Importe""\s*:\s*""?([^""}\s,]+)""?");
                if (match.Success)
                {
                    amount = match.Groups[1].Value;
                    Console.WriteLine($"[DEBUG] Parsed Importe value: '{amount}'");
                }
                else
                {
                    Console.WriteLine($"[DEBUG] 'Importe' parameter NOT FOUND in JSON. Using default: {amount}");
                }

                Console.WriteLine($"[DEBUG] Adding to SDK parameters - Importe: '{amount}'");
                var parametros = new Dictionary<ParametroOperacion, object>();
                parametros.Add(ParametroOperacion.Importe, amount);
                parametros.Add(ParametroOperacion.ReferenciaComercio, "API-" + DateTime.Now.Ticks);

                var peticion = new Peticion();
                peticion.SetAfiliacion(Afiliacion, Moneda.Pesos);
                peticion.SetTerminal(Terminal, GetMacAddress());
                peticion.SetOperacion(Operacion.Venta, parametros);
                peticion.Operador = "API";
                peticion.Fecha = DateTime.Now.ToString("yyyyMMddHHmmss");

                var tarjeta = peticion.LeerTarjeta();
                var respuesta = peticion.Autorizar();

                string authCode = respuesta.Autorizacion ?? "";
                string respCode = respuesta.CodigoRespuesta ?? "";
                string message = respuesta.Leyenda ?? "";

                return "{\"success\":" + (respCode == "00" ? "true" : "false") + ",\"message\":\"" + message + "\",\"authCode\":\"" + authCode + "\",\"responseCode\":\"" + respCode + "\"}";
            }
            catch (Exception ex)
            {
                return "{\"success\":false,\"message\":\"" + ex.Message.Replace("\"", "'") + "\"}";
            }
        }

        static void Pause()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}
