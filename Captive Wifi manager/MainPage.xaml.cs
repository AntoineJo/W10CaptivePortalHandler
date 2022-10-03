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
        enum Connectivity
        {
            NotDefined = 0,
            NetworkError = -1,
            OtherError = -2,
            InternetConnected = 1,
            CaptivePortalD = 2
        }

        
        bool initialLaunch = true;
        bool internetConnected = false;
        Connectivity connectivityStatus = Connectivity.NotDefined;
        string captivePortalUri = "";

        public MainPage()
        {
            this.InitializeComponent();
            NetworkStatusChange();

            // Create a timer and set a 10s interval to force a connectivity check in case the network connectivity status change event is not triggered
            var aTimer = new Timer();
            aTimer.Interval = 10000;
            aTimer.Elapsed += everyXseconds;
            aTimer.AutoReset = true;
            // Start the timer
            aTimer.Enabled = true;

            //Initial check
            onLaunch();
        }

        private void everyXseconds(object sender, ElapsedEventArgs e)
        {
            onLaunch();
        }

        private void OnNewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs e)
        {

            sender.Navigate(e.Uri);
            e.Handled = true;

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
                //Check the current connection status
                bool currentConnection = await checkInternetConnectivity();

                //if the status is different that the one before or if this is the initial launch
                if (internetConnected != currentConnection || initialLaunch)
                {
                    //Block or display the webview
                    if (currentConnection)
                    {
                        connectedToInternet();
                    }
                    else
                    {
                        notConnectedToInternet();
                    }
                    initialLaunch = false;
                    internetConnected = currentConnection;
                }
                //else do nothing

            }
            else
            {
                notConnectedToInternet();
            }
        }

        private async Task<bool> checkInternetConnectivity()
        {

            try
            {

                //Using the Microsoft Edge captive portal detection mecanism (same as chromium detection https://source.chromium.org/chromium/chromium/src/+/main:components/captive_portal/core/captive_portal_detector.cc)
                Uri testUri = new Uri("http://edge-http.microsoft.com/captiveportal/generate_204");
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(testUri);

                // A 204 response code indicates there's no captive portal.
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    captivePortalUri = "";
                    return true;
                }
                else
                {
                    if(response.RequestMessage.RequestUri != testUri)
                    {
                        captivePortalUri = response.RequestMessage.RequestUri.AbsoluteUri;
                    }
                    else
                    {
                        captivePortalUri = "";
                    }
                    return false;
                }
            }
            catch (HttpRequestException httpException)
            {
                //Test if the certificate is not valid, in that case it is likely that we are behind a captive portal
                if (httpException.HResult == -2147012858)
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
                        if (captivePortalUri != "")
                        {
                            webview.Navigate(new Uri(captivePortalUri));
                        }
                        else
                        {
                            webview.Navigate(new Uri("http://www.msftconnecttest.com/redirect"));
                        }

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
            //force refresh by cleaning previous network connectivity status
            initialLaunch = true;
            onLaunch();
        }
    }
}
