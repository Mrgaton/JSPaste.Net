using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JSPaste.Net
{
    public static class JSPasteClient
    {
        private const string ServerEndPoint = "https://jspaste.eu/";

        public static HttpClient httpClient { get; set; } = new HttpClient()
        {
            BaseAddress = new Uri(ServerEndPoint),
            DefaultRequestHeaders = { { "User-Agent", "JSPaste-CS Client" } }
        };

        public static void Send(string data, DocumentSettings settings = default) => Send(data, Encoding.UTF8, settings);

        public static void Send(string data, Encoding enc, DocumentSettings settings = default) => Send(enc.GetBytes(data), settings);

        public static void Send(byte[] data, DocumentSettings settings = default)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Get, "/documents"))
            {
            }
        }

        public static async Task<string> Get(string key) => await Get(key, Encoding.UTF8);

        public static async Task<string> Get(string key, Encoding enc) => enc.GetString(await GetRaw(key));

        public static async Task<byte[]> GetRaw(string key)
        {
            return null;
        }

        public static async Task<bool> Remove(string key, string secret)
        {
            return false;
        }

        private static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req)
        {
            //Por si acaso en algun futuro tengo que modificar todas las requests que salen de la libreria

            req.Version = HttpVersion.Version20;

            return await httpClient.SendAsync(req);
        }
    }

    public class DocumentSettings
    {
        public static TimeSpan LiveSpan { get; set; } = TimeSpan.Zero;
        public static string SecretMask { get; set; }
        public DocumentSettings()
        { }
    }

    public class JSDocument
    {
        private string _key { get; set; }
        public string Key  { get { return _key; } }

        private string _secret { get; set; }
        public string Secret { get { return _secret; } }

        public JSDocument(string key, string secret)
        {
            _key = key;
            _secret = secret;
        }
    }
}