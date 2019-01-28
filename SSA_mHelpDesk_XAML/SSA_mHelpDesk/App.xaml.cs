using SSA_mHelpDesk.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SSA_mHelpDesk
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            base.OnStartup(e);

            if (UserSettings.ApiKey == null || UserSettings.ApiSecret == null)
            {
                SettingsWindow settingsWindow = new SettingsWindow()
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                };
                settingsWindow.ShowDialog();

                if (settingsWindow.DialogResult != true)
                    Shutdown(1);
            }
        }
    }
}
