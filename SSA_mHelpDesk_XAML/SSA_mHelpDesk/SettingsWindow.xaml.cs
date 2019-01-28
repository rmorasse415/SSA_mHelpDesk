using SSA_mHelpDesk.API;
using SSA_mHelpDesk.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SSA_mHelpDesk
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly AuthenticationManager mAuthManager = AuthenticationManager.Instance;

        private static Color sKeyValidColor = Color.FromArgb(0xFF, 0x70, 0xE0, 0x70);
        private static Color sKeyNotValidColor = Color.FromArgb(0xFF, 0xE0, 0x70, 0x70);

        public SolidColorBrush KeyIndicatorColor { get; set; }

        private AuthenticationManager.AuthenticationInfo AuthInfo { get; set; } = null;
        private bool RefreshingAuthInfo { get; set; } = true;

        private string initApiKey;
        private string initApiSecret;

        public SettingsWindow()
        {
            InitializeComponent();

            //PortalId.GetBindingExpression(TextBox.TextProperty).ValidateWithoutUpdate();

            DataContext = this;
        }

        //used for binding purposes for verification
        private string _apiKeyString;
        public string ApiKeyString
        {
            get => _apiKeyString;
            set {
                _apiKeyString = value;
                SyncAccessTokenState();
            }
        }
        public string SecretString { get; set; }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Button_Click_Save(object sender, RoutedEventArgs e)
        {
            if (Validation.GetHasError(ApiKey) ||
                Validation.GetHasError(Secret) ||
                Validation.GetHasError(PortalId))

            {
                ScrollViewer.ScrollToTop();
                return;
            }

            if (!mAuthManager.IsAuthValid(AuthInfo))
            {
                // The user is trying to save with an invalid AuthInfo
                
                // TODO Verify user wants to save
            }

            UserSettings.ApiKey = ApiKey.Password;
            UserSettings.ApiSecret = Secret.Password;
            UserSettings.PortalId = PortalId.Text;
            UserSettings.Production = (AccountType.SelectedIndex == 0);
            UserSettings.Bearer_Workaround = BearerWorkaround.IsChecked ?? true;

            UserSettings.Save();

            DialogResult = true;
            Close();
        }

        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            initApiKey = UserSettings.ApiKey;
            initApiSecret = UserSettings.ApiSecret;

            ApiKeyString = UserSettings.ApiKey;
            SecretString = UserSettings.ApiSecret;
            PortalId.Text = UserSettings.PortalId;

            AccountType.SelectedIndex = UserSettings.Production ? 0 : 1;
            BearerWorkaround.IsChecked = UserSettings.Bearer_Workaround;

            KeyIndicatorColor = new SolidColorBrush(sKeyNotValidColor);

            BeginFetchAuthInfo();

            //AccountNum = UserSettings.PortalId;
        }

        private void BeginFetchAuthInfo()
        {
            RefreshingAuthInfo = true;
            SyncAccessTokenState();
            var getAuthAwaiter = mAuthManager.GetAuthInfoAsync().GetAwaiter();
            getAuthAwaiter.OnCompleted(() =>
            {
                RefreshingAuthInfo = false;
                AuthInfo = getAuthAwaiter.GetResult();
                SyncAccessTokenState();
                RefreshToken.Text = AuthInfo?.RefreshToken;
            });
        }

        private void SyncAccessTokenState()
        {
            if (RefreshingAuthInfo) //we are getting
            {
                AccessTokenProgress.Visibility = Visibility.Visible;
                AccessTokenValidIndicator.Visibility = Visibility.Collapsed;
                ViewAccessToken.Visibility = Visibility.Hidden;
                AccessTokenValidText.Text = "Getting new Access Token...";
                RefreshKeyRow.Visibility = Visibility.Hidden;
            }
            else
            {
                AccessTokenProgress.Visibility = Visibility.Collapsed;

                if (AuthInfo == null || AuthInfo.AccessToken == null)
                {
                    AccessTokenValidText.Text = "Error retrieving Access Token";
                    ViewAccessToken.Visibility = Visibility.Hidden;
                    KeyIndicatorColor.Color = sKeyNotValidColor;
                    RevealRefreshKey();
                }
                else if (AuthInfo.ApiKey != ApiKeyString)
                {
                    AccessTokenValidText.Text = "Your Access Token is for a different API Key";
                    ViewAccessToken.Visibility = Visibility.Hidden;
                    KeyIndicatorColor.Color = sKeyNotValidColor;
                }
                else if (mAuthManager.IsAuthValid(AuthInfo))
                {
                    AccessTokenValidText.Text = "Your Access Token is valid until " + AuthInfo.AccessTokenExpiration;
                    ViewAccessToken.Visibility = Visibility.Visible;
                    KeyIndicatorColor.Color = sKeyValidColor;
                }
                else
                {
                    AccessTokenValidText.Text = "Your Access Token expired on " + AuthInfo.AccessTokenExpiration;
                    ViewAccessToken.Visibility = Visibility.Visible;
                    KeyIndicatorColor.Color = sKeyNotValidColor;
                    RevealRefreshKey();
                }

                RefreshKeyRow.Visibility = Visibility.Visible;
                AccessTokenValidIndicator.Visibility = Visibility.Visible;
            }
        }

        private void AccountType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BearerWorkaround.IsEnabled = (AccountType.SelectedIndex == 0);
        }

        private void RevealRefreshKey()
        {
            ShowRefreshKey.Visibility = Visibility.Collapsed;
            RefreshToken.Visibility = Visibility.Visible;
            RefreshTokenButton.Visibility = Visibility.Visible;
        }

        private void RefreshTokenButton_Click(object sender, RoutedEventArgs e)
        {
            //Set these settings now so auth can use them
            // They will be reverted on close if the user doesn't hit save
            UserSettings.ApiKey = ApiKeyString;
            UserSettings.ApiSecret = SecretString;
            mAuthManager.RenewAuth(RefreshToken.Text);
            BeginFetchAuthInfo();
        }

        private void ShowRefreshKey_Click(object sender, RoutedEventArgs e)
        {
            RevealRefreshKey();
        }

        private void ViewAccessToken_Click(object sender, RoutedEventArgs e)
        {
            if (AuthInfo != null) // double check
                MessageBox.Show(AuthInfo.AccessToken, "Access Token");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult != true)
            {
                UserSettings.ApiKey = initApiKey;
                UserSettings.ApiSecret = initApiSecret;
            }
        }
    }
}
