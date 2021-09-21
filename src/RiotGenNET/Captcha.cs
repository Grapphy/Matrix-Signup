using System;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RiotGenNET
{
    class TwoCaptcha
    {
        public string ApiKey;
        public string InURL { get; private set; } = "https://2captcha.com/in.php";
        public string ResURL { get; private set; } = "https://2captcha.com/res.php";

        /// <summary>
        /// Creates a TwoCaptcha object. This class is used to solve
        /// Google ReCaptcha v2 using 2captcha API, which requires a
        /// valid client key with enough credit.
        /// </summary>
        /// <see href=https://2captcha.com/2captcha-api>2captcha Docs</see>
        /// <param name="key"> Client key for API.</param>
        public TwoCaptcha(string key) {
            ApiKey = key;
        }

        /// <summary>
        /// Solves Google ReCaptcha v2 through 2captcha API.
        /// It is required to have credits since it is not a free
        /// service. The default timeout is 2 minutes, which usually
        /// takes less than that.
        /// </summary>
        /// <param name="site_key">Google Site Key to identify service.</param>
        /// <param name="site_url">Website URL where the captcha is required.</param>
        /// <param name="timeout">Timeout for the request.</param>
        /// <returns>Captcha solution hash.</returns>
        /// <exception cref="Exception">TimeoutError</exception>
        public async Task<string> SolveRecaptcha(string site_key, string site_url, int timeout = 120) {
            using (var client = new HttpClient())
            {
                UriBuilder url = new UriBuilder(InURL);
                NameValueCollection urlParams = HttpUtility.ParseQueryString(url.Query);
                urlParams.Add("key", ApiKey);
                urlParams.Add("method", "userrecaptcha");
                urlParams.Add("googlekey", site_key);
                urlParams.Add("pageurl", site_url);
                urlParams.Add("json", "1");
                url.Query = urlParams.ToString();

                var res = await client.GetStringAsync(url.Uri);
                JObject r_json = (JObject)JsonConvert.DeserializeObject(res);

                if ( r_json["error_text"] != null )
                    throw new Exception(r_json["error_text"].ToString());
                
                await Task.Delay(15000);

                url = new UriBuilder(ResURL);
                urlParams = HttpUtility.ParseQueryString(url.Query);
                urlParams.Add("key", ApiKey);
                urlParams.Add("action", "get");
                urlParams.Add("id", r_json["request"].ToString());
                urlParams.Add("json", "1");
                url.Query = urlParams.ToString();

                for (int i = 0; i < (timeout / 5); i++) {
                    res = await client.GetStringAsync(url.Uri);
                    r_json = (JObject)JsonConvert.DeserializeObject(res);

                    if ( r_json["error_text"] != null )
                        throw new Exception(r_json["error_text"].ToString());

                    if (r_json["status"].ToString() == "1")
                        return r_json["request"].ToString();

                    await Task.Delay(5000);
                }

                throw new Exception("Error captcha timedout!");
            }
        }
    }
}