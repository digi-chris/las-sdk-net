using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using IniParser;
using IniParser.Model;

namespace Lucidtech.Las.Core
{
    /// <summary>
    /// Used to fetch and store credentials. One of 3 conditions must be met to successfully create credentials.
    /// 1. The path to the file where the credentials are stored is provided
    /// 2. AccessKeyId, SecretAccessKey and ApiKey are provided
    /// 3. Credentials are located in default path ~/.lucidtech/credentials.cfg
    /// 
    /// Get credentials by contacting hello@lucidtech.ai
    /// 
    /// </summary>
    public class Credentials
    {
        
        /// <summary>
        /// Amazon Access key ID. Provided by Lucidtech.
        /// </summary>
        public string AccessKeyId { get; }
        
        /// <summary>
        /// Amazon Secret Access Key. Provided by Lucidtech.
        /// </summary>
        public string SecretAccessKey { get; }
        
        /// <summary>
        /// AWS API Gateway API key. Provided by Lucidtech.
        /// </summary>
        public string ApiKey{ get; }
        
        /// <summary>
        /// Credentials constructor where AccessKeyId, SecretAccessKey and ApiKey are provided.
        /// </summary>
        /// <param name="accessKeyId"> Access key id </param>
        /// <param name="secretAccessKey"> Secret Access Key </param>
        /// <param name="apiKey"> API key </param>
        /// <exception cref="ArgumentException"></exception>
        public Credentials(string accessKeyId, string secretAccessKey,
            string apiKey)
        {
            AccessKeyId = accessKeyId;
            SecretAccessKey = secretAccessKey;
            ApiKey = apiKey;
        }
        
        /// <summary>
        /// Credentials constructor where the path is provided.
        /// </summary>
        /// <param name="credentialsPath"> Path to the file where the credentials are stored </param>
        public Credentials(string credentialsPath)
        {
            var credentials = ReadCredentials(credentialsPath);
            AccessKeyId = credentials["AccessKeyId"];
            SecretAccessKey = credentials["SecretAccessKey"];
            ApiKey = credentials["ApiKey"];
        }
        
        /// <summary>
        /// Credentials constructor where the credentials are located at the default path.
        /// ~/.lucidtech/credentials.cfg for linux and %USERPROFILE%\.lucidtech\credentials.cfg for Windows.
        /// </summary>
        public Credentials() : this(GetCredentialsPath()){}
        
        private static string GetCredentialsPath()
        {
            string path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)?"%USERPROFILE%\\.lucidtech\\credentials.cfg":"%HOME%/.lucidtech/credentials.cfg";
            return Environment.ExpandEnvironmentVariables(path);
        }
        private static Dictionary<string, string> ReadCredentials(string credentialPath)
        {
            var parser = new FileIniDataParser();
            const string section = "default";
            IniData config = parser.ReadFile(credentialPath);
            var ret = new Dictionary<string, string>()
            {
                {"AccessKeyId", config[section]["access_key_id"]},
                {"SecretAccessKey", config[section]["secret_access_key"]},
                {"ApiKey", config[section]["api_key"]}
            } ;
            return ret;
        }
    }
}
