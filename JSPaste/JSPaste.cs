using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JSPaste.Net
{
    public static class JSPasteClient
    {
        private static bool WriteResponses = true;

        public static string ServerEndPoint { get; set; } = "https://jspaste.eu";

        public static HttpClient httpClient { get; set; } = new HttpClient()
        {
            DefaultRequestHeaders = { { "User-Agent", "JSPaste-CS Client" } }
        };

        public static async Task<JSDocument> Send(string data, DocumentSettings settings = default) => await Send(data, Encoding.UTF8, settings);

        public static async Task<JSDocument> Send(string data, Encoding enc, DocumentSettings settings = default) => await Send(enc.GetBytes(data), settings);

        public static async Task<JSDocument> Send(byte[] data, DocumentSettings settings = default)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Post, ServerEndPoint + "/documents"))
            {
                req.Content = new ByteArrayContent(data);

                //if (settings.LiveSpan.TotalSeconds > 0) throw new Exception("Not implemented yet");

                using (var res = await SendAsync(req))
                {
                    string responseString = await res.Content.ReadAsStringAsync();

                    if (WriteResponses) Console.WriteLine(responseString);

                    var response = MinimalJsonParser.ParseJson(responseString);

                    return new JSDocument((string)response["key"], (string)response["secret"]);
                }
            }
        }

        public static async Task<string> Get(string key) => await Get(new JSDocument(key));

        public static async Task<string> Get(JSDocument doc) => await Get(doc, Encoding.UTF8);

        public static async Task<string> Get(string key, Encoding enc) => await Get(new JSDocument(key), enc);

        public static async Task<string> Get(JSDocument doc, Encoding enc) => enc.GetString(await GetRaw(doc));

        public static async Task<byte[]> GetRaw(string key) => await GetRaw(new JSDocument(key));

        public static async Task<byte[]> GetRaw(JSDocument doc)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Get, ServerEndPoint + "/documents/" + doc.Key + "/raw"))
            {
                using (var res = await SendAsync(req))
                {
                    return await res.Content.ReadAsByteArrayAsync();
                }
            }
        }

        public static async Task<bool> Remove(JSDocument doc) => await Remove(doc.Key, doc.Secret);

        public static async Task<bool> Remove(string key, string secret)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Delete, ServerEndPoint + "documents/" + key))
            {
                req.Headers.TryAddWithoutValidation("secret", secret);

                using (var res = await SendAsync(req))
                {
                    string responseString = await res.Content.ReadAsStringAsync();

                    if (WriteResponses) Console.WriteLine(responseString);

                    var response = MinimalJsonParser.ParseJson(responseString);

                    return response["error"] == null;
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
        public TimeSpan LiveSpan { get; set; } = TimeSpan.Zero;
        public string? SecretMask { get; set; }
    }

    public class JSDocument
    {
        private string _key { get; set; }

        public string Key
        { get { return _key; } }

        private string _secret { get; set; }

        public string Secret
        { get { return _secret; } }

        public string Password { get; set; }

        public async Task<string> Data() => await JSPasteClient.Get(this);

        public async Task<byte[]> DataRaw() => await JSPasteClient.GetRaw(this);

        public async Task<bool> Remove() => await Remove(_secret);

        public async Task<bool> Remove(string secret)
        {
            if (secret == null) throw new ArgumentNullException("Document secret is null");

            return await JSPasteClient.Remove(_key, secret);
        }

        public JSDocument(string key, string secret = null, string acessKey = null)
        {
            _key = key;
            _secret = secret;
            Password = acessKey;
        }
    }
}