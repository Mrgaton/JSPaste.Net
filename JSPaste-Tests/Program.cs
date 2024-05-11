using JSPasteNet;
using System.Diagnostics;
using System.Text;

namespace JSPaste_Tests
{
    internal static class Program
    {
        //No esperes nada profesional en tests
        private static void Main(string[] args)
        {
            JSPasteNet.JSPasteClient.ServerEndPoint = "http://[::1]:4000";
            //JSPasteNet.JSPasteClient.ServerEndPoint = "https://api.inetol.net/jspaste/v2";
            
            //var data = File.ReadAllBytes("C:\\Users\\Mrgaton\\Mega\\Programas\\Programas de CSharp\\AsciiPlayer\\AsciiPlayer\\bin\\Debug\\buffer.txt");
            var data = File.ReadAllBytes("C:\\Users\\mrgaton\\Downloads\\IMG20240415184420.jpg");

            DocumentSettings.DefaultLifeTime = TimeSpan.FromMinutes(1);
            DocumentSettings.DefaultKeyLength = 20;
            DocumentSettings.DefaultSecret = "un chiste";
            DocumentSettings.DefaultPassword = "chiste";

            var res = JSPasteClient.Publish(data, new()
            {
                LifeTime = TimeSpan.MaxValue,
                //Key = "gato-con-gafas"
            }).Result;

            Console.WriteLine();

            var serverData = res.ContentRaw().Result;

            if (!serverData.SequenceEqual(data)) throw new Exception("raw roto de nuevo 💨");

            Console.WriteLine(res.Content().Result);
            Console.WriteLine(res.GetHashCode());

            //var a = res.Edit("hola").Result;

            Thread.Sleep(0);
            Console.WriteLine(res.Content().Result);

            Console.WriteLine(res.GetHashCode());

            /*Process.Start(new ProcessStartInfo()
            {
                FileName = res.RawUrl,
                UseShellExecute = true
            });*/

            Console.WriteLine(res.Check().Result);


            if (res.RawUrl.StartsWith("https://",StringComparison.InvariantCultureIgnoreCase))
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = res.RawUrl,
                    UseShellExecute = true
                });
            }

            Console.WriteLine(res.RawUrl);

            /*Console.ReadLine();

            for (int i = 0; i < 929; i++)
            {
                var doc = res.ContentRaw().Result;

                bool re = JSPasteClient.Edit(res, Encoding.UTF8.GetBytes(new Random().NextInt64().ToString())).Result;

                Console.WriteLine(data.Length);
                Console.WriteLine(doc.Length);
                //Console.WriteLine(Encoding.UTF8.GetString(doc));

                Thread.Sleep(1000);
            }*/
        }
    }
}