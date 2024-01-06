using System;
using System.Data.Common;
using System.Net.Http;
using System.Text;

namespace JSPaste
{
    public class JSPaste
    {
        private readonly const string ServerEndPoint = "https://jspaste.eu/";

        public static HttpClient httpClient = new HttpClient();
        public static void Send(string data) => Send(data,Encoding.UTF8);
        public static void Send(string data, Encoding enc) => Send(enc.GetBytes(data));
        public static void Send(byte[] data)
        {

        }
    }

    public class JSDocument
    {
        private string _key { get; set; }
        public string Key { get { return _key;  } }

        private string _secret { get; set; }
        public string Secret { get { return _secret; } }
        public JSDocument(string key, string secret) 
        {
            _key = key;
            _secret = secret;
        }
    }
}
