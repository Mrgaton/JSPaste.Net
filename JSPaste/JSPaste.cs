using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JSPasteNet
{
    public static class JSPasteClient
    {
        public static Encoding DefaultEncoding = Encoding.UTF8;

#if DEBUG
        private const bool WriteResponses = true;
#else
        private const bool WriteResponses = false;
#endif

        private static string _serverEndPoint = "https://jspaste.eu/api/v2";

        public static string ServerEndPoint
        {
            get => _serverEndPoint;
            set { _serverEndPoint = !value.ToLower().Contains("/api/v") ? value.Trim().TrimEnd('/') + "/api/v2" : value.Trim().TrimEnd('/'); }
        }

        public static HttpClient httpClient { get; set; } = new HttpClient()
        {
            DefaultRequestHeaders = { { "User-Agent", "JSPaste-CS Client" } },
            Timeout = TimeSpan.FromSeconds(30)
        };

        public static async Task<JSDocument> Publish(string data, DocumentSettings? settings = default) => await Publish(data, DefaultEncoding, settings);

        public static async Task<JSDocument> Publish(string data, Encoding enc, DocumentSettings? settings = default) => await Publish(enc.GetBytes(data), settings);

        public static async Task<JSDocument> Publish(byte[] data, DocumentSettings? settings = default)
        {
            using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, _serverEndPoint + "/documents"))
            {
                req.Content = new ByteArrayContent(data);

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
            using (var req = new HttpRequestMessage(HttpMethod.Get, _serverEndPoint + "/documents/" + key + "/raw"))
            {
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
            using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Delete, _serverEndPoint + "/documents/" + key))
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

        public static async Task<bool> Edit(JSDocument doc, string data) => await Edit(data, doc.Key, doc.Secret);

        public static async Task<bool> Edit(string key, string secret, string data) => await Edit(key, secret, DefaultEncoding.GetBytes(data));

        public static async Task<bool> Edit(JSDocument doc, byte[] data) => await Edit(doc.Key, doc.Secret, data);

        public static async Task<bool> Edit(string key, string secret, byte[] data)
        {
            using (HttpRequestMessage req = new HttpRequestMessage(new HttpMethod("PATCH"), _serverEndPoint + "/documents/" + key))
            {
                req.Content = new ByteArrayContent(data);

                req.Headers.TryAddWithoutValidation("secret", secret);

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
            using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, _serverEndPoint + "/documents/" + key + "/exists"))
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
        private static int DefaultKeyLength = int.MinValue;
        private static string DefaultSecret = null;
        private static string DefaultPassword = null;
        private static TimeSpan DefaultLifeTime = TimeSpan.MinValue;

        public DocumentSettings()
        {
            if (DefaultKeyLength > 0) this.KeyLength = DefaultKeyLength;
            if (DefaultSecret != null) this.Secret = DefaultSecret;
            if (DefaultPassword != null) this.Password = DefaultPassword;
            if (DefaultLifeTime > TimeSpan.MinValue) this.LifeTime = DefaultLifeTime;
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

        public string Url => JSPasteClient.ServerEndPoint + "/documents/" + this.Key + (string.IsNullOrEmpty(Password) ? null : "/?p=" + this.Password);

        public string RawUrl => JSPasteClient.ServerEndPoint + "/documents/" + this.Key + "/raw" + (string.IsNullOrEmpty(Password) ? null : "/?p=" + this.Password);

        public async Task<string> Data() => await JSPasteClient.Get(this);

        public async Task<byte[]> DataRaw() => await JSPasteClient.GetRaw(this);

        public async Task<bool> Remove() => await Remove(_secret);

        public async Task<bool> Remove(string secret)
        {
            if (secret == null) throw new ArgumentNullException(nameof(secret));

            return await JSPasteClient.Remove(_key, secret);
        }

        public async Task<bool> Edit(string data, string secret) => await Edit(Encoding.UTF8.GetBytes(data), secret);

        public async Task<bool> Edit(byte[] data, string secret)
        {
            if (secret == null) throw new ArgumentNullException(nameof(secret));

            return await JSPasteClient.Edit(_key, secret, data);
        }

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
    }

    internal static class Base64Url
    {
        public static string ToBase64Url(byte[] data) => Convert.ToBase64String(data).Trim('=').Replace('+', '-').Replace('/', '_');

        public static byte[] FromBase64Url(string data) => Convert.FromBase64String(data.Replace('_', '/').Replace('-', '+').PadRight(data.Length + (4 - data.Length % 4) % 4, '='));
    }
}