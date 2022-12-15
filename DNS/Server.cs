﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Core.Utils;
using Microsoft.Extensions.Logging;

namespace DNS
{
    internal class Server
    {
        private static readonly ConcurrentDictionary<EndPoint, long> Addresses = new();
        private static readonly Logger<Server> Logger = new(LoggerFactory.Create(builder => builder.AddConsole()));

        private const long Ttl = TimeSpan.TicksPerMinute * 5;

        public static void Main(string[] args)
        {
            var listener = new TcpListener(IPAddress.Any, 8000);
            listener.Start();
            Logger.LogInformation($"Start server on {listener.LocalEndpoint}");

            while (true)
            {
                var clientSocket = listener.AcceptTcpClient();
                
                Task.Run(() => HandleClient(clientSocket));
            }
        }

        private static void HandleClient(TcpClient clientSocket)
        {
            var address = clientSocket.Client.RemoteEndPoint;
            Logger.LogInformation($"Handle peer {address}");
            
            using var input = clientSocket.GetStream();
            using var output = clientSocket.GetStream();
            
            var now = DateTime.Now.Ticks;
            Addresses[address] = now;
            
            var addresses = Addresses
                .Where(pair => pair.Key != address && pair.Value + Ttl > now)
                .ToArray();
            var serializedAddresses = Serializer.ToBytes(addresses);
            
            output.Write(serializedAddresses, 0, serializedAddresses.Length);
        }
    }
}