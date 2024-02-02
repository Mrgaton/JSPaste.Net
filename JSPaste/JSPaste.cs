using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JSPaste.Net
{
    public static class JSPasteClient
    {
#if DEBUG
        private static bool WriteResponses = true;
#else
        private static bool WriteResponses = false;
#endif

        private static string _serverEndPoint = "https://jspaste.eu/api/v2";

        public static string ServerEndPoint
        {
            get { return _serverEndPoint; }
            set { _serverEndPoint = value.Trim().TrimEnd('/') + "/api/v2"; }
        }

        public static HttpClient httpClient { get; set; } = new HttpClient()
        {
            DefaultRequestHeaders = { { "User-Agent", "JSPaste-CS Client" } }
        };

        public static async Task<JSDocument> Send(string data, DocumentSettings? settings = default) => await Send(data, Encoding.UTF8, settings);

        public static async Task<JSDocument> Send(string data, Encoding enc, DocumentSettings? settings = default) => await Send(enc.GetBytes(data), settings);

        public static async Task<JSDocument> Send(byte[] data, DocumentSettings? settings = default)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Post, _serverEndPoint + "/documents"))
            {
                req.Content = new ByteArrayContent(data);

                if (settings != null)
                {
                    if (settings.Password != null) req.Headers.Add("password", settings.Password);
                    if (settings.LifeTime.TotalSeconds >= 0) req.Headers.Add("lifetime", ((long)settings.LifeTime.TotalSeconds).ToString());
                    if (settings.DesiredSecret != null) req.Headers.Add("secret", settings.DesiredSecret);
                }

                using (var res = await SendAsync(req))
                {
                    string responseString = await res.Content.ReadAsStringAsync();

                    if (WriteResponses) Console.WriteLine(responseString);

                    var response = MinimalJsonParser.ParseJson(responseString);

                    if ((int)res.StatusCode >= 400) throw new Exception(responseString);

                    return new JSDocument((string)response["key"], (string)response["secret"], (settings != null ? settings.Password : null));
                }
            }
        }

        public static async Task<string> Get(string key, string? password = null) => await Get(new JSDocument(key, password));

        public static async Task<string> Get(JSDocument doc) => await Get(doc, Encoding.UTF8);

        public static async Task<string> Get(string key, Encoding enc) => await Get(new JSDocument(key), enc);

        public static async Task<string> Get(JSDocument doc, Encoding enc) => enc.GetString(await GetRaw(doc));

        public static async Task<byte[]> GetRaw(JSDocument doc) => await GetRaw(doc.Key, doc.Password);

        public static async Task<byte[]> GetRaw(string key, string? password = null)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Get, _serverEndPoint + "/documents/" + key + "/raw"))
            {
                if (password != null) req.Headers.TryAddWithoutValidation("password", password);

                using (var res = await SendAsync(req))
                {
                    return await res.Content.ReadAsByteArrayAsync();
                }
            }
        }

        public static async Task<bool> Remove(JSDocument doc) => await Remove(doc.Key, doc.Secret);

        public static async Task<bool> Remove(string key, string secret)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Delete, _serverEndPoint + "/documents/" + key))
            {
                req.Headers.TryAddWithoutValidation("secret", secret);

                using (var res = await SendAsync(req))
                {
                    string responseString = await res.Content.ReadAsStringAsync();

                    if (WriteResponses) Console.WriteLine(responseString);

                    return (int)res.StatusCode == 200;
                }
            }
        }

        public static async Task<bool> Update(string data, JSDocument doc) => await Update(data, doc.Key, doc.Secret);

        public static async Task<bool> Update(string data, string key, string secret) => await Update(Encoding.UTF8.GetBytes(data), key, secret);

        public static async Task<bool> Update(byte[] data, JSDocument doc) => await Update(data, doc.Key, doc.Secret);

        public static async Task<bool> Update(byte[] data, string key, string secret)
        {
            using (var req = new HttpRequestMessage(new HttpMethod("PATCH"), _serverEndPoint + "/documents/" + key))
            {
                req.Content = new ByteArrayContent(data);

                req.Headers.TryAddWithoutValidation("secret", secret);

                using (var res = await SendAsync(req))
                {
                    string responseString = await res.Content.ReadAsStringAsync();

                    if (WriteResponses) Console.WriteLine(responseString);

                    return (int)res.StatusCode == 200;
                }
            }
        }

        private static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req)
        {
            //Por si acaso en algun futuro tengo que modificar todas las requests que salen de la libreria

            return await httpClient.SendAsync(req);
        }
    }

    public class DocumentSettings
    {
        public TimeSpan LifeTime { get; set; } = TimeSpan.MinValue;
        public string? SecretMask { get; set; }
        public string? Password { get; set; }
        public string? DesiredSecret { get; set; }
    }

    public class JSDocument
    {
        private string _key { get; set; }

        public string Key
        { get { return _key; } }

        public byte[] KeyBytes
        { 
            get 
            { 
                return Base64Url.FromBase64Url(_key);
            } 
        }

        private string? _secret { get; set; }

        public string? Secret
        { get { return _secret; } }

        public string? Password { get; set; }

        public string Url { get {  return JSPasteClient.ServerEndPoint + "/documents/" + this.Key + (string.IsNullOrEmpty(Password) ? null : "/?p=" + this.Password); } }

        public async Task<string> Data() => await JSPasteClient.Get(this);

        public async Task<byte[]> DataRaw() => await JSPasteClient.GetRaw(this);

        public async Task<bool> Remove() => await Remove(_secret);
        public async Task<bool> Remove(string secret)
        {
            if (secret == null) throw new ArgumentNullException(nameof(secret));

            return await JSPasteClient.Remove(_key, secret);
        }

        public async Task<bool> Update(string data, string secret) => await Update(Encoding.UTF8.GetBytes(data), secret);

        public async Task<bool> Update(byte[] data, string secret)
        {
            if (secret == null) throw new ArgumentNullException(nameof(secret));

            return await JSPasteClient.Update(data, _key, secret);
        }

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