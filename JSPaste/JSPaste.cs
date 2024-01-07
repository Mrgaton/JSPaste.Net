using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace JSPaste.Net
{
    public class JSPasteClient
    {
        private const string ServerEndPoint = "https://jspaste.eu/";

        public static HttpClient httpClient = new HttpClient()
        {
            BaseAddress = new Uri(ServerEndPoint)
        };

        public static void Send(string data, DocumentSettings settings = default) => Send(data, Encoding.UTF8, settings);

        public static void Send(string data, Encoding enc, DocumentSettings settings = default) => Send(enc.GetBytes(data), settings);

        public static void Send(byte[] data, DocumentSettings settings = default)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Get, "/documents"))
            {
                req.Version = HttpVersion.Version20;
            }
        }

        public static string Get(string key) => Get(key, Encoding.UTF8);

        public static string Get(string key, Encoding enc) => enc.GetString(GetRaw(key));

        public static byte[] GetRaw(string key)
        {
            return null;
        }

        public static bool Remove(string key, string secret)
        {
            return false;
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
        public string Key
        { get { return _key; } }

        private string _secret { get; set; }
        public string Secret
        { get { return _secret; } }

        public JSDocument(string key, string secret)
        {
            _key = key;
            _secret = secret;
        }
    }
}