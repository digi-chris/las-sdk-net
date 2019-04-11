using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Lucidtech.Las.Core;

namespace Lucidtech.Las.Utils
{
    public class AmazonAuthorization
    {
        
        private string Region { get; }
        private string Service { get; }
        private Credentials Creds { get; }
        private const string Algorithm = "AWS4-HMAC-SHA256";

        public AmazonAuthorization(Credentials credentials)
        {
            Creds = credentials;
            Region = "eu-west-1";
            Service = "execute-api";
        }

        public Dictionary<string, string> SignHeaders(Uri uri, string method, byte[] body)
        {
            if (body == null)
            {
                body = Encoding.UTF8.GetBytes("");
            }
            
            string amzDate = AmzDate();
            string dateStamp = Datestamp();

            Dictionary<string, string> headers = Headers(uri, amzDate);
            
            byte[] canonicalRequest = GetCanonicalRequest(uri, method, body, headers);
            
            string reqDigest = ComputeHashSha256(canonicalRequest);
            string credScope = GetCredentialScope(dateStamp);
            byte[] stringToSign = GetSignString(amzDate, credScope, reqDigest);
            byte[] signingKey = GetSignatureKey(dateStamp);
            string signature = StringFromByteArray(SignHash(signingKey, stringToSign));
                 
            var authHeader = BuildAuthHeader(
                amzDate: amzDate,
                signature: signature,
                credScope: credScope,
                signedHeaders: headers.Keys.ToList()
            );
            return authHeader;
        }
        
        public static string StringFromByteArray(byte[] bytes)
        {
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));  
            }
            return builder.ToString();  
        }
        
        private static byte[] SignHash(byte[] key, byte[] msg)
        {
            var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(msg);
        }
        
        private byte[] GetCanonicalRequest(Uri uri, string method, byte[] body, Dictionary<string, string> headers)
        {
            string headerList = string.Join(";", headers.Keys);
            
            string headerParts = "";
            foreach (var entry in headers)
            {
                headerParts = headerParts + $"{entry.Key}:{entry.Value}\n";
            }

            string payloadHash = ComputeHashSha256(body);
            string query = GetCanonicalQueryString(uri.Query);
            
            var requestComponents = new List<string>() {
                method,
                uri.AbsolutePath,
                query,
                headerParts,
                headerList,
                payloadHash
            };
            
            string canReq = string.Join("\n",requestComponents);
            byte[] canonicalRequest = Encoding.UTF8.GetBytes(canReq);
            
            return canonicalRequest;
        }

        private static string GetCanonicalQueryString(string query)
        {
            if (!string.IsNullOrEmpty(query))
            {
                throw new NotSupportedException("Creating canonical query string is not implemented");
            }
            return query;
        }
        
        private Dictionary<string, string> Headers(Uri uri, string amzDate)
        {
            var headers = new Dictionary<string, string>()
            {
                {"host", uri.Authority},
                {"x-amz-date", amzDate},
                {"x-api-key", Creds.ApiKey}
            };
            return headers;
        }

        private byte[] GetSignatureKey(string dateStamp)
        {
            var signature = Encoding.UTF8.GetBytes("AWS4" + Creds.SecretAccessKey);
            var parts = new List<string>() {dateStamp, Region, Service, "aws4_request"};
            foreach (var part in parts)
            {
                signature = SignHash(signature, Encoding.UTF8.GetBytes(part));
            }
            return signature;
        }

        private string GetCredentialScope(string dateStamp)
        {
            var parts = new List<string>() {dateStamp, Region, Service, "aws4_request"};
            return string.Join("/", parts);
        }
        
        private Dictionary<string, string> BuildAuthHeader(
            string amzDate, string signature, string credScope, List<string> signedHeaders)
        {
            var auth = new Dictionary<string, string>()
            {
                {"Credential", $"{Creds.AccessKeyId}/{credScope}"},
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
                {"x-api-key", Creds.ApiKey},
                {"Authorization", string.Concat(Algorithm," ", authString)}
            };
            return headers;
        }
        
        private static byte[] GetSignString(string amzDate, string credScope, string reqDigest)
        {
            var parts = new List<string>(){Algorithm, amzDate, credScope, reqDigest};
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
    } 
} 
