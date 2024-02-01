using JSPaste.Net;
using System.Diagnostics;
using System.Text;

namespace JSPaste_Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            JSPasteClient.ServerEndPoint = "http://[::1]:4000";

            var data = File.ReadAllBytes("C:\\Users\\Mrgaton\\Downloads\\teredoFix.bat");

            DocumentSettings settings = new DocumentSettings()
            {
                LifeTime = TimeSpan.Zero,
                Password = "jeje",
                DesiredSecret = "COME PINGAS"
            };

            var res = JSPasteClient.Send(data, settings).Result;

            Process.Start(new ProcessStartInfo()
            {
                FileName = JSPasteClient.ServerEndPoint + "/documents/" + res.Key + "/?p=" + settings.Password,
                UseShellExecute = true
            });

            Console.WriteLine(res.Key);
            Console.ReadLine();
            for (int i = 0; i < 929; i++)
            {
                var doc = res.DataRaw().Result;

                bool re = JSPasteClient.Update(Encoding.UTF8.GetBytes(new Random().NextInt64().ToString()), res).Result;

                Console.WriteLine(data.Length);
                Console.WriteLine(doc.Length);
                //Console.WriteLine(Encoding.UTF8.GetString(doc));

                Thread.Sleep(1000);
            }
        }
    }
}