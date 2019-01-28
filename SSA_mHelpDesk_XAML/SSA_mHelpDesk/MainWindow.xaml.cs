using SSA_mHelpDesk.API;
using SSA_mHelpDesk.Domain;
using System;
using System.Windows;
using System.Windows.Controls;

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

        public MainWindow()
        {
            InitializeComponent();

            PageState = State.VerifyingAuth;

            mPageAnimationManager = new ContentAnimationManager(this, pageFrame);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VerifyAuthAndLoad();
        }

        private void VerifyAuthAndLoad()
        {
            PageState = State.VerifyingAuth;
            var getAuthInfoAwaiter = sAuthManager.GetAuthInfoAsync().GetAwaiter();
            getAuthInfoAwaiter.OnCompleted(() =>
            {
                var authInfo = getAuthInfoAwaiter.GetResult();
                if (sAuthManager.IsAuthValid(authInfo))
                {
                    PageState = State.LoadingTicketList;
                    mTicketListViewModel.RefreshTicketsAsync().GetAwaiter().OnCompleted(() => PageState = State.LoadingComplete);
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
                VerifyAuthAndLoad();
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
                    break;
                case State.LoadingTicketList:
                    nextPage = mLoadingPage;
                    mLoadingViewModel.DisplayMessage = "Refreshing Ticket List...";
                    break;
                case State.LoadingComplete:
                    if (mTicketListPage == null)
                        mTicketListPage = new TicketListPage(mTicketListViewModel);

                    nextPage = mTicketListPage;
                    break;
                case State.AuthVerificationFailed:
                    if (mAuthFailedPage == null)
                    {
                        mAuthFailedPage = new AuthVericationFailedPage();
                        mAuthFailedPage.SettingsButton.Click += Button_Click_Settings;
                    }

                    nextPage = mAuthFailedPage;
                    break;
                default:
                    nextPage = null;
                    break;
            }

            if (prev == State.Init || pageFrame.Content == null)
            {
                pageFrame.Content = nextPage;
            }
            else if (!ReferenceEquals(nextPage, pageFrame.Content))
            {
                //animate to next page
                mPageAnimationManager.AnimateToContent(nextPage);
            }
        }
    } //END class

    
}
