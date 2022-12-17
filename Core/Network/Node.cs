using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using Core.Utils;

namespace Core.Network;

public class Node
{
    private readonly TcpListener server;
    private readonly TcpClient client;
    
    private static readonly IPEndPoint DnsEndPoint = new(
        IPAddress.Parse(ConfigurationManager.AppSettings["DnsAddress"]),
        Convert.ToInt32(ConfigurationManager.AppSettings["DnsPort"]));

    public Node()
    {
        server = new TcpListener(IPAddress.Any, 0);
        var port = ((IPEndPoint)server.LocalEndpoint).Port;

        client = new TcpClient(new IPEndPoint(IPAddress.Any, port));
    }

    public void Run()
    {
        server.Start();
        foreach (var endPoint in GetAnotherNodes())
        {
            Console.WriteLine(endPoint);
        }
    }

    private EndPoint[] GetAnotherNodes()
    {
        var data = MakeRequest(DnsEndPoint, Serializer.ToBytes(DnsCommands.Connect));

        return Serializer.FromBytes<EndPoint[]>(data);
    }

    private byte[] MakeRequest(IPEndPoint point, byte[] data)
    {
        client.Connect(point);
        using var stream = client.GetStream();
        
        stream.Write(data, 0, data.Length);
        stream.Flush();

        var receiveData = new byte[client.ReceiveBufferSize];
        stream.Read(receiveData, 0, receiveData.Length);
        
        return receiveData;
    }
}