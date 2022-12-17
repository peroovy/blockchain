using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Core.Utils;
using Microsoft.Extensions.Logging;
using System.Configuration;
using Core;
using Core.Network;

namespace DNS;

internal class Server
{
    private static readonly ConcurrentDictionary<EndPoint, long> Addresses = new();
    private static readonly Logger<Server> Logger = new(LoggerFactory.Create(builder => builder.AddConsole()));

    public static void Main(string[] args)
    {
        var port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
        var listener = new TcpListener(IPAddress.Any, port);

        listener.Start();
        Logger.LogInformation($"Listen {listener.LocalEndpoint}");

        while (true)
        {
            var clientSocket = listener.AcceptTcpClient();

            Task.Run(() => HandleClient(clientSocket));
        }
    }
    
    private static void HandleClient(TcpClient client)
    {
        var address = client.Client.RemoteEndPoint;

        using var stream = client.GetStream();
        var receiveData = new byte[client.ReceiveBufferSize];
        stream.Read(receiveData, 0, receiveData.Length);
        
        var command = Serializer.FromBytes<DnsCommands>(receiveData);

        switch (command)
        {
            case DnsCommands.Connect:
                HandleConnect(stream, address);
                break;
            
            case DnsCommands.Disconnect:
                HandleDisconnect(address);
                break;
            
            default:
                throw new ArgumentOutOfRangeException($"Unknown command {command}");
        }
    }

    private static void HandleConnect(NetworkStream stream, EndPoint address)
    {
        Logger.LogInformation($"Connect node: {address}");

        var now = DateTime.Now.Ticks;
        Addresses[address] = now;
            
        var addresses = Addresses
            .Where(pair => pair.Key != address)
            .Select(pair => pair.Key)
            .ToArray();
            
        var serializedAddresses = Serializer.ToBytes(addresses);

        stream.Write(serializedAddresses, 0, serializedAddresses.Length);
        stream.Flush();
    }

    private static void HandleDisconnect(EndPoint address)
    {
        if (Addresses.TryRemove(address, out _))
            Logger.LogInformation($"Disconnect node {address}");
    }
}