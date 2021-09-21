using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RiotGenNET
{
    class AuthenticationData
    {
        [JsonProperty("session", Order = 1)]
        public string Session;
        [JsonProperty("type", Order = 2)]
        public string Type;
        [JsonProperty("response", Order = 3, NullValueHandling = NullValueHandling.Ignore)]
        public string Response;
    }

    class RegistrationData
    {
        [JsonProperty("username", Order = 1)]
        public string Username;
        [JsonProperty("password", Order = 2)]
        public string Password;
        [JsonProperty("initial_device_display_name", Order = 3)]
        public string Device;
        [JsonProperty("auth", Order = 4, NullValueHandling = NullValueHandling.Ignore)]
        public AuthenticationData Auth;
        [JsonProperty("inhibit_login", Order = 5)]
        public bool InhibitLogin;

        /// <summary>
        /// Converts the object into a JSON string.
        /// </summary>
        /// <returns>JSON String.</returns>
        public String ToJsonString() {
            return JObject.FromObject(this).ToString(Formatting.None);
        }

        /// <summary>
        /// Returns the parameters from an initial registration request
        /// on the Matrix API. It should return captcha site key and
        /// registration session.
        /// </summary>
        /// <param name="response">String response from request.</param>
        /// <returns>Registration session and captcha site key if any.</returns>
        public (String, String) GetParameters(string response) {
            dynamic r_json = JsonConvert.DeserializeObject(response);

            if (response.Contains("m.login.recaptcha")) {
                return (r_json["session"].ToString(),
                        r_json["params"]["m.login.recaptcha"]["public_key"].ToString());
            }

            return (r_json["session"].ToString(), "");
        }

        /// <summary>
        /// Returns parsed data from the fully registration response on
        /// the Matrix API. The formatting is user_id|access_token.
        /// </summary>
        /// <param name="response">String response from request.</param>
        /// <returns>Formatted user_id and access_token</returns>
        public String GetAuthorization(string response) {
            dynamic r_json = JsonConvert.DeserializeObject(response);
            return String.Format("{0}|{1}",
                                 r_json["user_id"].ToString(),
                                 r_json["access_token"].ToString());
        }
    }
}