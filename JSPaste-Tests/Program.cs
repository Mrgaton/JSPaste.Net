using JSPaste.Net;
using System.Text;

namespace JSPaste_Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            JSPasteClient.ServerEndPoint = "http://[::1]:4000";

            var data = File.ReadAllBytes("C:\\Users\\Mrgaton\\Downloads\\Release.zip");

            var res = JSPasteClient.Send(data).Result;

            Console.WriteLine(res.Key);

            for (int i = 0; i < 929; i++)
            {
                var doc = res.DataRaw().Result;

                bool re = JSPasteClient.Update(Encoding.UTF8.GetBytes(new Random().NextInt64().ToString()), res).Result;

                Console.WriteLine(data.Length);
                Console.WriteLine(doc.Length);
                Console.WriteLine(Encoding.UTF8.GetString(doc));
            }
        }
    }
}