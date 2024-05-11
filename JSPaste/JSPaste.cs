using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JSPasteNet
{
    public static class JSPasteClient
    {
        public static Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

#if DEBUG
        private const bool WriteResponses = true;
#else
        private const bool WriteResponses = false;
#endif

        private static string _serverEndPoint = "https://jspaste.eu/api/documents";

        public static string ServerEndPoint
        {
            get => _serverEndPoint;

            set
            {
                string start = !value.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) ? "https://" : string.Empty;

                _serverEndPoint = start + value.TrimEnd('/') + (value.TrimEnd('/').EndsWith("/api/v2/documents", StringComparison.InvariantCultureIgnoreCase) ? null : "/api/v2/documents");

                if (_serverEndPoint[_serverEndPoint.Length - 1] == '/') _serverEndPoint.Substring(0, ServerEndPoint.Length - 2);
            }
        }

        public static HttpClient httpClient { get; set; } = new HttpClient()
        {
            DefaultRequestHeaders = { { "User-Agent", "JSPaste-CS Client" } },
            Timeout = TimeSpan.FromSeconds(30),
        };

        public static async Task<JSDocument> Publish(string data, DocumentSettings? settings = default) => await Publish(data, DefaultEncoding, settings);

        public static async Task<JSDocument> Publish(string data, Encoding enc, DocumentSettings? settings = default) => await Publish(enc.GetBytes(data), settings);

        public static async Task<JSDocument> Publish(byte[] data, DocumentSettings? settings = default)
        {
            using (HttpRequestMessage req = new(HttpMethod.Post, _serverEndPoint))
            {
                req.Content = new ByteArrayContent(data);

                if (settings == default) settings = new DocumentSettings();
                if (settings != null)
                {
                    if (settings.Key != null) req.Headers.Add("key", settings.Key);
                    if (settings.KeyLength != null && settings.KeyLength > 0) req.Headers.Add("keyLength", settings.KeyLength.ToString());
                    if (settings.Secret != null) req.Headers.Add("secret", settings.Secret);
                    if (settings.Password != null) req.Headers.Add("password", settings.Password);
                    if (settings.LifeTime.TotalSeconds > 0) req.Headers.Add("lifetime", ((long)settings.LifeTime.TotalSeconds).ToString());
                }

                using (HttpResponseMessage res = await SendAsync(req))
                {
                    string responseString = await res.Content.ReadAsStringAsync();

                    if (WriteResponses) Console.WriteLine(responseString);

                    var response = MinimalJsonParser.ParseJson(responseString);

                    if ((int)res.StatusCode >= 400) throw new Exception(responseString);

                    return new JSDocument((string)response["key"], (string)response["secret"], (settings != null ? settings.Password : null));
                }
            }
        }

        public static async Task<string> Get(string key, string? password = null) => DefaultEncoding.GetString(await GetRaw(key, password));

        public static async Task<string> Get(JSDocument doc) => await Get(doc, DefaultEncoding);

        public static async Task<string> Get(string key, Encoding enc, string? password = null) => enc.GetString(await GetRaw(key, password));

        public static async Task<string> Get(JSDocument doc, Encoding enc) => enc.GetString(await GetRaw(doc));

        public static async Task<byte[]> GetRaw(JSDocument doc) => await GetRaw(doc.Key, doc.Password);

        public static async Task<byte[]> GetRaw(string key, string? password = null)
        {
            using (HttpRequestMessage req = new (HttpMethod.Get, _serverEndPoint + '/' +  key + "/raw"))
            {
                if (password == null && DocumentSettings.DefaultPassword != null) password = DocumentSettings.DefaultPassword;
                if (password != null) req.Headers.TryAddWithoutValidation("password", password);

                using (HttpResponseMessage res = await SendAsync(req))
                {
                    return await res.Content.ReadAsByteArrayAsync();
                }
            }
        }

        public static async Task<bool> Remove(JSDocument doc) => await Remove(doc.Key, doc.Secret);

        public static async Task<bool> Remove(string key, string secret)
        {
            using (HttpRequestMessage req = new (HttpMethod.Delete, _serverEndPoint + key))
            {
                req.Headers.TryAddWithoutValidation("secret", secret);

                using (HttpResponseMessage res = await SendAsync(req))
                {
                    string responseString = await res.Content.ReadAsStringAsync();

                    if (WriteResponses) Console.WriteLine(responseString);

                    return (int)res.StatusCode == 200;
                }
            }
        }


        public static async Task<bool> Edit(string key, string secret, string data) => await Edit(new JSDocument(key, secret), data);
        public static async Task<bool> Edit(string key, string secret, byte[] data) => await Edit(new JSDocument(key, secret), data);

        public static async Task<bool> Edit(string key, string secret, string password, string data) => await Edit(new JSDocument(key, secret, password), data);
        public static async Task<bool> Edit(string key, string secret, string password, byte[] data) => await Edit(new JSDocument(key, secret, password), data);

        public static async Task<bool> Edit(JSDocument doc, string data) => await Edit(doc, DefaultEncoding.GetBytes(data));

        public static async Task<bool> Edit(JSDocument doc, byte[] data)
        {
            using (HttpRequestMessage req = new (new HttpMethod("PATCH"), _serverEndPoint + doc.Key))
            {
                req.Content = new ByteArrayContent(data);

                req.Headers.TryAddWithoutValidation("secret", doc.Secret);

                if (doc.Password != null) 
                    req.Headers.TryAddWithoutValidation("password", doc.Password);

                using (HttpResponseMessage res = await SendAsync(req))
                {
                    string responseString = await res.Content.ReadAsStringAsync();

                    if (WriteResponses) Console.WriteLine(responseString);

                    return (int)res.StatusCode == 200;
                }
            }
        }

        public static async Task<bool> Check(JSDocument doc) => await Check(doc.Key);

        public static async Task<bool> Check(string key)
        {
            using (HttpRequestMessage req = new (HttpMethod.Get, _serverEndPoint + key + "/exists"))
            {
                using (HttpResponseMessage res = await SendAsync(req))
                {
                    string responseString = await res.Content.ReadAsStringAsync();

                    if (WriteResponses) Console.WriteLine(responseString);

                    return (int)res.StatusCode == 200;
                }
            }
        }

        private static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req)
        {
            //Por si acaso en algun futuro tengo que modificar todas las requests que salen de la lib

            return await httpClient.SendAsync(req);
        }
    }

    public class DocumentSettings
    {
        public static int? DefaultKeyLength { get; set; }
        public static string DefaultSecret { get; set; }
        public static string DefaultPassword { get; set; }
        public static TimeSpan? DefaultLifeTime { get; set; }

        public DocumentSettings()
        {
            if (DefaultKeyLength != null) this.KeyLength = DefaultKeyLength;
            if (DefaultSecret != null) this.Secret = DefaultSecret;
            if (DefaultPassword != null) this.Password = DefaultPassword;
            if (DefaultLifeTime != null) this.LifeTime = (TimeSpan)DefaultLifeTime;
        }

        public string? Key { get; set; }
        public int? KeyLength { get; set; }
        public string? Secret { get; set; }
        public string? Password { get; set; }
        public TimeSpan LifeTime { get; set; }
    }

    public class JSDocument
    {
        private string _key { get; set; }

        public string Key => _key;

        public byte[] KeyBytes => Base64Url.FromBase64Url(_key);

        private string? _secret { get; set; }

        public string? Secret { get => _secret; }

        public string? Password { get; set; }

        public string Url => JSPasteClient.ServerEndPoint + this.Key + (string.IsNullOrEmpty(Password) ? null : "?p=" + this.Password);

        public string RawUrl => JSPasteClient.ServerEndPoint + this.Key + "/raw" + (string.IsNullOrEmpty(Password) ? null : "?p=" + this.Password);

        public async Task<string> Content() => await JSPasteClient.Get(this);

        public async Task<byte[]> ContentRaw() => await JSPasteClient.GetRaw(this);

        public async Task<bool> Remove() => await Remove(_secret);

        public async Task<bool> Remove(string secret)
        {
            if (secret == null) throw new ArgumentNullException(nameof(secret));

            return await JSPasteClient.Remove(_key, secret);
        }

        public async Task<bool> Edit(string data) => await JSPasteClient.Edit(this ,data);
        public async Task<bool> Edit(byte[] data) => await JSPasteClient.Edit(this ,data);

        public async Task<bool> Edit(string data, string secret, string? password = null) => await JSPasteClient.Edit(_key, secret,password,data);
        public async Task<bool> Edit(byte[] data, string secret, string? password = null) => await JSPasteClient.Edit(_key, secret, password, data);

        public async Task<bool> Check() => await JSPasteClient.Check(this);

        public JSDocument(string key, string? secret = null, string? password = null)
        {
            _key = key;
            _secret = secret;
            Password = password;
        }

        public JSDocument(byte[] key, string? secret = null, string? password = null)
        {
            _key = Base64Url.ToBase64Url(key);
            _secret = secret;
            Password = password;
        }

        public override string ToString() => ToStringAsync().Result;

        public async Task<string> ToStringAsync() => await this.Content();

        public override int GetHashCode() => GetHashCodeAsync().Result;

        public async Task<int> GetHashCodeAsync()
        {
            using (MD5 hashAlgorithm = MD5.Create())
            {
                return BitConverter.ToInt32(hashAlgorithm.ComputeHash(await this.ContentRaw()), 0);
            }
        }
    }

    internal static class Base64Url
    {
        public static string ToBase64Url(byte[] data) => Convert.ToBase64String(data).Trim('=').Replace('+', '-').Replace('/', '_');

        public static byte[] FromBase64Url(string data) => Convert.FromBase64String(data.Replace('_', '/').Replace('-', '+').PadRight(data.Length + (4 - data.Length % 4) % 4, '='));
    }
}