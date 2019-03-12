using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Lucidtech.Las.Cred;

namespace Lucidtech.Las.AWSSignatureV4
{
	public class Auth
	{
		/* General information */
		private string Region { get; }
		private string Service { get; }

		/* Keys the customer receives from Lucidtech */
		private string AwsApiKey { get; }
		private string AwsAccessKey { get; }
		private string AwsSecretKey { get; }
		private const string ALGORITHM = "AWS4-HMAC-SHA256";

		public Auth(Credentials credentials)
		{
			AwsApiKey = credentials.ApiKey ;
			AwsSecretKey = credentials.SecretAccessKey;
			AwsAccessKey = credentials.AccessKeyId;
			Region = "eu-west-1";
			Service = "execute-api";
		}

        public Dictionary<string, string> SignHeaders(Uri uri, string method, byte [] body)
        {
	        if (body == null)
	        {
		        body = Encoding.UTF8.GetBytes("");
	        }
	        
            string amzDate = AmzDate();
            string dateStamp = Datestamp();

            Dictionary<string, string> headers = Headers(uri, amzDate);
            
            byte [] canonicalRequest = GetCanonicalRequest(uri, method, body, headers);
            
            string reqDigest = ComputeHashSha256(canonicalRequest);
            string credScope = GetCredentialScope(dateStamp);
			byte[] stringToSign = GetSignString(amzDate, credScope, reqDigest);
            byte[] signingKey = GetSignatureKey(dateStamp);
            string signature = StringFromByteArray(SignHash(signingKey, stringToSign));
			     
            var authHeader = BuildAuthHeader(
	            amzDate: amzDate,
	            signature: signature,
	            credScope:credScope,
	            signedHeaders:headers.Keys.ToList()
		    );
            return authHeader;
        }
        
        /* Public Static Methods */
		public static string StringFromByteArray(byte[] bytes)
		{
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));  
            }
            return builder.ToString();  
		}
		
        public static byte[] SignHash(byte[] key, byte[] msg)
        {
			var hmac = new HMACSHA256(key);
			return hmac.ComputeHash(msg);
        }
        
        /* Private Methods */
        
		private byte[] GetCanonicalRequest(Uri uri, string method, byte [] body, Dictionary<string, string> headers)
        {
	        string headerList = string.Join(";", headers.Keys);
	        
	        string headerParts = "";
	        foreach (var entry in headers)
            {
				headerParts = headerParts + $"{entry.Key}:{entry.Value}\n";
            }

	        string payloadHash = ComputeHashSha256(body);
	        
            var requestComponents = new List<string>() {
                method,
                uri.AbsolutePath,
                uri.Query,
                headerParts,
                headerList,
                payloadHash
            };
		    
            string canReq = string.Join("\n",requestComponents);
            byte[] canonicalRequest = Encoding.UTF8.GetBytes(canReq);
		    
	        return canonicalRequest;
        }
	
		private Dictionary<string, string> Headers(Uri uri, string amzDate)
		{
			var headers = new Dictionary<string, string>()
			{
				{"host", uri.Authority},
				{"x-amz-date", amzDate},
				{"x-api-key", AwsApiKey}
			};
			return headers;
		}

        private byte[] GetSignatureKey(string dateStamp)
        {
	        var signature = Encoding.UTF8.GetBytes("AWS4" + AwsSecretKey);
	        
	        var parts = new List<string>() {dateStamp, Region, Service, "aws4_request"};
	        foreach (var part in parts)
	        {
		        signature = SignHash(signature, Encoding.UTF8.GetBytes(part));
	        }
	        
	        return signature;
        }

        private string GetCredentialScope(string dateStamp)
        {
	        // Comment: suddenly a static string...
	        var parts = new List<string>() {dateStamp, Region, Service, "aws4_request"};
	        return string.Join("/", parts);
        }
		private Dictionary<string, string> BuildAuthHeader(string amzDate, string signature, string credScope, List<string> signedHeaders)
		{
			var auth = new Dictionary<string, string>()
			{
				{"Credential", $"{AwsAccessKey}/{credScope}"},
				{"SignedHeaders", string.Join(";", signedHeaders)},
				{"Signature", signature}
			};
			
			var authParts = new List<string>();
	        foreach (var entry in auth)
            {
				authParts.Add($"{entry.Key}={entry.Value}");
            }

	        var authString = string.Join(", ", authParts);

	        var headers = new Dictionary<string, string>() 
	        {
                {"x-amz-date", amzDate},
                {"x-api-key", AwsApiKey},
                {"Authorization", string.Concat(ALGORITHM," ", authString)}
            };
            return headers;
		}
		
		/* Private Static Methods */
        private static byte[] GetSignString(string amzDate, string credScope, string reqDigest)
        {
	        var parts = new List<string>(){ALGORITHM, amzDate, credScope, reqDigest};
	        return Encoding.UTF8.GetBytes(string.Join("\n", parts));
        }

        private static string ComputeHashSha256(byte[] body)
        {
	        var sha256Hash = SHA256.Create();
            var bytes = sha256Hash.ComputeHash(body);
            return StringFromByteArray(bytes);
        }
        
		private static string AmzDate()
		{
			DateTime now = DateTime.UtcNow;
			string amzDate = now.ToString("yyyyMMddTHHmmssZ");
			return amzDate;
		}

		private static string Datestamp()
		{
			DateTime now = DateTime.UtcNow;
			string dateStamp = now.ToString("yyyyMMdd");
			return dateStamp;
		}

		
    } // Class Auth
} // Namespace Lucidtech.Las.AWSSignatureV4
