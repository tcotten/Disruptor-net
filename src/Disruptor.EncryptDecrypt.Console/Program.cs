using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Disruptor.EncryptDecrypt;

public class Program
{
    public static void Main(string[] args)
    {
        byte[] entropyData = CreateRandomEntropy();
        string entropyFileName = ".keyconfig";
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
                string entropyFilePath = Path.Combine(Path.GetDirectoryName(filePath) ?? "", entropyFileName);
                File.WriteAllBytes(entropyFilePath, entropyData);
                Console.WriteLine("Done writing files.");
                Console.WriteLine("Reading them back in.");
                byte[] entropyDataReLoad = File.ReadAllBytes(entropyFilePath);
                if (entropyData.SequenceEqual(entropyDataReLoad))
                {
                    Console.WriteLine("Entropy data matches");
                }
                byte[] protectedDataReLoad = UnProtect(File.ReadAllBytes(filePath), entropyDataReLoad);
                if (keyToProtect == Encoding.ASCII.GetString(protectedDataReLoad))
                {
                    Console.WriteLine("Decrypt of key successful from files.");
                }
            }
        }
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
}
