using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace FinancialScrapper
{
    public class FileServer
    {
        private readonly HttpListener _listener;
        private readonly string _filePath;

        public FileServer(string urlPrefix, string filePath)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(urlPrefix);
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public void Start()
        {
            _listener.Start(); // exception
            Task.Run(HandleRequests);
        }

        public void Stop()
        {
            Dispose();
            _listener.Stop();
        }

        private async Task HandleRequests()
        {
            while (_listener.IsListening)
            {
                var context = await _listener.GetContextAsync();
                ProcessRequest(context);
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            if (request.HttpMethod != "GET" || request.RawUrl != "/download")
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close();
                return;
            }

            if (!File.Exists(_filePath))
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Close();
                return;
            }

            SendFile(response);
        }

        private void SendFile(HttpListenerResponse response)
        {
            response.ContentType = "text/csv";
            response.ContentLength64 = new FileInfo(_filePath).Length;

            using var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fileStream.CopyTo(response.OutputStream);
            fileStream.Close();
            response.OutputStream.Close();
        }

        private void Dispose()
        {
            _listener.Close();
        }

    }
}