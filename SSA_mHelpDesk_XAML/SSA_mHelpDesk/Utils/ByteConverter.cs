using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    }
}
