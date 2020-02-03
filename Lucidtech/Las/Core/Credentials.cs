using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using IniParser;
using IniParser.Model;

using RestSharp;
using Newtonsoft.Json;

namespace Lucidtech.Las.Core
{
    /// <summary>
    /// Used to fetch and store credentials. One of 3 conditions must be met to successfully create credentials.
    /// 1. The path to the file where the credentials are stored is provided
    /// 2. ClientId, ClientSecret, ApiKey, AuthEndpoint and ApiEndpoint are provided
    /// 3. Credentials are located in default path ~/.lucidtech/credentials.cfg
    /// 
    /// Get credentials by contacting hello@lucidtech.ai
    /// 
    /// </summary>
    public class AmazonCredentials
    {
        
        /// <summary>
        /// Amazon Access key ID. Provided by Lucidtech.
        /// </summary>
        public string ClientId { get; }
        
        /// <summary>
        /// Amazon Secret Access Key. Provided by Lucidtech.
        /// </summary>
        public string ClientSecret { get; }
        
        /// <summary>
        /// AWS API Gateway API key. Provided by Lucidtech.
        /// </summary>
        public string ApiKey{ get; }
        
        /// <summary>
        /// AWS Authorization endpoint. Provided by Lucidtech.
        /// </summary>
        public string AuthEndpoint{ get; }
        
        /// <summary>
        /// AWS API Gateway API endpoint. Provided by Lucidtech.
        /// </summary>
        public string ApiEndpoint{ get; }

        /// <summary>
        /// Access token to API endpoint.
        /// </summary>
        public string AccessToken
        {
            get
            {
                accessToken, expiration = AccesTokenAndTimestamp;
                if !accessToken || (DateTime.UtcNow > expiration)
                {
                    accessToken, expiration = GetClientCredentials();
                    AccesTokenAndTimestamp = Tuple.Create(accessToken, expiration)
                }
                return accessToken
            }
        }
        
        /// <summary>
        /// Access token and timestamp to API endpoint. Provided and updated by calling AccessToken.
        /// </summary>
        private Tuple<string, int> AccessTokenAndTimestamp;

        private Tuple<string, int> GetClientCredentials()
        {
            url = $"https://{AuthEndpoint}/oauth2/token?grant_type=client_credentials"  
            headers = {'Content-Type': 'application/x-www-form-urlencoded'}

            var restClient = new RestClient(ApiEndpoint);
            restClient.Authenticator = new HttpBasicAuthenticator(ClientId, ClientSecret)

            var request = new RestRequest(url, Method.POST, DataFormat.Json);
            request.JsonSerializer = JsonSerialPublisher.Default;
            foreach (var entry in headers) { request.AddHeader(entry.Key, entry.Value); }

            var response = restClient.Execute(request);
            var response_data = JsonDecode(response);
            var a = response_data['access_token'];
            var b = response_data['expires_in'];
            var exp = DateTime.UtcNow + b;
            
            return new Tuple<String,Int32>(a, exp);
        }
        
        private object JsonDecode(IRestResponse response)
        {
            try
            {
                var jsonResponse = JsonSerialPublisher.DeserializeObject(response.Content);
                return jsonResponse;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in response. Returned {e}");
                throw new Exception(response.ToString());
            }
            
        }
        
        /// <summary>
        /// Credentials constructor where ClientId, ClientSecret, ApiKey, AuthEndpoint and ApiEndpoint are provided.
        /// </summary>
        /// <param name="clientId"> Access key id </param>
        /// <param name="clientSecret"> Secret Access Key </param>
        /// <param name="apiKey"> API key </param>
        /// <param name="authEndpoint"> Authorization endpoint </param>
        /// <param name="apiEndpoint"> API endpoint </param>
        /// <exception cref="ArgumentException"></exception>
        public AmazonCredentials(string clientId, string clientSecret, string apiKey, string authEndpoint, string apiEndpoint)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            ApiKey = apiKey;
            AuthEndpoint = authEndpoint;
            ApiEndpoint = apiEndpoint;

        }
        
        /// <summary>
        /// Credentials constructor where the path is provided.
        /// </summary>
        /// <param name="credentialsPath"> Path to the file where the credentials are stored </param>
        public AmazonCredentials(string credentialsPath)
        {
            var credentials = ReadCredentials(credentialsPath);
            ClientId = credentials["ClientId"];
            ClientSecret = credentials["ClientSecret"];
            ApiKey = credentials["ApiKey"];
            AuthEndpoint = credentials["AuthEndpoint"];
            ApiEndpoint = credentials["ApiEndpoint"];
        }
        
        /// <summary>
        /// Credentials constructor where the credentials are located at the default path.
        /// ~/.lucidtech/credentials.cfg for linux and %USERPROFILE%\.lucidtech\credentials.cfg for Windows.
        /// </summary>
        public AmazonCredentials() : this(GetCredentialsPath()) {}
        
        private static string GetCredentialsPath()
        {
            string path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 
                "%USERPROFILE%\\.lucidtech\\credentials.cfg" : "%HOME%/.lucidtech/credentials.cfg";
            return Environment.ExpandEnvironmentVariables(path);
        }
        
        private static Dictionary<string, string> ReadCredentials(string credentialPath)
        {
            var parser = new FileIniDataParser();
            const string section = "default";
            IniData config = parser.ReadFile(credentialPath);
            var ret = new Dictionary<string, string>()
            {
                {"ClientId", config[section]["client_id"]},
                {"ClientSecret", config[section]["client_secret"]},
                {"ApiKey", config[section]["api_key"]}
                {"AuthEndpoint", config[section]["auth_endpoint"]}
                {"ApiEndpoint", config[section]["api_endpoint"]}
            };
            return ret;
        }
    }
}
