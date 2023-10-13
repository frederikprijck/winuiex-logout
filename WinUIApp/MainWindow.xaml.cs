using IdentityModel.Client;
using IdentityModel.OidcClient.Browser;
using IdentityModel.OidcClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Protection.PlayReady;
using System.Text.Json.Nodes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUIApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        OidcClient client = new OidcClient(new OidcClientOptions
        {
            Authority = "https://frdrkprck-free.eu.auth0.com",
            ClientId = "PgFGXcmhdOYCJ8CSWmjFd5GK86DgWysF",
            Browser = new WebAuthenticatorBrowser(),
            RedirectUri = $"myapp://callback",
            PostLogoutRedirectUri = $"myapp://callback",
            Scope = "openid profile email"
        });

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private async void OnLoginClicked(object sender, RoutedEventArgs e)
        {
            var result = await client.LoginAsync();

            if (result.IsError == false)
            {
                LoginBtn.Visibility = Visibility.Collapsed;
                LogoutBtn.Visibility = Visibility.Visible;
            }
        }

        private async void OnLogoutClicked(object sender, RoutedEventArgs e)
        {
            var logoutParameters = new Dictionary<string, string>();

            logoutParameters["client_id"] = client.Options.ClientId;
            logoutParameters["returnTo"] = client.Options.PostLogoutRedirectUri;


            var logoutUrl = new RequestUrl($"https://frdrkprck-free.eu.auth0.com/v2/logout").Create(new Parameters(logoutParameters));
            var logoutRequest = new LogoutRequest();
            var browserOptions = new BrowserOptions(logoutUrl, logoutParameters["returnTo"])
            {
                Timeout = TimeSpan.FromSeconds(logoutRequest.BrowserTimeout),
                DisplayMode = logoutRequest.BrowserDisplayMode,
            };

            await client.Options.Browser.InvokeAsync(browserOptions);

            LogoutBtn.Visibility = Visibility.Collapsed;
            LoginBtn.Visibility = Visibility.Visible;
        }

        private string GetReturnToUrl()
        {
            var returnTo = client.Options.PostLogoutRedirectUri;

            UriBuilder b = new UriBuilder(returnTo);
            Uri uri = new Uri(returnTo);

            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

            query["state"] = GetState();

            b.Query = query.ToString();

            return b.Uri.ToString();
        }

        private string GetState()
        {
            //return "abc";
            var g = Guid.NewGuid();
            var taskId = g.ToString();
            var stateJson = new JsonObject
            {
                { "appInstanceId", Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().Key },
                { "signinId", taskId }
            };

            return Uri.EscapeDataString(stateJson.ToJsonString());
            //return stateJson.ToJsonString();
        }
    }
}
