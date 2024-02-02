using JSPaste.Net;
using System.Diagnostics;
using System.Text;

namespace JSPaste_Tests
{
    internal class Program
    {
        //No esperes nada profesional en tests
        private static void Main(string[] args)
        {
            JSPasteClient.ServerEndPoint = "http://[::1]:4000";

            var data = File.ReadAllBytes("C:\\Users\\Mrgaton\\Downloads\\frame_0_delay-0.1s.png");

            DocumentSettings settings = new DocumentSettings()
            {
                LifeTime = TimeSpan.Zero,
                Password = "jeje",
                DesiredSecret = "COME PINGAS"
            };

            var res = JSPasteClient.Send(data, settings).Result;


            Console.WriteLine();
            foreach(var b in res.KeyBytes)
            {
                Console.Write(b + ", ");
            }
            Console.WriteLine();

            Process.Start(new ProcessStartInfo()
            {
                FileName = res.Url,
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