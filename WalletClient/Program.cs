using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Core;
using Core.Utils;

namespace WalletClient;

internal class Program
{
    private static readonly IPEndPoint DnsAddress = new(IPAddress.Parse("127.0.0.1"), 8000);
        
    public static void Main(string[] args)
    {
        var server = new TcpListener(IPAddress.Any, 0);
        var address = (IPEndPoint)server.LocalEndpoint;
        var client = new TcpClient(new IPEndPoint(IPAddress.Loopback, address.Port));
            
        client.Connect(DnsAddress);

        var stream = client.GetStream();
        var bytes = new List<byte>();

        int b;
        while ((b = stream.ReadByte()) != -1)
            bytes.Add((byte)b);

        foreach (var endPoint in Serializer.FromBytes<EndPoint[]>(bytes.ToArray()))
            Console.WriteLine(endPoint);
    }
}

    

public class Node
{
    private readonly TcpListener server;
    private readonly TcpClient client;
    
    private readonly Wallet wallet = Wallet.Load();

    public Node()
    {
        server = new TcpListener(IPAddress.Any, 0);
        client = new TcpClient((IPEndPoint)server.LocalEndpoint);
    }
}
