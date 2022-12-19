using System.Collections.Concurrent;
using System.Net;

namespace Core.Network;

internal static class ConcurrentDictionaryExtensions
{
    public static void AddAddress(this ConcurrentDictionary<IPEndPoint, bool> addresses, IPEndPoint address)
    {
        addresses[address] = true;
    }
}