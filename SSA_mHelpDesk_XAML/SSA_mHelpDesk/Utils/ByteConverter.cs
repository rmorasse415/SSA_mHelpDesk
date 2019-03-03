using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSA_mHelpDesk.Utils
{
    static class ByteConverter
    {
        public static byte[] ConvertToBytes(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(json);
        }

        public static T CreateFromBytes<T>(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            T ret = JsonConvert.DeserializeObject<T>(json);

            return ret;
        }

        public static void WriteAllBytesToHiddenFile(String fileLocation, byte[] fileData)
        {
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
        }
    }
}
