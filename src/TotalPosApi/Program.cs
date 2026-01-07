using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    private static TotalPosService _sdk;
    private static HttpListener _listener;
    private static bool _isRunning = false;

    // Configuration - adjust as needed
    private const string Afiliacion = "0000001";
    private const string Terminal = "00000001";
    private const int Port = 5000;
    
    static void Main(string[] args)
    {
        Console.WriteLine("==================================");
        Console.WriteLine("  TotalPos API Server");
        Console.WriteLine("==================================\n");

        // Auto-detect COM port
        string connectionType;
        try
        {
            connectionType = DetectTerminalComPort();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: {ex.Message}");
            Console.WriteLine("Falling back to USB connection type.\n");
            connectionType = "USB";
        }

        // Initialize SDK service
        _sdk = new TotalPosService(connectionType, Afiliacion, Terminal);

        // Start HTTP API
        StartApi();
        
        Console.WriteLine("\nPress ENTER to stop the server...");
        Console.ReadLine();
        
        StopApi();
    }

    static void StartApi()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{Port}/");
        _listener.Start();
        _isRunning = true;

        // Ensure logs directory exists
        Directory.CreateDirectory("logs");
        
        Console.WriteLine($"API Server running on http://localhost:{Port}");
        Console.WriteLine("\nEndpoints:");
        Console.WriteLine($"  POST http://localhost:{Port}/api/initialize");
        Console.WriteLine($"  POST http://localhost:{Port}/api/loadkeys");
        Console.WriteLine($"  POST http://localhost:{Port}/api/sale");
        Console.WriteLine($"  POST http://localhost:{Port}/api/refund");
        Console.WriteLine($"  POST http://localhost:{Port}/api/consultpoints");
        Console.WriteLine($"  POST http://localhost:{Port}/api/cancelsale");
        Console.WriteLine($"  POST http://localhost:{Port}/api/cancelsalecard");
        Console.WriteLine($"  POST http://localhost:{Port}/api/cancelrefund");
        Console.WriteLine($"  POST http://localhost:{Port}/api/cancelrefundcard");
        Console.WriteLine($"  GET  http://localhost:{Port}/api/health\n");

        _ = Task.Run(() => ListenLoop());
    }

    static async void ListenLoop()
    {
        while (_isRunning)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequest(context));
            }
            catch (Exception ex)
            {
                if (_isRunning)
                    Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }
    }

    static void HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        
        response.AddHeader("Access-Control-Allow-Origin", "*");
        response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
        response.ContentType = "application/json";

        string result = "{}";
        
        try
        {
            string path = request.Url.AbsolutePath.ToLower();
            string method = request.HttpMethod;
            
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {method} {path}");

            if (method == "OPTIONS")
            {
                response.StatusCode = 200;
                response.Close();
                return;
            }

            if (path == "/api/initialize" && method == "POST")
            {
                result = HandleInitialize();
            }
            else if (path == "/api/loadkeys" && method == "POST")
            {
                result = HandleLoadKeys();
            }
            else if (path == "/api/sale" && method == "POST")
            {
                result = HandleSale(request);
            }
            else if (path == "/api/refund" && method == "POST")
            {
                result = HandleRefund(request);
            }
            else if (path == "/api/consultpoints" && method == "POST")
            {
                result = HandleConsultPoints();
            }
            else if (path == "/api/cancelsale" && method == "POST")
            {
                result = HandleCancelSale(request);
            }
            else if (path == "/api/cancelsalecard" && method == "POST")
            {
                result = HandleCancelSaleCard(request);
            }
            else if (path == "/api/cancelrefund" && method == "POST")
            {
                result = HandleCancelRefund(request);
            }
            else if (path == "/api/cancelrefundcard" && method == "POST")
            {
                result = HandleCancelRefundCard(request);
            }
            else if (path == "/api/health" && method == "GET")
            {
                result = "{\"status\":\"healthy\",\"service\":\"TotalPos API\"}";
            }
            else
            {
                response.StatusCode = 404;
                result = JsonConvert.SerializeObject(new { error = "Not Found" });
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = 500;
            result = JsonConvert.SerializeObject(new { error = ex.Message, stack = ex.StackTrace });
            Console.WriteLine($"[ERROR] {ex.Message}");
        }

        byte[] buffer = Encoding.UTF8.GetBytes(result);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    static string HandleInitialize()
    {
        try
        {
            _sdk.Initialize();
            
            return JsonConvert.SerializeObject(new
            {
                success = true,
                message = "SDK initialized successfully"
            });
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new
            {
                success = false,
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    static string HandleLoadKeys()
    {
        try
        {
            var resultado = _sdk.LoadKeys();

            return JsonConvert.SerializeObject(new
            {
                success = resultado.CodigoRespuesta == "00",
                codigoRespuesta = resultado.CodigoRespuesta,
                leyenda = resultado.Leyenda,
                autorizacion = resultado.Autorizacion,
                referenciaFinanciera = resultado.ReferenciaFinanciera,
                message = resultado.CodigoRespuesta == "00" ? "Keys loaded successfully" : $"LoadKeys failed: {resultado.Leyenda}",
                details = new
                {
                    idOperacion = resultado.IdOperacion,
                    numeroControl = resultado.NumeroControl
                }
            });
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new
            {
                success = false,
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    static string HandleSale(HttpListenerRequest request)
    {
        string body = new StreamReader(request.InputStream).ReadToEnd();
        var saleRequest = JsonConvert.DeserializeObject<SaleRequest>(body);

        if (saleRequest == null || saleRequest.Amount <= 0)
            throw new ArgumentException("Invalid sale request");

        string reference = saleRequest.Reference ?? $"REF-{DateTime.Now.Ticks}";
        
        var resultado = _sdk.ProcessSale(
            saleRequest.Amount, 
            reference, 
            saleRequest.Cashback, 
            saleRequest.PromoCode, 
            saleRequest.Points
        );

        // Log transaction to console
        Console.WriteLine("\n========================================");
        Console.WriteLine("TRANSACTION COMPLETED");
        Console.WriteLine("========================================");
        Console.WriteLine($"Timestamp:       {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Type:            SALE");
        Console.WriteLine($"Amount:          ${saleRequest.Amount:F2}");
        Console.WriteLine($"Reference:       {reference}");
        Console.WriteLine($"Response Code:   {resultado.CodigoRespuesta ?? "N/A"}");
        Console.WriteLine($"Status:          {(resultado.CodigoRespuesta == "00" ? "APPROVED" : "DECLINED")}");
        Console.WriteLine($"Message:         {resultado.Leyenda ?? "N/A"}");
        Console.WriteLine($"Auth Code:       {resultado.Autorizacion ?? "N/A"}");
        Console.WriteLine($"Financial Ref:   {resultado.ReferenciaFinanciera ?? "N/A"}");
        Console.WriteLine("========================================\n");

        // Log to file
        LogTransactionToFile("SALE", saleRequest.Amount, reference, resultado);

        return JsonConvert.SerializeObject(new
        {
            codigoRespuesta = resultado.CodigoRespuesta,
            leyenda = resultado.Leyenda,
            autorizacion = resultado.Autorizacion,
            referenciaFinanciera = resultado.ReferenciaFinanciera,
            aprobada = resultado.CodigoRespuesta == "00"
        });
    }

    static string HandleRefund(HttpListenerRequest request)
    {
        string body = new StreamReader(request.InputStream).ReadToEnd();
        var refundRequest = JsonConvert.DeserializeObject<RefundRequest>(body);

        if (refundRequest == null || refundRequest.Amount <= 0)
            throw new ArgumentException("Invalid refund request");

        if (string.IsNullOrEmpty(refundRequest.FinancialReference))
            throw new ArgumentException("FinancialReference is required");

        string reference = refundRequest.Reference ?? $"REF-{DateTime.Now.Ticks}";
        
        var resultado = _sdk.ProcessRefund(refundRequest.Amount, reference, refundRequest.FinancialReference);

        Console.WriteLine("\n========================================");
        Console.WriteLine("REFUND COMPLETED");
        Console.WriteLine("========================================");
        Console.WriteLine($"Timestamp:       {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Type:            REFUND");
        Console.WriteLine($"Amount:          ${refundRequest.Amount:F2}");
        Console.WriteLine($"Reference:       {reference}");
        Console.WriteLine($"Response Code:   {resultado.CodigoRespuesta ?? "N/A"}");
        Console.WriteLine($"Status:          {(resultado.CodigoRespuesta == "00" ? "APPROVED" : "DECLINED")}");
        Console.WriteLine($"Message:         {resultado.Leyenda ?? "N/A"}");
        Console.WriteLine($"Auth Code:       {resultado.Autorizacion ?? "N/A"}");
        Console.WriteLine($"Financial Ref:   {resultado.ReferenciaFinanciera ?? "N/A"}");
        Console.WriteLine("========================================\n");

        LogTransactionToFile("REFUND", refundRequest.Amount, reference, resultado);

        return JsonConvert.SerializeObject(new
        {
            codigoRespuesta = resultado.CodigoRespuesta,
            leyenda = resultado.Leyenda,
            autorizacion = resultado.Autorizacion,
            referenciaFinanciera = resultado.ReferenciaFinanciera,
            aprobada = resultado.CodigoRespuesta == "00"
        });
    }

    static string HandleConsultPoints()
    {
        try
        {
            var resultado = _sdk.ConsultPoints();

            Console.WriteLine("\n========================================");
            Console.WriteLine("POINTS CONSULTATION COMPLETED");
            Console.WriteLine("========================================");
            Console.WriteLine($"Timestamp:       {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Response Code:   {resultado.CodigoRespuesta ?? "N/A"}");
            Console.WriteLine($"Message:         {resultado.Leyenda ?? "N/A"}");
            Console.WriteLine("========================================\n");

            return JsonConvert.SerializeObject(new
            {
                codigoRespuesta = resultado.CodigoRespuesta,
                leyenda = resultado.Leyenda,
                success = resultado.CodigoRespuesta == "00"
            });
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new
            {
                success = false,
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    static string HandleCancelSale(HttpListenerRequest request)
    {
        string body = new StreamReader(request.InputStream).ReadToEnd();
        var cancelRequest = JsonConvert.DeserializeObject<CancelRequest>(body);

        if (cancelRequest == null || string.IsNullOrEmpty(cancelRequest.FinancialReference))
            throw new ArgumentException("FinancialReference is required");

        var resultado = _sdk.CancelSale(cancelRequest.FinancialReference);

        Console.WriteLine("\n========================================");
        Console.WriteLine("SALE CANCELLATION COMPLETED");
        Console.WriteLine("========================================");
        Console.WriteLine($"Timestamp:       {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Financial Ref:   {cancelRequest.FinancialReference}");
        Console.WriteLine($"Response Code:   {resultado.CodigoRespuesta ?? "N/A"}");
        Console.WriteLine($"Status:          {(resultado.CodigoRespuesta == "00" ? "CANCELLED" : "FAILED")}");
        Console.WriteLine($"Message:         {resultado.Leyenda ?? "N/A"}");
        Console.WriteLine("========================================\n");

        return JsonConvert.SerializeObject(new
        {
            codigoRespuesta = resultado.CodigoRespuesta,
            leyenda = resultado.Leyenda,
            cancelled = resultado.CodigoRespuesta == "00"
        });
    }

    static string HandleCancelSaleCard(HttpListenerRequest request)
    {
        string body = new StreamReader(request.InputStream).ReadToEnd();
        var cancelRequest = JsonConvert.DeserializeObject<CancelCardRequest>(body);

        if (cancelRequest == null || cancelRequest.Amount <= 0 || string.IsNullOrEmpty(cancelRequest.FinancialReference))
            throw new ArgumentException("Amount and FinancialReference are required");

        var resultado = _sdk.CancelSaleCard(cancelRequest.Amount, cancelRequest.FinancialReference);

        Console.WriteLine("\n========================================");
        Console.WriteLine("SALE CANCELLATION (WITH CARD) COMPLETED");
        Console.WriteLine("========================================");
        Console.WriteLine($"Timestamp:       {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Amount:          ${cancelRequest.Amount:F2}");
        Console.WriteLine($"Financial Ref:   {cancelRequest.FinancialReference}");
        Console.WriteLine($"Response Code:   {resultado.CodigoRespuesta ?? "N/A"}");
        Console.WriteLine($"Status:          {(resultado.CodigoRespuesta == "00" ? "CANCELLED" : "FAILED")}");
        Console.WriteLine($"Message:         {resultado.Leyenda ?? "N/A"}");
        Console.WriteLine("========================================\n");

        return JsonConvert.SerializeObject(new
        {
            codigoRespuesta = resultado.CodigoRespuesta,
            leyenda = resultado.Leyenda,
            cancelled = resultado.CodigoRespuesta == "00"
        });
    }

    static string HandleCancelRefund(HttpListenerRequest request)
    {
        string body = new StreamReader(request.InputStream).ReadToEnd();
        var cancelRequest = JsonConvert.DeserializeObject<CancelRequest>(body);

        if (cancelRequest == null || string.IsNullOrEmpty(cancelRequest.FinancialReference))
            throw new ArgumentException("FinancialReference is required");

        var resultado = _sdk.CancelRefund(cancelRequest.FinancialReference);

        Console.WriteLine("\n========================================");
        Console.WriteLine("REFUND CANCELLATION COMPLETED");
        Console.WriteLine("========================================");
        Console.WriteLine($"Timestamp:       {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Financial Ref:   {cancelRequest.FinancialReference}");
        Console.WriteLine($"Response Code:   {resultado.CodigoRespuesta ?? "N/A"}");
        Console.WriteLine($"Status:          {(resultado.CodigoRespuesta == "00" ? "CANCELLED" : "FAILED")}");
        Console.WriteLine($"Message:         {resultado.Leyenda ?? "N/A"}");
        Console.WriteLine("========================================\n");

        return JsonConvert.SerializeObject(new
        {
            codigoRespuesta = resultado.CodigoRespuesta,
            leyenda = resultado.Leyenda,
            cancelled = resultado.CodigoRespuesta == "00"
        });
    }

    static string HandleCancelRefundCard(HttpListenerRequest request)
    {
        string body = new StreamReader(request.InputStream).ReadToEnd();
        var cancelRequest = JsonConvert.DeserializeObject<CancelCardRequest>(body);

        if (cancelRequest == null || cancelRequest.Amount <= 0 || string.IsNullOrEmpty(cancelRequest.FinancialReference))
            throw new ArgumentException("Amount and FinancialReference are required");

        var resultado = _sdk.CancelRefundCard(cancelRequest.Amount, cancelRequest.FinancialReference);

        Console.WriteLine("\n========================================");
        Console.WriteLine("REFUND CANCELLATION (WITH CARD) COMPLETED");
        Console.WriteLine("========================================");
        Console.WriteLine($"Timestamp:       {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Amount:          ${cancelRequest.Amount:F2}");
        Console.WriteLine($"Financial Ref:   {cancelRequest.FinancialReference}");
        Console.WriteLine($"Response Code:   {resultado.CodigoRespuesta ?? "N/A"}");
        Console.WriteLine($"Status:          {(resultado.CodigoRespuesta == "00" ? "CANCELLED" : "FAILED")}");
        Console.WriteLine($"Message:         {resultado.Leyenda ?? "N/A"}");
        Console.WriteLine("========================================\n");

        return JsonConvert.SerializeObject(new
        {
            codigoRespuesta = resultado.CodigoRespuesta,
            leyenda = resultado.Leyenda,
            cancelled = resultado.CodigoRespuesta == "00"
        });
    }

    static void LogTransactionToFile(string transactionType, decimal amount, string reference, dynamic resultado)
    {
        try
        {
            string logFileName = Path.Combine("logs", $"transactions_{DateTime.Now:yyyyMMdd}.log");
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}|{transactionType}|{amount:F2}|{reference}|{resultado.CodigoRespuesta ?? "N/A"}|{resultado.Leyenda ?? "N/A"}|{resultado.Autorizacion ?? "N/A"}|{resultado.ReferenciaFinanciera ?? "N/A"}";
            
            File.AppendAllText(logFileName, logEntry + Environment.NewLine);
            Console.WriteLine($"[LOG] Transaction saved to {logFileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOG ERROR] Failed to write to log file: {ex.Message}");
        }
    }

    static void StopApi()
    {
        _isRunning = false;
        _listener?.Stop();
        Console.WriteLine("Server stopped.");
    }

    static string DetectTerminalComPort()
    {
        Console.WriteLine("[INFO] Starting COM port auto-detection for terminal...");
        
        try
        {
            // Step 1: Find USB Serial Device Candidates using WMI
            var candidates = new System.Collections.Generic.List<string>();
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%COM%'");
            
            foreach (ManagementObject device in searcher.Get())
            {
                string caption = device["Caption"]?.ToString() ?? "";
                Console.WriteLine($"[DEBUG] Found device: {caption}");
                
                // Check if it's a USB Serial device
                string captionUpper = caption.ToUpper();
                if (captionUpper.Contains("USB") &&
                    (captionUpper.Contains("SERIAL") || captionUpper.Contains("SERIE")))
                {
                    // Extract COM port number
                    var match = Regex.Match(caption, @"COM(\d+)", RegexOptions.IgnoreCase);
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
                    var testPinPad = new EGlobal.TotalPosSDKNet.JPinPad.PinPad.PinPadUsb(portName, 10);
                    testPinPad.Open();
                    
                    if (testPinPad.IsOpened())
                    {
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
            
            // Step 3: No valid terminal found
            string errorMsg = candidates.Count > 0 
                ? $"Found {candidates.Count} USB Serial device(s) but none responded as a PAX terminal."
                : "No USB Serial devices detected. Please connect the PAX terminal via USB.";
            
            Console.WriteLine($"[ERROR] {errorMsg}");
            throw new InvalidOperationException(errorMsg);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] COM port detection failed: {ex.Message}");
            throw new InvalidOperationException("Failed to detect terminal COM port.", ex);
        }
    }

    class SaleRequest
    {
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public decimal? Cashback { get; set; }
        public string PromoCode { get; set; }
        public decimal? Points { get; set; }
    }

    class RefundRequest
    {
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public string FinancialReference { get; set; }
    }

    class CancelRequest
    {
        public string FinancialReference { get; set; }
    }

    class CancelCardRequest
    {
        public decimal Amount { get; set; }
        public string FinancialReference { get; set; }
    }
}
