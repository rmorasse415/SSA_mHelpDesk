using SSA_mHelpDesk.API;
using SSA_mHelpDesk.Domain;
using SSA_mHelpDesk.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SSA_mHelpDesk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static readonly AuthenticationManager sAuthManager = AuthenticationManager.Instance;

        private TicketListViewModel mTicketListViewModel = new TicketListViewModel();
        private LoadingPageViewModel mLoadingViewModel = new LoadingPageViewModel();

        private TicketListPage mTicketListPage = null;
        private AuthVericationFailedPage mAuthFailedPage = null;
        private LoadingPage mLoadingPage = null;

        private ContentAnimationManager mPageAnimationManager;
        public string TitleLastUpdate = "1/1/1/";

        private enum State
        {
            Init,
            VerifyingAuth,
            AuthVerificationFailed,
            LoadingTicketList,
            LoadingComplete,
        }

        private State _pageState = State.Init;
        private State PageState
        {
            get => _pageState;
            set
            {
                var prevState = _pageState;
                _pageState = value;

                if (prevState != _pageState)
                    OnPageStateChanged(prevState, _pageState);
            }
        }

        private DispatcherTimer refreshTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            PageState = State.VerifyingAuth;

            refreshTimer.Tick += RefreshTimer_Tick;

            mPageAnimationManager = new ContentAnimationManager(this, pageFrame);
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            VerifyAuthAndLoad(false);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VerifyAuthAndLoad(true);
        }

        private void VerifyAuthAndLoad(bool startup)
        {
            PageState = State.VerifyingAuth;
            var getAuthInfoAwaiter = sAuthManager.GetAuthInfoAsync().GetAwaiter();
            getAuthInfoAwaiter.OnCompleted(() =>
            {
                var authInfo = getAuthInfoAwaiter.GetResult();
                if (sAuthManager.IsAuthValid(authInfo))
                {
                    PageState = State.LoadingTicketList;
                    mTicketListViewModel.RefreshTicketsAsync(startup).GetAwaiter().OnCompleted(() => PageState = State.LoadingComplete);
                }
                else // invalid auth
                {
                    PageState = State.AuthVerificationFailed;
                }
            });
        }

        private void Button_Click_Settings(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            bool? result = settingsWindow.ShowDialog();

            if (result == true)
            {
                VerifyAuthAndLoad(true);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnPageStateChanged(State prev, State now)
        {
            UserControl nextPage;
            switch(now)
            {
                case State.VerifyingAuth:
                    if (mLoadingPage == null)
                        mLoadingPage = new LoadingPage(mLoadingViewModel);

                    nextPage = mLoadingPage;
                    mLoadingViewModel.DisplayMessage = "Authenticating...";

                    refreshTimer.Stop();
                    break;
                case State.LoadingTicketList:
                    if (mLoadingPage == null)
                        mLoadingPage = new LoadingPage(mLoadingViewModel);

                    nextPage = mLoadingPage;
                    mLoadingViewModel.DisplayMessage = "Refreshing Ticket List...";

                    refreshTimer.Stop();
                    break;
                case State.LoadingComplete:
                    if (mTicketListPage == null)
                        mTicketListPage = new TicketListPage(mTicketListViewModel);

                    nextPage = mTicketListPage;
                    mTicketListViewModel.ShowRefreshIndicator = false;

                    // This will also restart the timer if it was already running
                    if (UserSettings.AutoRefresh)
                    {
                        refreshTimer.Interval = UserSettings.AutoRefreshPeriod;
                        refreshTimer.Start();
                    }
                    break;
                case State.AuthVerificationFailed:
                    if (mAuthFailedPage == null)
                    {
                        mAuthFailedPage = new AuthVericationFailedPage();
                        mAuthFailedPage.SettingsButton.Click += Button_Click_Settings;
                    }

                    nextPage = mAuthFailedPage;

                    refreshTimer.Stop();
                    break;
                default:
                    nextPage = null;
                    break;
            }

            if (prev == State.Init || pageFrame.Content == null)
            {
                pageFrame.Content = nextPage;
            }
            else if (ReferenceEquals(pageFrame.Content, mTicketListPage) &&
                ReferenceEquals(nextPage, mLoadingPage))
            {
                //don't animate page, just show the refresh indicator
                mTicketListViewModel.ShowRefreshIndicator = true;
            }
            else if (!ReferenceEquals(nextPage, pageFrame.Content))
            {
                //animate to next page
                mTicketListViewModel.ShowRefreshIndicator = false;
                mPageAnimationManager.AnimateToContent(nextPage);
            }
        }
    } //END class

    
}
