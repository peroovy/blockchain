using System.IO;
using Core.Utils;

namespace Core;

public class Wallet
{
    public Wallet(string privateKey, string publicKey)
    {
        PrivateKey = privateKey;
        PublicKey = publicKey;
        PublicKeyHash = RsaUtils.HashPublicKey(PublicKey).ToHexDigest();
        Address = RsaUtils.GetAddressFromPublicKey(PublicKey);
    }
    
    public string PrivateKey { get; }

    public string PublicKey { get; }
    
    public string PublicKeyHash { get; }
    
    public string Address { get; }
    
    public static Wallet Load(string privateFilePath, string publicFilePath)
    {
        using var privateFile = File.Open(privateFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        using var privateFileReader = new StreamReader(privateFile);
        using var privateFileWriter = new StreamWriter(privateFile);
        
        using var publicFile = File.Open(publicFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        using var publicFileReader = new StreamReader(publicFile);
        using var publicFileWriter = new StreamWriter(publicFile);

        if (privateFile.Length == 0 || publicFile.Length == 0)
        {
            var keys = RsaUtils.GenerateRsaPair();

            privateFileWriter.Write(keys.privateKey);
            publicFileWriter.Write(keys.publicKey);
            
            return new Wallet(keys.privateKey, keys.publicKey);
        }

        var privateKey = privateFileReader.ReadToEnd();
        var publicKey = publicFileReader.ReadToEnd();

        return new Wallet(privateKey, publicKey);
    }
}