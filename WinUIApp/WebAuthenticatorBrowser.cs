using IdentityModel.Client;
using IdentityModel.OidcClient.Browser;
using System;
using System.Threading.Tasks;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUIApp
{
    public class WebAuthenticatorBrowser : IdentityModel.OidcClient.Browser.IBrowser
    {
        /// <inheritdoc />
        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
        {
            try
            {
                // If it's logout, and it's windows we need to add state to returnTo

                if (options.StartUrl.IndexOf("logout") > -1)
                {
                    WinUIExEx.WebAuthenticator.BeforeProcessStart = (uri) =>
                    {
                        // Move state param on root, to state param on returnTo

                        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                        var state = query["state"];
                        var returnTo = query["returnTo"];


                        UriBuilder b = new UriBuilder(returnTo);
                        Uri returnToUri = new Uri(returnTo);

                        var returnToQuery = System.Web.HttpUtility.ParseQueryString(returnToUri.Query);

                        returnToQuery["state"] = Uri.EscapeDataString(state);

                        b.Query = returnToQuery.ToString();


                        UriBuilder a = new UriBuilder(uri);


                        query["returnTo"] = b.Uri.ToString();

                        a.Query = query.ToString();

                        return a.Uri;
                    };
                }

               

                var result = await WinUIExEx.WebAuthenticator.AuthenticateAsync(new Uri(options.StartUrl), new Uri(options.EndUrl));


                var url = new RequestUrl(options.EndUrl)
                    .Create(new Parameters(result.Properties));

                return new BrowserResult
                {
                    Response = url,
                    ResultType = BrowserResultType.Success
                };
            }
            catch (TaskCanceledException)
            {
                return new BrowserResult
                {
                    ResultType = BrowserResultType.UserCancel,
                    ErrorDescription = "Login canceled by the user."
                };
            }
        }
    }
}
