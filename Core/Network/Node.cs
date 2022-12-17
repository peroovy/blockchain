// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Net;
// using System.Net.Sockets;
// using System.Threading.Tasks;
// using Core.Repositories;
// using Core.Utils;
//
// namespace Core.Network;
//
// [Serializable]
// public enum Commands
// {
//     Version,
//     Blockchain
// }
//
// [Serializable]
// public class Version
// {
//     public Version(IPEndPoint addressFrom, int bestHeight)
//     {
//         AddressFrom = addressFrom;
//         BestHeight = bestHeight;
//     }
//     
//     public IPEndPoint AddressFrom { get; }
//     
//     public int BestHeight { get; }
// }
//
// public class Node
// {
//     private readonly TcpListener server;
//     private readonly TcpClient client;
//     private readonly Wallet wallet = Wallet.Load();
//     
//     private readonly IBlocksRepository blocksRepository;
//     private readonly IUtxosRepository utxosRepository;
//     
//     private static readonly IPEndPoint DnsAddress = new(IPAddress.Parse("127.0.0.1"), 8000);
//
//     public Node(IBlocksRepository blocksRepository, IUtxosRepository utxosRepository)
//     {
//         this.blocksRepository = blocksRepository;
//         this.utxosRepository = utxosRepository;
//         server = new TcpListener(IPAddress.Any, 0);
//         var address = (IPEndPoint)server.LocalEndpoint;
//         
//         client = new TcpClient(new IPEndPoint(IPAddress.Loopback, address.Port));
//     }
//
//     public void Run()
//     {
//         var blockchain = FindHighestBlockchain();
//         if (blockchain is not null)
//         {
//             blocksRepository.Rebase(blockchain);
//             utxosRepository.Reindex(blockchain);
//         }
//         
//         server.Start();
//         while (true)
//         {
//             var tcpClient = server.AcceptTcpClient();
//
//             Task.Run(() => HandleClient(tcpClient));
//         }
//     }
//
//     private void HandleClient(TcpClient tcpClient)
//     {
//         var stream = tcpClient.GetStream();
//         
//         var data = GetDataFromStream(stream);
//         var command = Serializer.FromBytes<Commands>(data);
//
//         var response = GetResponse(command);
//         stream.Write(response, 0, response.Length);
//         stream.Flush();
//     }
//
//     private byte[] GetResponse(Commands command)
//     {
//         switch (command)
//         {
//             case Commands.Version:
//             {
//                 var height = blocksRepository.GetHeight();
//                 var version = new Version((IPEndPoint)server.Server.RemoteEndPoint, height);
//
//                 return Serializer.ToBytes(version);
//             }
//
//             case Commands.Blockchain:
//             {
//                 var blocks = blocksRepository
//                     .GetAll()
//                     .ToArray();
//
//                 return Serializer.ToBytes(blocks);
//             }
//                 
//             default:
//                 throw new ArgumentOutOfRangeException();
//         }
//     }
//
//     private IEnumerable<Block> FindHighestBlockchain()
//     {
//         var height = blocksRepository.GetHeight();
//
//         var version = GetAnotherNodes()
//             .Select(point => MakeRequest(point, Serializer.ToBytes(Commands.Version)))
//             .Select(Serializer.FromBytes<Version>)
//             .OrderBy(v => v.BestHeight)
//             .FirstOrDefault();
//
//         if (version is null || height >= version.BestHeight)
//             return null;
//
//         return GetBlockchainFrom(version.AddressFrom);
//     }
//
//     private IEnumerable<Block> GetBlockchainFrom(IPEndPoint address)
//     {
//         var data = MakeRequest(address, Serializer.ToBytes(Commands.Blockchain));
//
//         return Serializer.FromBytes<Block[]>(data);
//     }
//     
//     private IPEndPoint[] GetAnotherNodes()
//     {
//         var bytes = MakeRequest(DnsAddress, Array.Empty<byte>());
//
//         return Serializer.FromBytes<IPEndPoint[]>(bytes);
//     }
//
//     private byte[] MakeRequest(IPEndPoint endPoint, byte[] data)
//     {
//         var stream = client.GetStream();
//         
//         stream.Write(data, 0, data.Length);
//         
//         client.Connect(endPoint);
//
//         return GetDataFromStream(stream);
//     }
//
//     private static byte[] GetDataFromStream(Stream stream)
//     {
//         var bytes = new List<byte>();
//
//         int b;
//         while ((b = stream.ReadByte()) != -1)
//             bytes.Add((byte)b);
//
//         return bytes.ToArray();
//     }
// }