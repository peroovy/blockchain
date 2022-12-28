using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Core.Utils;

namespace Core.Network;

public abstract class P2PNode
{
    protected readonly IPEndPoint AddressFrom;
    
    private readonly TcpListener listener;
    private readonly TimeSpan connectionTimeout = TimeSpan.FromSeconds(5);

    protected P2PNode(IPAddress address, int port)
    {
        AddressFrom = new IPEndPoint(address, port);
        listener = new TcpListener(new IPEndPoint(IPAddress.Parse("0.0.0.0"), port));
    }

    public virtual void Run()
    {
        var listenerThread = new Thread(() =>
        {
            listener.Start();

            while (true)
            {
                var node = listener.AcceptTcpClient();

                Task.Run(() =>
                {
                    var package = Receive(node);
                    
                    HandlePackage(package);
                });
            }
        });
        listenerThread.Start();
    }

    protected abstract void HandlePackage(Package package);

    protected bool TrySend(IPEndPoint remoteEndPoint, Package package)
    {
        var data = Serializer.ToBytes(package);
        var messageLength = BitConverter.GetBytes(data.Length);

        using var client = new TcpClient();

        client.BeginConnect(remoteEndPoint.Address, remoteEndPoint.Port, null, null)
            .AsyncWaitHandle
            .WaitOne(connectionTimeout, true);

        if (!client.Connected)
            return false;

        using var stream = client.GetStream();
        stream.Write(messageLength, 0, messageLength.Length);
        stream.Write(data, 0, data.Length);

        return true;
    }
    
    private Package Receive(TcpClient node)
    {
        using var stream = node.GetStream();

        var messageLength = new byte[4];
        stream.Read(messageLength, 0, messageLength.Length);
        var length = BitConverter.ToInt32(messageLength, 0);
        
        using var memoryStream = new MemoryStream();
        var buffer = new byte[512];
        var total = 0;
        do
        {
            var amount = stream.Read(buffer, 0, buffer.Length);
            memoryStream.Write(buffer, 0, amount);
            total += amount;
        } while (total < length);
        
        return Serializer.FromBytes<Package>(memoryStream.GetBuffer());
    }
}