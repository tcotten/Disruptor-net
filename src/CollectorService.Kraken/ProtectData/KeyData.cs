//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks;

//namespace ProtectData;

//public static class KeyData
//{
//    private static Tuple<string, string> GetKrakenKeys(string filePath)
//    {
//        byte[] eData;
//        byte[] protectedData;
//        if (File.Exists(filePath))
//        {
//            eData = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(filePath) ?? "", ".keyconfig"));
//            protectedData = File.ReadAllBytes(filePath);
//            byte[] binData = UnProtect(protectedData, eData);
//            var keys = Encoding.ASCII.GetString(binData).Split('|');
//            return new Tuple<string, string>(keys[0], keys[1]);
//        }
//        return new Tuple<string, string>("", "");
//    }

//    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "This will only be run on windows.")]
//    public static byte[] Protect(byte[] data, byte[] entropyData)
//    {
//        try
//        {
//            // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
//            // only by the same current user.
//            return ProtectedData.Protect(data, entropyData, DataProtectionScope.LocalMachine);
//        }
//        catch (CryptographicException e)
//        {
//            Console.WriteLine("Data was not encrypted. An error occurred.");
//            Console.WriteLine(e.ToString());
//            return new byte[data.Length];
//        }
//    }
//    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "This will only be run on windows.")]
//    public static byte[] UnProtect(byte[] encryptedData, byte[] entropyData)
//    {
//        try
//        {
//            //Decrypt the data using DataProtectionScope.CurrentUser.
//            return ProtectedData.Unprotect(encryptedData, entropyData, DataProtectionScope.LocalMachine);
//        }
//        catch (CryptographicException e)
//        {
//            Console.WriteLine("Data was not decrypted. An error occurred.");
//            Console.WriteLine(e.ToString());
//            return new byte[encryptedData.Length];
//        }
//    }
//    public static byte[] CreateRandomEntropy()
//    {
//        // Create a byte array to hold the random value.
//        byte[] entropy = new byte[16];

//        // Create a new instance of the RNGCryptoServiceProvider.
//        // Fill the array with a random value.
//        RandomNumberGenerator.Fill(entropy);
//        //new RNGCryptoServiceProvider().GetBytes(entropy);

//        // Return the array.
//        return entropy;
//    }

//}
