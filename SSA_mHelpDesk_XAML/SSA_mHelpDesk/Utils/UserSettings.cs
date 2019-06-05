using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SSA_mHelpDesk.Utils
{
    static class UserSettings
    {
        private static string dataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\SSA_mHelpDesk";
        private static readonly string accountInfoFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\SSA_mHelpDesk\\.accountInfo.dat";
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
                if (!Directory.Exists(dataPath))
                {
                    // Try to create the directory.
                    DirectoryInfo di = Directory.CreateDirectory(dataPath);

                }


                // load the account info from a file
                _accountInfo = EncryptedFileManager.ReadObject<AccountInfo>(accountInfoFile);
            }
            catch (Exception)
            {
                MessageBox.Show("Setting file does not exist." + dataPath, "SSA_mHelpDesk");
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
        public static bool AutoRefresh { get => Properties.Settings.Default.AutoRefresh; set => Properties.Settings.Default.AutoRefresh = value; }
        public static TimeSpan AutoRefreshPeriod { get => Properties.Settings.Default.AutoRefreshPeriod; set => Properties.Settings.Default.AutoRefreshPeriod = value; }
    }
}
