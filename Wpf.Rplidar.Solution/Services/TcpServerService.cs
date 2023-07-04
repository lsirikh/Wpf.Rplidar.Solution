using Caliburn.Micro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Wpf.Rplidar.Solution.Models;

namespace Wpf.Rplidar.Solution.Services
{
    public class TcpServerService
    {
        private TcpListener _server;
        private TcpClient _client;
        private NetworkStream _stream;

        public delegate void EventDele(string msg, string status);
        public event EventDele Message;

        public TcpServerService()
        {
            
        }
       
        public async void TcpInitialize(string ipAddress, int port)
        {
            _server = new TcpListener(IPAddress.Parse(ipAddress), port);
            await StartAsync();
        }

        public async Task StartAsync()
        {
            try
            {
                _server.Start();
                Message?.Invoke("Server started...", "서버 작동 시작");

                _client = await _server.AcceptTcpClientAsync();

                Message?.Invoke("Client was connected", "클라이언트 접속");
                _stream = _client.GetStream();
                await SendAsync("Welcom!");
            }
            catch
            {
            }
            
        }

        public async Task<string> ReceiveAsync()
        {
            if (_client != null && _client.Connected)
            {
                var reader = new StreamReader(_stream, Encoding.UTF8);
                var message = await reader.ReadLineAsync();
                return message;
            }

            return null;
        }

        public async Task SendAsync(string message)
        {
            if (_client != null && _client.Connected)
            {
                var writer = new StreamWriter(_stream, Encoding.UTF8);
                await writer.WriteLineAsync(message);
                await writer.FlushAsync();
            }
        }

        public async void SendAsync(LidarDataModel model)
        {
            if (_client != null && _client.Connected)
            {
                var writer = new StreamWriter(_stream, Encoding.UTF8);
                string json = JsonConvert.SerializeObject(model);
                await writer.WriteLineAsync(json);
                await writer.FlushAsync();
            }
        }

        public void Stop()
        {
            _stream?.Close();
            _client?.Close();
            _server?.Stop();
            Message?.Invoke("Server was terminated!", "서버 종료");
        }


    }
}
