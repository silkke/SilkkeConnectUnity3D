using System;
using LitJson;
using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Silkke
{
    public class API
    {
        static public bool printDebugMessages = true;

        public delegate void ApiCallback(JsonData reponse);
        public delegate void LogCallback(string data);
        public delegate void DLCallback(string id, WWW www);

        static public bool isInProd = false;

        // ##### PROD URL #####
        public static string apiURL = "https://silkke.net/api/v3.1/";
        public static string apiURLTokenProd = "https://silkke.net/oauth/token";

        // ###### DEV URL #####
        public static string apiURLDev = "https://dev.silkke.net/api/v3.1/";
        public static string apiURLTokenDev = "https://dev.silkke.net/oauth/token";

        // Return API url depending on if it's in dev OR prod
        static private string getapiURL(string functionURL)
        {
            if (isInProd)
                return apiURL + functionURL;
            else
                return apiURLDev + functionURL;
        }

        // SEND 'GET' REQUEST
        static public IEnumerator GET(string functionURL
                              , Dictionary<string, string> args
                              , ApiCallback onSuccess
                              , ApiCallback onFailure
                              , LogCallback statusLog
                              , LogCallback errorLog
                              , string apiURLOverride = null)
        {
            string url;
            if (apiURLOverride == null)
                url = getapiURL(functionURL);
            else
                url = apiURLOverride + functionURL;

            int i = 0;

            if (args != null)
            {
                url += "?";
                foreach (var entry in args)
                {
                    url += entry.Key + "=" + entry.Value;
                    if (i + 1 != args.Count)
                    { url += "&"; i++; }
                }
            }

            WWW www;
            WWWForm form = new WWWForm();

            Dictionary<string, string> headers = form.headers;

            // Authorization header
            if (AccessToken.access_token != null && AccessToken.refresh_token != null)
                headers["Authorization"] = "Bearer " + AccessToken.access_token;

            www = new WWW(url, null, headers);

            yield return www;

            if (printDebugMessages)
                Debug.Log("Request : " + url);

            if (!string.IsNullOrEmpty(www.error))
                errorLog(url + ": failed to connect to server ==> " + www.error);
            else
            {
                if (printDebugMessages)
                    Debug.Log("[API Call : " + www.text + "]");

                JsonData response = JsonMapper.ToObject(www.text);
                if (Convert.ToBoolean(response["error"].ToString()) == true)
                {
                    errorLog("GET : " + url + " => [Failed : " + response["error_long_msg"] + "]");
                    onFailure(response["error_long_msg"]);
                }
                else
                {
                    if (printDebugMessages)
                        statusLog("GET : " + url + " => [Success]");
                    onSuccess(response);
                }
            }
        }

        // SEND 'POST' REQUEST
        static public IEnumerator POST(string functionURL, string res
                              , Dictionary<string, string> args
                              , ApiCallback onSuccess
                              , ApiCallback onFailure
                              , LogCallback statusLog
                              , LogCallback errorLog)
        {
            string url = getapiURL(functionURL);

            WWW www;
            WWWForm form = new WWWForm();

            Dictionary<string, string> headers = form.headers;

            // Authorization header
            if (AccessToken.access_token != null && AccessToken.refresh_token != null)
                headers["Authorization"] = "Bearer " + AccessToken.access_token;

            if (args != null)
            {
                foreach (var entry in args)
                    form.AddField(entry.Key, entry.Value);
                www = new WWW(url, form.data, headers);
            }
            else
                www = new WWW(url, null, headers);

            if (printDebugMessages)
                statusLog("POST : " + url + " => calling...");

            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                errorLog(url + ": failed to connect to server" + " => " + www.error);
                onFailure("Internet connexion problem");
            }
            else
            {
                if (printDebugMessages)
                    Debug.Log("[API Call : " + www.text + "]");

                JsonData response = JsonMapper.ToObject(www.text);
                if (Convert.ToBoolean(response["error"].ToString()) == true)
                {
                    errorLog("POST : " + url + " => [Failed : " + response["error_long_msg"] + "]");
                    onFailure(response["error_long_msg"]);
                }
                else
                {
                    if (printDebugMessages)
                        statusLog("POST : " + url + " => [Success]");
                    onSuccess(response);
                }
            }
        }

        // Get access token for connection
        static public IEnumerator getAccessToken(Dictionary<string, string> logs,
                                                 ApiCallback onSuccess,
                                                 ApiCallback onFailure)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(appConfig.clientID + ":" + appConfig.secretID);
            var encoded = Convert.ToBase64String(plainTextBytes);

            WWW www;
            WWWForm form = new WWWForm();
            if (appConfig.clientID != null && appConfig.secretID != null)
            {
                form.AddField("client_id", appConfig.clientID);
                form.AddField("client_secret", appConfig.secretID);
            }

            // Refresh token after first connection and after expire_time
            if (logs == null)
            {
                form.AddField("grant_type", "refresh_token");
                form.AddField("refresh_token", AccessToken.refresh_token);
            }
            // First connection
            else
            {
                form.AddField("grant_type", "password");
                form.AddField("username", logs["email"]);
                form.AddField("password", logs["password"]);
                form.AddField("scope", "account.read avatar.read avatar.use");
            }

            var headers = form.headers;
            headers["Authorization"] = "Basic " + encoded;
            if (isInProd)
                www = new WWW(apiURLTokenProd, form.data, headers);
            else
                www = new WWW(apiURLTokenDev, form.data, headers);

            yield return www;

            JsonData response = JsonMapper.ToObject(www.text);
            if (!string.IsNullOrEmpty(www.error))
                onFailure(response[1]);
            else
                onSuccess(response);
        }

        static public IEnumerator Download(string id, string url
                                          , DLCallback onSuccess
                                          , DLCallback onFailure)
        {
            using (WWW www = new WWW(url))
            {
                yield return www;

                if (!string.IsNullOrEmpty(www.error))
                    onFailure(id, www);
                else
                    onSuccess(id, www);
            }
        }
    }
}
