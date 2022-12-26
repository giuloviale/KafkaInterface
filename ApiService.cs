﻿using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading.Tasks;

namespace Api.NETCORE.Dynamics365.Clases
{
    public class ApiService
    {
        public class OAuthMessageHandler : DelegatingHandler
        {
            private AuthenticationHeaderValue authHeader;

            public OAuthMessageHandler(string serviceUrl, string clientId, string redirectUrl, string username, string password,
                    HttpMessageHandler innerHandler)
                : base(innerHandler)
            {

                string apiVersion = "9.2";
                string webApiUrl = $"{serviceUrl}/api/data/v{apiVersion}/";

                //Build Microsoft.Identity.Client (MSAL) OAuth Token Request
                var authBuilder = PublicClientApplicationBuilder.Create(clientId)
                                .WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs)
                                .WithRedirectUri(redirectUrl)
                                .Build();
                var scope = serviceUrl + "//.default";
                string[] scopes = { scope };

                AuthenticationResult authBuilderResult;
                if (username != string.Empty && password != string.Empty)
                {
                    //Make silent Microsoft.Identity.Client (MSAL) OAuth Token Request
                    var securePassword = new SecureString();
                    foreach (char ch in password) securePassword.AppendChar(ch);
                    authBuilderResult = authBuilder.AcquireTokenByUsernamePassword(scopes, username, securePassword)
                                .ExecuteAsync().Result;
                }
                else
                {
                    //Popup authentication dialog box to get token
                    authBuilderResult = authBuilder.AcquireTokenInteractive(scopes)
                                .ExecuteAsync().Result;
                }

                //Note that an Azure AD access token has finite lifetime, default expiration is 60 minutes.
                authHeader = new AuthenticationHeaderValue("Bearer", authBuilderResult.AccessToken);
            }

            protected override Task<HttpResponseMessage> SendAsync(
                      HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                request.Headers.Authorization = authHeader;
                return base.SendAsync(request, cancellationToken);
            }

            public static HttpClient GetHttpClient(string clientId, string redirectUrl, string version = "v9.2")
            {
                string url = redirectUrl;
                string username = "power_platform@ues21.edu.ar";
                string password = "yPhkkLs3s4UZZptPRNBU";
                string Id = clientId;
                try
                {
                    HttpMessageHandler messageHandler = new OAuthMessageHandler(url, clientId, redirectUrl, username, password,
                                  new HttpClientHandler());

                    HttpClient httpClient = new HttpClient(messageHandler)
                    {
                        BaseAddress = new Uri(string.Format("{0}/api/data/{1}/", url, version)),

                        Timeout = new TimeSpan(0, 2, 0)  //2 minutes
                    };

                    return httpClient;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
