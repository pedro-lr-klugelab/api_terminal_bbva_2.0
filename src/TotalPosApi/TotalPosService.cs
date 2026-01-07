using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Xml;
using EGlobal.TotalPosSDKNet.Interfaz.Authorizer;
using EGlobal.TotalPosSDKNet.Interfaz.Catalog;
using EGlobal.TotalPosSDKNet.Interfaz.Util;

public class TotalPosService
{
    private readonly string _connectionType;
    private readonly string _afiliacion;
    private readonly string _terminal;
    private bool _isInitialized = false;
    private const string CONFIG_PATH = "pinpad.config";

    public TotalPosService(string connectionType, string afiliacion, string terminal)
    {
        _connectionType = connectionType;
        _afiliacion = afiliacion;
        _terminal = terminal;
    }

    public void Initialize()
    {
        if (_isInitialized)
            return;

        Console.WriteLine("Initializing TotalPos SDK...");
        Console.WriteLine($"Current Directory: {System.IO.Directory.GetCurrentDirectory()}");
        Console.WriteLine($"Base Directory: {AppDomain.CurrentDomain.BaseDirectory}");
        Console.WriteLine($"Checking config files:");
        Console.WriteLine($"  pinpad.config exists: {System.IO.File.Exists("pinpad.config")}");
        Console.WriteLine($"  corresponsales.config exists: {System.IO.File.Exists("corresponsales.config")}");
        Console.WriteLine($"  bines_20210520181704.config exists: {System.IO.File.Exists("bines_20210520181704.config")}");
        
        string macAddress = GetMacAddress();
        Console.WriteLine($"MAC Address: {macAddress}");

        var config = new Configuracion
        {
            PinPadConexion = _connectionType,
            PinPadPuerto = "",
            PinPadTimeOut = "60",
            PinPadMensaje = "SISTEMA LISTO",
            
            HostUrl = "http://localhost:8080/totalpos/ws/autorizaciones/transacciones",
            ComercioAfiliacion = _afiliacion,
            ComercioTerminal = _terminal,
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
        
        // Verificar si el PinPad se inicializó correctamente
        if (Interfaz.Instance.PinPad == null)
        {
            Console.WriteLine("WARNING: PinPad is NULL after initialization. Attempting manual initialization...");
            try
            {
                var pinPadUsb = new EGlobal.TotalPosSDKNet.JPinPad.PinPad.PinPadUsb(_connectionType, 60);
                pinPadUsb.Open();
                
                if (pinPadUsb.IsOpened())
                {
                    Console.WriteLine("PinPad opened successfully.");
                    var info = pinPadUsb.GetInfo();
                    if (info != null)
                    {
                        Console.WriteLine($"Terminal detected: {info.Model}, SN: {info.SerialNumber}");
                    }
                    pinPadUsb.Close();
                    Console.WriteLine("Port closed to allow SDK control.");
                }
                
                // Use reflection to inject PinPad instance
                var type = typeof(Interfaz);
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var field = type.GetField("pinPad", flags) ?? type.GetField("_pinPad", flags);
                
                if (field != null)
                {
                    var newPinPad = new EGlobal.TotalPosSDKNet.JPinPad.PinPad.PinPadUsb(_connectionType, 60);
                    field.SetValue(Interfaz.Instance, newPinPad);
                    Console.WriteLine("PinPad instance injected successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize PinPad: {ex.Message}");
            }
        }
        
        _isInitialized = true;
        Console.WriteLine("SDK Initialized Successfully");
        
        // Verificar bandera de carga de llaves
        if (CheckKeysFlag())
        {
            Console.WriteLine("Keys flag is set to 1. Performing automatic key load...");
            var keyLoadResult = LoadKeys();
            
            // Si la carga fue exitosa (código 00), actualizar la bandera a 0
            if (keyLoadResult.CodigoRespuesta == "00")
            {
                Console.WriteLine("Key load successful. Updating keys flag to 0...");
                UpdateKeysFlag("0");
            }
            else
            {
                Console.WriteLine($"Key load failed with code {keyLoadResult.CodigoRespuesta}. Flag remains at 1.");
            }
        }
    }

    public dynamic LoadKeys()
    {
        // Ensure SDK is initialized before loading keys
        if (!_isInitialized)
            Initialize();

        Console.WriteLine("Loading terminal keys (Carga de Llaves)...");
        Console.WriteLine($"Using Afiliacion: {_afiliacion}, Terminal: {_terminal}");
        
        var peticion = new Peticion();
        peticion.SetAfiliacion(_afiliacion, Moneda.Pesos);
        
        string macAddress = GetMacAddress();
        Console.WriteLine($"Using MAC: {macAddress}");
        peticion.SetTerminal(_terminal, macAddress);
        
        var parametros = new Dictionary<ParametroOperacion, object>();
        peticion.SetOperacion(Operacion.CargaLlaves, parametros);

        Console.WriteLine("Peticion configured. Calling Autorizar()...");
        Console.WriteLine($"Interfaz.Instance.PinPad is null? {Interfaz.Instance.PinPad == null}");
        
        var respuesta = peticion.Autorizar();
        
        Console.WriteLine($"Key load result: {respuesta.CodigoRespuesta} - {respuesta.Leyenda}");
        return respuesta;
    }

    public dynamic ProcessSale(decimal amount, string reference, decimal? cashback = null, string promoCodigo = null, decimal? puntos = null)
    {
        if (!_isInitialized)
            Initialize();

        var parametros = new Dictionary<ParametroOperacion, object>
        {
            { ParametroOperacion.Importe, amount.ToString("F2") },
            { ParametroOperacion.ReferenciaComercio, reference }
        };

        // Cashback/Advance
        if (cashback.HasValue && cashback.Value > 0)
            parametros.Add(ParametroOperacion.Cash, cashback.Value.ToString("F2"));

        // Puntos
        if (puntos.HasValue && puntos.Value > 0)
            parametros.Add(ParametroOperacion.Puntos, puntos.Value.ToString("F2"));

        var peticion = new Peticion();
        peticion.SetAfiliacion(_afiliacion, Moneda.Pesos);
        peticion.SetTerminal(_terminal, GetMacAddress());
        peticion.SetOperacion(Operacion.Venta, parametros);
        
        peticion.Operador = "API001";
        peticion.Fecha = DateTime.Now.ToString("yyyyMMddHHmmss");

        Console.WriteLine("Reading card...");
        var tarjeta = peticion.LeerTarjeta();
        
        Console.WriteLine("Authorizing transaction...");
        var respuesta = peticion.Autorizar();

        return respuesta;
    }

    public dynamic ProcessRefund(decimal amount, string reference, string financialReference)
    {
        if (!_isInitialized)
            Initialize();

        var parametros = new Dictionary<ParametroOperacion, object>
        {
            { ParametroOperacion.Importe, amount.ToString("F2") },
            { ParametroOperacion.ReferenciaComercio, reference },
            { ParametroOperacion.ReferenciaFinanciera, financialReference }
        };

        var peticion = new Peticion();
        peticion.SetAfiliacion(_afiliacion, Moneda.Pesos);
        peticion.SetTerminal(_terminal, GetMacAddress());
        peticion.SetOperacion(Operacion.Devolucion, parametros);
        
        peticion.Operador = "API001";
        peticion.Fecha = DateTime.Now.ToString("yyyyMMddHHmmss");

        Console.WriteLine("Reading card...");
        var tarjeta = peticion.LeerTarjeta();
        
        Console.WriteLine("Authorizing refund...");
        var respuesta = peticion.Autorizar();

        return respuesta;
    }

    public dynamic ConsultPoints()
    {
        if (!_isInitialized)
            Initialize();

        var parametros = new Dictionary<ParametroOperacion, object>();

        var peticion = new Peticion();
        peticion.SetAfiliacion(_afiliacion, Moneda.Pesos);
        peticion.SetTerminal(_terminal, GetMacAddress());
        peticion.SetOperacion(Operacion.ConsultaPuntos, parametros);
        
        peticion.Operador = "API001";
        peticion.Fecha = DateTime.Now.ToString("yyyyMMddHHmmss");

        Console.WriteLine("Reading card for points consultation...");
        var tarjeta = peticion.LeerTarjeta();
        
        Console.WriteLine("Consulting points...");
        var respuesta = peticion.Autorizar();

        return respuesta;
    }

    public dynamic CancelSale(string financialReference)
    {
        if (!_isInitialized)
            Initialize();

        var parametros = new Dictionary<ParametroOperacion, object>
        {
            { ParametroOperacion.ReferenciaFinanciera, financialReference }
        };

        var peticion = new Peticion();
        peticion.SetAfiliacion(_afiliacion, Moneda.Pesos);
        peticion.SetTerminal(_terminal, GetMacAddress());
        peticion.SetOperacion(Operacion.CancelacionVenta, parametros);
        
        peticion.Operador = "API001";
        peticion.Fecha = DateTime.Now.ToString("yyyyMMddHHmmss");

        Console.WriteLine("Cancelling sale...");
        var respuesta = peticion.Autorizar();

        return respuesta;
    }

    public dynamic CancelSaleCard(decimal amount, string financialReference)
    {
        if (!_isInitialized)
            Initialize();

        var parametros = new Dictionary<ParametroOperacion, object>
        {
            { ParametroOperacion.Importe, amount.ToString("F2") },
            { ParametroOperacion.ReferenciaFinanciera, financialReference }
        };

        var peticion = new Peticion();
        peticion.SetAfiliacion(_afiliacion, Moneda.Pesos);
        peticion.SetTerminal(_terminal, GetMacAddress());
        peticion.SetOperacion(Operacion.CancelacionVentaTarjeta, parametros);
        
        peticion.Operador = "API001";
        peticion.Fecha = DateTime.Now.ToString("yyyyMMddHHmmss");

        Console.WriteLine("Reading card for sale cancellation...");
        var tarjeta = peticion.LeerTarjeta();
        
        Console.WriteLine("Cancelling sale with card...");
        var respuesta = peticion.Autorizar();

        return respuesta;
    }

    public dynamic CancelRefund(string financialReference)
    {
        if (!_isInitialized)
            Initialize();

        var parametros = new Dictionary<ParametroOperacion, object>
        {
            { ParametroOperacion.ReferenciaFinanciera, financialReference }
        };

        var peticion = new Peticion();
        peticion.SetAfiliacion(_afiliacion, Moneda.Pesos);
        peticion.SetTerminal(_terminal, GetMacAddress());
        peticion.SetOperacion(Operacion.CancelacionDevolucion, parametros);
        
        peticion.Operador = "API001";
        peticion.Fecha = DateTime.Now.ToString("yyyyMMddHHmmss");

        Console.WriteLine("Cancelling refund...");
        var respuesta = peticion.Autorizar();

        return respuesta;
    }

    public dynamic CancelRefundCard(decimal amount, string financialReference)
    {
        if (!_isInitialized)
            Initialize();

        var parametros = new Dictionary<ParametroOperacion, object>
        {
            { ParametroOperacion.Importe, amount.ToString("F2") },
            { ParametroOperacion.ReferenciaFinanciera, financialReference }
        };

        var peticion = new Peticion();
        peticion.SetAfiliacion(_afiliacion, Moneda.Pesos);
        peticion.SetTerminal(_terminal, GetMacAddress());
        peticion.SetOperacion(Operacion.CancelacionDevolucionTarjeta, parametros);
        
        peticion.Operador = "API001";
        peticion.Fecha = DateTime.Now.ToString("yyyyMMddHHmmss");

        Console.WriteLine("Reading card for refund cancellation...");
        var tarjeta = peticion.LeerTarjeta();
        
        Console.WriteLine("Cancelling refund with card...");
        var respuesta = peticion.Autorizar();

        return respuesta;
    }

    private string GetMacAddress()
    {
        try
        {
            var nic = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up && 
                                    n.NetworkInterfaceType != NetworkInterfaceType.Loopback);
            
            if (nic == null)
                return "00-00-00-00-00-00";
            
            var macBytes = nic.GetPhysicalAddress().GetAddressBytes();
            if (macBytes.Length == 0)
                return "00-00-00-00-00-00";
            
            // Format as XX-XX-XX-XX-XX-XX (with dashes)
            return string.Join("-", macBytes.Select(b => b.ToString("X2")));
        }
        catch
        {
            return "00-00-00-00-00-00";
        }
    }

    private bool CheckKeysFlag()
    {
        try
        {
            if (!System.IO.File.Exists(CONFIG_PATH))
            {
                Console.WriteLine($"Config file not found: {CONFIG_PATH}");
                return false;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(CONFIG_PATH);
            
            var node = xmlDoc.SelectSingleNode("//appSettings/add[@key='llaves']");
            if (node == null)
            {
                Console.WriteLine("Keys flag not found in config");
                return false;
            }

            string value = node.Attributes["value"]?.Value ?? "0";
            Console.WriteLine($"Keys flag value: {value}");
            return value == "1";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading keys flag: {ex.Message}");
            return false;
        }
    }

    private void UpdateKeysFlag(string newValue)
    {
        try
        {
            if (!System.IO.File.Exists(CONFIG_PATH))
            {
                Console.WriteLine($"Config file not found: {CONFIG_PATH}");
                return;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(CONFIG_PATH);
            
            var node = xmlDoc.SelectSingleNode("//appSettings/add[@key='llaves']");
            if (node == null)
            {
                Console.WriteLine("Keys flag not found in config");
                return;
            }

            node.Attributes["value"].Value = newValue;
            xmlDoc.Save(CONFIG_PATH);
            Console.WriteLine($"Keys flag updated to: {newValue}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating keys flag: {ex.Message}");
        }
    }
}