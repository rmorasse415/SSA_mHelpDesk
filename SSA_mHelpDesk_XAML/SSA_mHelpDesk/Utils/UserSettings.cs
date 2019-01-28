using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SSA_mHelpDesk.Utils
{
    static class UserSettings
    {
        private static readonly string accountInfoFile = ".accountInfo.dat";
        private static AccountInfo _accountInfo;

        struct AccountInfo
        {
            public string apiKey;
            public string apiSecret;
            public string portalId;
        }

        static UserSettings()
        {
            try
            {
                // load the account info from a file
                _accountInfo = EncryptedFileManager.ReadObject<AccountInfo>(accountInfoFile);
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        public static void Save()
        {
            // Save account keys
            EncryptedFileManager.WriteObject(_accountInfo, accountInfoFile);

            // Set account file as hidden
            File.SetAttributes(accountInfoFile, File.GetAttributes(accountInfoFile) | FileAttributes.Hidden);

            // Save other properties
            Properties.Settings.Default.Save();
        }

        public static string ApiKey { get => _accountInfo.apiKey; set => _accountInfo.apiKey = value; }
        public static string ApiSecret { get => _accountInfo.apiSecret; set => _accountInfo.apiSecret = value; }
        public static string PortalId { get => _accountInfo.portalId; set => _accountInfo.portalId = value; }
        public static bool Production { get => Properties.Settings.Default.Production; set => Properties.Settings.Default.Production = value; }
        public static bool Bearer_Workaround { get => Properties.Settings.Default.Bearer_Workaround; set => Properties.Settings.Default.Bearer_Workaround = value; }
    }
}
