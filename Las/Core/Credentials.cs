using System;
using System.Linq;
using System.Runtime.InteropServices;

using IniParser;
using IniParser.Model;

namespace Lucidtech.Las.Core
{
    /// <summary>
    /// Used to fetch and store credentials. One of 3 conditions must be met to successfully create credentials.
    /// 
    /// 1. credentials_path is provided
    /// 2. access_key_id, secret_access_key and api_key is provided
    /// 3. credentials is located in default path ~/.lucidtech/credentials.cfg
    /// 
    /// </summary>
    public class Credentials
    {
        public string AccessKeyId {get;}
        public string SecretAccessKey {get;}
        public string ApiKey{get;}
        
        /// <summary>
        /// The Credentials constructor, will create all the necessary credentials.
        /// 
        /// 1. credentials_path is provided
        /// 2. access_key_id, secret_access_key and api_key is provided
        /// 3. credentials is located in default path ~/.lucidtech/credentials.cfg
        /// 
        /// </summary>
        /// <param name="credentialsPath"> Path to credentials file</param>
        /// <param name="accessKeyId">Access key Id</param>
        /// <param name="secretAccessKey">Secret Access Key</param>
        /// <param name="apiKey">API key</param>
        /// <exception cref="ArgumentException"></exception>
        public Credentials(string credentialsPath = "", string accessKeyId = "", string secretAccessKey = "",
            string apiKey = "")
        {
            string[] input = 
            {
                accessKeyId,
                secretAccessKey,
                apiKey,
                credentialsPath
            };
            
            string[] credentials = input.Take(3).ToArray(); 
            
            if (input.Any(s => string.IsNullOrEmpty(s)))
            {
                if (string.IsNullOrEmpty(credentialsPath))
                {
                    string path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)?"%UserProfile%\\.lucidtech\\credentials.cfg":"%HOME%/.lucidtech/credentials.cfg";
                    credentialsPath = Environment.ExpandEnvironmentVariables(path);
                }

                credentials = ReadCredentials(credentialsPath);

                AccessKeyId = credentials[0];
                SecretAccessKey = credentials[1];
                ApiKey = credentials[2];
            }

            if (credentials.Any(s => string.IsNullOrEmpty(s)))
            {
                throw new ArgumentException("one or more of the credentials are empty");
            }
        }
        private static string[] ReadCredentials(string credentialPath)
        {
            var parser = new FileIniDataParser();
            const string section = "default";
            IniData config = parser.ReadFile(credentialPath);
            
            var accessKeyId = config[section]["access_key_id"];
            var secretAccessKey = config[section]["secret_access_key"];
            var apiKey = config[section]["api_key"];
            
            string[] ret = {accessKeyId, secretAccessKey, apiKey};
            
            return ret;
        }
    } // Class Credentials
} // Namespace Lucidtech.Las.Cred
