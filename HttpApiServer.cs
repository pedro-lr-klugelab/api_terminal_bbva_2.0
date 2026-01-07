using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EGlobal.TotalPosSDKNet.Interfaz.Authorizer;
using EGlobal.TotalPosSDKNet.Interfaz.Catalog;

namespace SimpleConnectionTest
{
    public class HttpApiServer
    {
        private HttpListener listener;
        private bool isRunning = false;

        public void Start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/");
            listener.Start();
            isRunning = true;

            Console.WriteLine("==============================================");
            Console.WriteLine(" BBVA Terminal API - Running on port 5000");
            Console.WriteLine("==============================================");
            Console.WriteLine("Endpoints:");
            Console.WriteLine("  POST /api/initialize");
            Console.WriteLine("  POST /api/sale");
            Console.WriteLine("  GET  /api/status");
            Console.WriteLine("==============================================\n");

            Task.Run(() => ListenLoop());
        }

        private async void ListenLoop()
        {
            while (isRunning)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    Task.Run(() => HandleRequest(context));
                }
                catch (Exception ex)
                {
                    if (isRunning) Console.WriteLine($"[ERROR] {ex.Message}");
                }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            response.AddHeader("Access-Control-Allow-Origin", "*");
            response.ContentType = "application/json";

            string result = "{}";
            try
            {
                string path = request.Url.AbsolutePath.ToLower();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {request.HttpMethod} {path}");

                if (path == "/api/initialize" && request.HttpMethod == "POST")
                {
                    result = Program.ApiInitialize();
                }
                else if (path == "/api/sale" && request.HttpMethod == "POST")
                {
                    string body = new StreamReader(request.InputStream).ReadToEnd();
                    result = Program.ApiSale(body);
                }
                else if (path == "/api/status" && request.HttpMethod == "GET")
                {
                    result = "{\"status\":\"running\",\"initialized\":" + Program.IsInitialized.ToString().ToLower() + "}";
                }
                else
                {
                    response.StatusCode = 404;
                    result = "{\"error\":\"Not Found\"}";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                result = "{\"error\":\"" + ex.Message.Replace("\"", "'") + "\"}";
                Console.WriteLine($"[ERROR] {ex.Message}");
            }

            byte[] buffer = Encoding.UTF8.GetBytes(result);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }

        public void Stop()
        {
            isRunning = false;
            listener?.Stop();
        }
    }
}
