using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SSA_mHelpDesk.Utils
{
    static class EncryptedFileManager
    {
        private static readonly int bytesOfEntropy = 20;

        public static void WriteData(byte[] data, string fileLocation)
        {
            // Generate additional entropy (will be used as the Initialization vector)
            byte[] entropy = new byte[bytesOfEntropy];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(entropy);
            }

            byte[] ciphertext = ProtectedData.Protect(data, entropy, DataProtectionScope.CurrentUser);

            List<byte> fileData = new List<byte>();
            fileData.AddRange(entropy.Take(bytesOfEntropy));
            fileData.AddRange(ciphertext);

            //N.B. Can't use File.WriteAllBytes when file is hidden
            using (FileStream fs = new FileStream(fileLocation, FileMode.OpenOrCreate))
            {
                using (BinaryWriter w = new BinaryWriter(fs, Encoding.UTF8, true))
                {
                    // Write your data here...
                    w.Write(fileData.ToArray());
                }
                // Set the stream length to the current position in order to truncate leftover text
                fs.SetLength(fs.Position);
            }

            //File.WriteAllBytes(fileLocation, fileData.ToArray());
        }

        public static byte[] ReadData(string fileLocation)
        {
            //throw exception on error
            byte[] fileData = File.ReadAllBytes(fileLocation);
            byte[] entropy = fileData.Take(bytesOfEntropy).ToArray(); //First bytesOfEntropy bytes
            byte[] ciphertext = fileData.Skip(bytesOfEntropy).ToArray(); //bytes bytesOfEntropy-end

            byte[] data = ProtectedData.Unprotect(ciphertext,
                    entropy,
                    DataProtectionScope.CurrentUser);

            return data;
        }

        public static void WriteObject(object obj, string fileLocation)
        {
            byte[] data = ByteConverter.ConvertToBytes(obj);
            WriteData(data, fileLocation);
        }

        public static T ReadObject<T>(string fileLocation)
        {
            byte[] data = ReadData(fileLocation);
            T ret = ByteConverter.CreateFromBytes<T>(data);

            return ret;
        }
    }
}
