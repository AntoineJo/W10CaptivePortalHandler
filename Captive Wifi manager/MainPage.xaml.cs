using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Captive_Wifi_manager
{
    /// <summary>
    /// A demo captive portal
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            NetworkStatusChange();

            // Create a timer and set a two second interval.
            var aTimer = new Timer();
            aTimer.Interval = 10000;

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += every10seconds;

            // Have the timer fire repeated events (true is the default)
            aTimer.AutoReset = true;

            // Start the timer
            aTimer.Enabled = true;

            onLaunch();
        }

        private void every10seconds(object sender, ElapsedEventArgs e)
        {
            onLaunch();
        }

        void NetworkStatusChange()
        {
            // register for network status change notifications
            try
            {
                var networkStatusCallback = new NetworkStatusChangedEventHandler(OnNetworkStatusChange);

                    NetworkInformation.NetworkStatusChanged += networkStatusCallback;
            }
            catch (Exception ex)
            {
                //rootPage.NotifyUser("Unexpected exception occured: " + ex.ToString(), NotifyType.ErrorMessage);
            }
        }

        private async void onLaunch()
        {
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (InternetConnectionProfile != null)
            {
                // there is a connection available to something
                var network = InternetConnectionProfile.GetNetworkConnectivityLevel();
                switch (network)
                {
                    case NetworkConnectivityLevel.ConstrainedInternetAccess: notConnectedToInternet(); break;
                    case NetworkConnectivityLevel.InternetAccess:
                        if (await checkInternetConnectivity())
                        {
                            connectedToInternet();
                        }
                        else
                        {
                            notConnectedToInternet();

                        }
                        connectedToInternet();
                        break;
                    case NetworkConnectivityLevel.LocalAccess: break;
                    case NetworkConnectivityLevel.None: break;
                }
            }
            else
            {
                //connectivityMaker.Text = "You are connected to: something or nothing, but not to Internet";
                notConnectedToInternet();
            }
        }

        private async Task<bool> checkInternetConnectivity()
        {

            try
            {
                Uri testUri = new Uri("http://www.msftconnecttest.com/redirect");
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(testUri);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                int a = 1;
            }
            return false;

        }

        async void OnNetworkStatusChange(object sender)
        {
            try
            {
                onLaunch();
            }
            catch (Exception ex)
            {
                //rootPage.NotifyUser("Unexpected exception occured: " + ex.ToString(), NotifyType.ErrorMessage);
            }
        }

        private void notConnectedToInternet()
        {
            try
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    try
                    {
                        webview.Navigate(new Uri("http://www.msftconnecttest.com/redirect"));
                        connectivityMaker.Text = "You are connected to: something or nothing, but not to Internet. Loading captive portal...";
                        webview.Visibility = Visibility.Visible;
                        blockText.Visibility = Visibility.Collapsed;
                    }
                    catch (Exception e)
                    {
                        connectivityMaker.Text = "Unknown error";
                    }
                });
            }
            catch (Exception e)
            {

            }
        }

        private void connectedToInternet()
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                try
                {
                    connectivityMaker.Text = "You are connected to: Internet";
                    webview.Visibility = Visibility.Collapsed;
                    blockText.Visibility = Visibility.Visible;
                }
                catch (Exception e)
                {
                    connectivityMaker.Text = "Unknown error";
                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            onLaunch();
        }
    }
}
