using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HelloWorld.OAuth2
{
    public class RelyingParty
    {
        public static readonly Config SSO73 = new Config
        {
            CLIENT_ID = "client1",
            CLIENT_SECRET = "client1.secret",
            SCOPE = "userinfo",
            REDIRECT_URI = "https://client1.ubidemo.com",
            METADATA_URI = "https://sso73.ubisecurecloudtest.com/uas/oauth2/metadata.json",
        };
        public HttpRequestMessage NewMetadataRequest(Config config)
        {
            if (config.METADATA_URI == null)
            {
                return null;
            }
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, config.METADATA_URI);
            return request;
        }
        public async Task<Metadata> GetMetadata(Config config)
        {
            HttpRequestMessage metadataRequest = NewMetadataRequest(config);
            if (metadataRequest == null)
            {
                return null;
            }
            HttpResponseMessage response = await new HttpClient().SendAsync(metadataRequest);
            string content = await response.Content.ReadAsStringAsync();
            if (content == null) return null;
            MemoryStream buf = new MemoryStream(Encoding.UTF8.GetBytes(content));
            Metadata metadata = new DataContractJsonSerializer(typeof(Metadata)).ReadObject(buf) as Metadata;
            return metadata;
        }
        public async Task<Config> DoDiscovery(Config config)
        {
            Metadata metadata = await GetMetadata(config);
            if (metadata == null)
            {
                return null;
            }
            config.AUTHORIZE_URI = metadata.AuthorizationEndpoint ?? config.AUTHORIZE_URI;
            config.TOKEN_URI = metadata.TokenEndpoint ?? config.TOKEN_URI;
            config.USERINFO_URI = metadata.UserInfoEndpoint ?? config.USERINFO_URI;
            config.TOKENINFO_URI = metadata.TokenInfoEndpoint ?? config.TOKENINFO_URI;
            return config;
        }
        public Uri NewAuthnRequest(Config config, Guid state)
        {
            UriBuilder uri = new UriBuilder(config.AUTHORIZE_URI);
            string authnRequest =
                "scope=" + WebUtility.UrlEncode(config.SCOPE)
                + "&response_type=" + WebUtility.UrlEncode("code")
                + "&redirect_uri=" + WebUtility.UrlEncode(config.REDIRECT_URI)
                + "&client_id=" + WebUtility.UrlEncode(config.CLIENT_ID)
                + "&state=" + state
                ;
            uri.Query = authnRequest;
            return uri.Uri;
        }
        public HttpRequestMessage NewAccessTokenRequest(Config config, string authorizationCode)
        {
            UriBuilder uri = new UriBuilder(config.TOKEN_URI);
            string tokenRequest =
                "grant_type=" + WebUtility.UrlEncode("authorization_code")
                + "&redirect_uri=" + WebUtility.UrlEncode(config.REDIRECT_URI)
                + "&code=" + WebUtility.UrlEncode(authorizationCode)
                + "&client_id=" + WebUtility.UrlEncode(config.CLIENT_ID)
                + "&client_secret=" + WebUtility.UrlEncode(config.CLIENT_SECRET)
                ;
            HttpContent body = new ByteArrayContent(Encoding.UTF8.GetBytes(tokenRequest));
            body.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri.Uri);
            request.Content = body;
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(config.CLIENT_ID + ":" + config.CLIENT_SECRET))
            );
            return request;
        }
        public async Task<TokenResponse> GetAccessToken(Config config, Uri authorizationResponse, Guid state)
        {
            var map = DecodeQueryString(authorizationResponse);
            // state
            string s;
            if (map.TryGetValue("state", out s))
            {
                if (state.ToString() != s)
                {
                    // invalid state
                    return null;
                }
            }
            else
            {
                // state missing
                return null;
            }
            // code
            string authorizationCode;
            if (map.TryGetValue("code", out authorizationCode))
            {
                HttpRequestMessage tokenRequest = NewAccessTokenRequest(config, authorizationCode);
                HttpResponseMessage response = await new HttpClient().SendAsync(tokenRequest);
                string content = await response.Content.ReadAsStringAsync();
                if (content == null) return null;
                MemoryStream buf = new MemoryStream(Encoding.UTF8.GetBytes(content));
                TokenResponse token = new DataContractJsonSerializer(typeof(TokenResponse)).ReadObject(buf) as TokenResponse;
                return token;
            }
            return null;
        }
        public HttpRequestMessage NewUserInfoRequest(Config config, TokenResponse accessToken)
        {
            UriBuilder uri = new UriBuilder(config.USERINFO_URI);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri.Uri);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                accessToken.AccessToken
            );
            return request;
        }
        public async Task<UserInfo> GetUserInfo(Config config, TokenResponse accessToken)
        {
            HttpRequestMessage userInfoRequest = NewUserInfoRequest(config, accessToken);
            HttpResponseMessage response = await new HttpClient().SendAsync(userInfoRequest);
            string content = await response.Content.ReadAsStringAsync();
            if (content == null) return null;
            MemoryStream buf = new MemoryStream(Encoding.UTF8.GetBytes(content));
            UserInfo userInfo = new DataContractJsonSerializer(typeof(UserInfo)).ReadObject(buf) as UserInfo;
            return userInfo;
        }
        public static IDictionary<string,string> DecodeQueryString(Uri uri) {
            var map = new Dictionary<string,string>();
            var query = uri.Query;
            // pattern to skip leading ? character
            var prefix = new Regex("^" + Regex.Escape("?"));
            query = prefix.Replace(query, "");
            // pattern to split on =
            var equals = new Regex("([^" + Regex.Escape("=") + "]+)(" + Regex.Escape("=") + ".*)?");
            // split on &
            foreach (var i in query.Split('&'))
            {
                var match = equals.Match(i);
                if (match.Success)
                {
                    var left = match.Groups[1];
                    var right = match.Groups[2];
                    map[WebUtility.UrlDecode(left.Value)] = WebUtility.UrlDecode(right.Value.Substring(1));
                }
            }
            return map;
        }
    }
}
