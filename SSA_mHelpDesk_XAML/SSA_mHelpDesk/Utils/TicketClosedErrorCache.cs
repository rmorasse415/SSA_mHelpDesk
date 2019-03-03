using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSA_mHelpDesk.Utils
{
    class TicketClosedErrorCache
    {
        private readonly String filename;
        private readonly Dictionary<String, Boolean> closedErrorCache = null;

        private static TicketClosedErrorCache mSingleton;
        public static TicketClosedErrorCache GetInstance()
        {
            return mSingleton;
        }

        // This is a slightly unusual instance of a singleton because it
        // needs to be initialized once with parameters before use
        public static void InitInstance(string fileName)
        {
            if (mSingleton != null)
            {
                throw new NotSupportedException("You cannot initialize this class more than once");
            }

            mSingleton = new TicketClosedErrorCache(fileName);
        }

        private TicketClosedErrorCache(string fileName)
        {
            this.filename = fileName;
            try
            {
                closedErrorCache = ByteConverter.CreateFromBytes<Dictionary<String, Boolean>>(File.ReadAllBytes(filename));
            }
            catch
            {
                closedErrorCache = new Dictionary<string, bool>();
            }
        }

        public void Save()
        {
            ByteConverter.WriteAllBytesToHiddenFile(filename, ByteConverter.ConvertToBytes(closedErrorCache));

            // Set file as hidden
            File.SetAttributes(filename, File.GetAttributes(filename) | FileAttributes.Hidden);
        }

        public void MarkAsChecked(string ticketId, bool closedError)
        {
            closedErrorCache.Add(ticketId, closedError);
        }

        public bool IsErrorCheckNeeded(string ticketId)
        {
            //If the key was not checked or if there was a close error then a check is needed
            if (!closedErrorCache.ContainsKey(ticketId) || closedErrorCache[ticketId])
            {
                return true;
            }

            return false;
        }
    }
}
