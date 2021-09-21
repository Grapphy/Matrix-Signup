using System.Net.Http.Headers;

namespace RiotGenNET
{
    class HeadersSuite
    {
        private string _defaultUserAgent;
        public string DefaultUserAgent { get => _defaultUserAgent; }
        
        /// <summary>
        /// Creates a class that contains methods to easily
        /// set headers in requests.
        /// </summary>
        public HeadersSuite() {
            _defaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:92.0) Gecko/20100101 Firefox/92.0";
        }

        /// <summary>
        /// Creates a class that contains methods to easily
        /// set headers in requests.
        /// </summary>
        /// <param name="userAgent">User-Agent string to use.</param>
        public HeadersSuite(string userAgent) {
            _defaultUserAgent = userAgent;
        }

        /// <summary>
        /// Set the request headers ready for replicate browser activity
        /// of a registration on Matrix API.
        /// </summary>
        /// <param name="headers">Request headers collection.</param>
        /// <param name="origin">Request origin, usually the homeserver.</param>
        public void RegistrationPost(HttpRequestHeaders headers, string origin) {
            headers.Clear();
            headers.Add("User-Agent", _defaultUserAgent);
            headers.Add("Accept", "application/json");
            headers.Add("Accept-Language", "en-US,en;q=0.5");
            headers.Add("Accept-Encoding", "deflate");
            headers.TryAddWithoutValidation("Content-Type", "application/json");
            headers.Add("Origin", origin);
        }
    }
}