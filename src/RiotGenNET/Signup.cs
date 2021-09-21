using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RiotGenNET
{
    class RiotManager
    {
        public string Homeserver;
        public string Device;
        public string ProxyHost;
        public string ProxyUser;
        public string ProxyPass;
        public TwoCaptcha TCaptcha;

        /// <summary>
        /// RiotManager allows the creation of Matrix account under
        /// a determined homeserver. It work together with 2captcha,
        /// which is an API for captcha solving.
        /// 
        /// Currently it does not support email verification, although
        /// some homeservers require it.
        /// </summary>
        /// <param name="home_server">Homeserver URL where to create accounts.</param>
        /// <param name="tcaptcha_key">2captcha client key.</param>
        /// <param name="device_name">A device name at registration.</param>
        /// <param name="use_proxy">Let the constructor set up a proxy.</param>
        /// <param name="use_auth">Let the constructor set up proxy authentication.</param>
        /// <param name="proxy_string">Proxy string as user:pass@host:port or host:port</param>
        public RiotManager(string home_server, string tcaptcha_key,
                           string device_name = "", bool use_proxy = false,
                           bool use_auth = false, string proxy_string = "") {
            Homeserver = home_server;
            TCaptcha = new TwoCaptcha(tcaptcha_key);

            if (device_name == String.Empty) {
                Device = String.Format("{0} (Firefox, Windows)", home_server.Split("//")[^1]);
            } else {
                Device = device_name;
            }

            if (use_proxy) {
                ProxyHost = String.Format("http://{0}", proxy_string.Split("@")[^1]);

                if (use_auth) {
                    ProxyUser = proxy_string.Split("@")[0].Split(":")[0];
                    ProxyPass = proxy_string.Split("@")[0].Split(":")[1];
                }
            }
        }

        /// <summary>
        /// Creates a Matrix account with the given username
        /// and password. Returns the full address of the account
        /// and the authorization key, which is used to interact with
        /// Matrix API endpoints.
        /// </summary>
        /// <param name="username">New account username.</param>
        /// <param name="password">New account password.</param>
        /// <returns>Formatted user_id and access_token</returns>
        public async Task<string> CreateRiotAccount(string username, string password)
        {
            HeadersSuite headers = new HeadersSuite();
            HttpClientHandler handler = new HttpClientHandler();
            
            if (ProxyHost != null) {
                handler.Proxy = new System.Net.WebProxy(ProxyHost);
                handler.UseProxy = true;
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => { return true; };

                if (ProxyUser != null) {
                    handler.Proxy.Credentials = new NetworkCredential(ProxyUser, ProxyPass);
                }
            }

            using var client = new HttpClient(handler);

            RegistrationData signupPayload = new RegistrationData();
            signupPayload.Username = username;
            signupPayload.Password = password;
            signupPayload.Device = Device;
            signupPayload.InhibitLogin = false;
            
            string session_id;
            string site_key;
            var registrationUrl = String.Format("{0}/_matrix/client/r0/register", Homeserver);

            using ( var request = new HttpRequestMessage(HttpMethod.Post, new Uri(registrationUrl)) )
            {            
                request.Content = new StringContent(signupPayload.ToJsonString());
                headers.RegistrationPost(request.Headers, Homeserver);
                var response = await client.SendAsync(request);
                var rdata = signupPayload.GetParameters(await response.Content.ReadAsStringAsync());
                session_id = rdata.Item1;
                site_key = rdata.Item2;
            }
            
            string captcha_response = await TCaptcha.SolveRecaptcha(site_key, Homeserver);
            signupPayload.Auth = new AuthenticationData();
            signupPayload.Auth.Session = session_id;
            signupPayload.Auth.Type = "m.login.recaptcha";
            signupPayload.Auth.Response = captcha_response;

            using ( var request = new HttpRequestMessage(HttpMethod.Post, new Uri(registrationUrl)) )
            {
                request.Content = new StringContent(signupPayload.ToJsonString());
                headers.RegistrationPost(request.Headers, Homeserver);
                var response = await client.SendAsync(request);
            }
            
            signupPayload.Auth.Type = "m.login.dummy";
            signupPayload.Auth.Response = null;

            using ( var request = new HttpRequestMessage(HttpMethod.Post, new Uri(registrationUrl)) )
            {
                request.Content = new StringContent(signupPayload.ToJsonString());
                headers.RegistrationPost(request.Headers, Homeserver);
                var response = await client.SendAsync(request);
                return signupPayload.GetAuthorization(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
