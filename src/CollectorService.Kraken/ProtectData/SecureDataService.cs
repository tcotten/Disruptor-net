using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProtectData;

public static class SecureDataService
{
    public static Tuple<string, string> GetKrakenKeys(Func<string, object> getObjectResourcesCall)
    {
        string machineName = Environment.MachineName;
        string keyString = String.Empty;
        if (machineName == "DESKTOP-EEHD2NV")
        {
            keyString = "1";
        }
        else if (machineName == "Orion")
        {
            keyString = "2";
        }
        string keyName = "LocalClient" + keyString;
        byte[] encryptedData = GetByteArrayResource(keyName, getObjectResourcesCall);
        byte[] entropyData = GetByteArrayResource(keyName + "Key", getObjectResourcesCall);
        byte[] binData = UnProtect(encryptedData, entropyData);
        var keys = Encoding.ASCII.GetString(binData).Split('|');
        return new Tuple<string, string>(keys[0], keys[1]);
    }
    private static byte[] GetByteArrayResource(string resourceName, Func<string, object> getObjectResourcesCall)
    {
        var edObj = getObjectResourcesCall(resourceName);
        //var edObj = Properties.Resources.ResourceManager.GetObject(resourceName);
        byte[] encryptedData;
        if (edObj != null)
        {
            encryptedData = (byte[])edObj;
            return encryptedData;
        }
        return Array.Empty<byte>();
    }
    //private static byte[] GetByteArrayResource(string resourceName)
    //{
    //    var edObj = Properties.Resources.ResourceManager.GetObject(resourceName);
    //    byte[] encryptedData;
    //    if (edObj != null)
    //    {
    //        encryptedData = (byte[])edObj;
    //        return encryptedData;
    //    }
    //    return Array.Empty<byte>();
    //}
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "This will only be run on windows.")]
    private static byte[] UnProtect(byte[] encryptedData, byte[] entropyData)
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

}
