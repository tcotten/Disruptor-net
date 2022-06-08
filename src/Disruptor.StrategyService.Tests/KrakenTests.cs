using CryptoExchange.Net.Authentication;
using Kraken.Net.Clients;
using Kraken.Net.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace Disruptor.StrategyService.Tests;

[TestClass]
public class KrakenTests
{
    private byte[] entropyData;

    public KrakenTests()
    {
        entropyData = CreateRandomEntropy();
    }
    [TestMethod]
    public void KrakenConnectTest()
    {
        var kBins = GetKrakenKeys(@"C:\Users\tcott\OneDrive\Apps\gunbot\Disruptor-net\LocalClient.bin");
        var krakenClient = new KrakenClient(new KrakenClientOptions()
        {
            ApiCredentials = new ApiCredentials(kBins.Item1, kBins.Item2),
            RequestTimeout = TimeSpan.FromSeconds(10)
        });
        Assert.IsNotNull(krakenClient);
        var tradeHistoryData = krakenClient.SpotApi.Trading.GetUserTradesAsync().GetAwaiter().GetResult();
        Assert.IsNotNull(tradeHistoryData);
        foreach (var trade in tradeHistoryData.Data.Trades)
        {
            // TODO: Write any new history to Kafka
            Assert.IsNotNull(trade);
        }
    }
    [TestMethod]
    public void ProtectDataTest()
    {
        string dataToProtect = "This is data secret to be protected.";
        byte[] data = Encoding.UTF8.GetBytes(dataToProtect);
        byte[] protectedData = Protect(data, entropyData);
        Assert.AreNotEqual(data, protectedData);
        byte[] unprotectedDataBytes = UnProtect(protectedData, entropyData);
        string unprotectedData = Encoding.UTF8.GetString(unprotectedDataBytes);
        Assert.AreEqual(dataToProtect, unprotectedData);
    }

    public void ManualProtectDataToFile()
    {
        Console.WriteLine("Enter the key to encrypt: ");
        string? keyToProtect = Console.ReadLine();
        if (!String.IsNullOrEmpty(keyToProtect))
        {
            byte[] data = Encoding.UTF8.GetBytes(keyToProtect);
            byte[] protectedData = Protect(data, entropyData);
            Console.WriteLine("Enter the file to write the encrypted data too: ");
            string? filePath = Console.ReadLine();
            if (!String.IsNullOrEmpty(filePath))
            {
                File.WriteAllBytes(filePath, protectedData);
            }
        }

    }
    private static Tuple<string,string> GetKrakenKeys(string filePath)
    {
        byte[] eData;
        byte[] protectedData;
        if (File.Exists(filePath))
        {
            eData = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(filePath) ?? "", ".keyconfig"));
            protectedData = File.ReadAllBytes(filePath);
            byte[] binData = UnProtect(protectedData, eData);
            var keys = Encoding.ASCII.GetString(binData).Split('|');
            return new Tuple<string,string>(keys[0], keys[1]);
        }
        return new Tuple<string, string>("", "");
    }
    //retrieve password from the windows vault store using Credential Manager   
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "This will only be run on windows.")]
    public static byte[] Protect(byte[] data, byte[] entropyData)
    {
        try
        {
            // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
            // only by the same current user.
            return ProtectedData.Protect(data, entropyData, DataProtectionScope.LocalMachine);
        }
        catch (CryptographicException e)
        {
            Console.WriteLine("Data was not encrypted. An error occurred.");
            Console.WriteLine(e.ToString());
            return new byte[data.Length];
        }
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "This will only be run on windows.")]
    public static byte[] UnProtect(byte[] encryptedData, byte[] entropyData)
    {
        try
        {
            //Decrypt the data using DataProtectionScope.CurrentUser.
            return ProtectedData.Unprotect(encryptedData, entropyData, DataProtectionScope.LocalMachine);
        }
        catch (CryptographicException e)
        {
            Console.WriteLine("Data was not decrypted. An error occurred.");
            Console.WriteLine(e.ToString());
            return new byte[encryptedData.Length];
        }
    }
    public static byte[] CreateRandomEntropy()
    {
        // Create a byte array to hold the random value.
        byte[] entropy = new byte[16];

        // Create a new instance of the RNGCryptoServiceProvider.
        // Fill the array with a random value.
        RandomNumberGenerator.Fill(entropy);
        //new RNGCryptoServiceProvider().GetBytes(entropy);

        // Return the array.
        return entropy;
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "This will only be run on windows.")]
    public static int EncryptDataToStream(byte[] Buffer, byte[] Entropy, DataProtectionScope Scope, Stream S)
    {
        if (Buffer == null)
            throw new ArgumentNullException(nameof(Buffer));
        if (Buffer.Length <= 0)
            throw new ArgumentException("The buffer length was 0.", nameof(Buffer));
        if (Entropy == null)
            throw new ArgumentNullException(nameof(Entropy));
        if (Entropy.Length <= 0)
            throw new ArgumentException("The entropy length was 0.", nameof(Entropy));
        if (S == null)
            throw new ArgumentNullException(nameof(S));

        int length = 0;

        // Encrypt the data and store the result in a new byte array. The original data remains unchanged.
        byte[] encryptedData = ProtectedData.Protect(Buffer, Entropy, Scope);

        // Write the encrypted data to a stream.
        if (S.CanWrite && encryptedData != null)
        {
            S.Write(encryptedData, 0, encryptedData.Length);

            length = encryptedData.Length;
        }

        // Return the length that was written to the stream.
        return length;
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "This will only be run on windows.")]

    public static byte[] DecryptDataFromStream(byte[] Entropy, DataProtectionScope Scope, Stream S, int Length)
    {
        if (S == null)
            throw new ArgumentNullException(nameof(S));
        if (Length <= 0)
            throw new ArgumentException("The given length was 0.", nameof(Length));
        if (Entropy == null)
            throw new ArgumentNullException(nameof(Entropy));
        if (Entropy.Length <= 0)
            throw new ArgumentException("The entropy length was 0.", nameof(Entropy));

        byte[] inBuffer = new byte[Length];
        byte[] outBuffer;

        // Read the encrypted data from a stream.
        if (S.CanRead)
        {
            S.Read(inBuffer, 0, Length);

            outBuffer = ProtectedData.Unprotect(inBuffer, Entropy, Scope);
        }
        else
        {
            throw new IOException("Could not read the stream.");
        }

        // Return the decrypted data
        return outBuffer;
    }
}
