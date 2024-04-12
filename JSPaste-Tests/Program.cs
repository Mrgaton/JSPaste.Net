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

            var data = "RFNGaWxlcy5kZXBzLmpzb24:HwDEuKFYLw76_x8CBDlUXI2cBOQB/-v8fAgQ5VFy1HAR4AQ:biAEao-VEhA:yMIXhemQjfynk3XXsFa45rx3wg3ADFLkqxE3p6PNBtnwhU5Qsall9M8y8XQBuN8K9wwR"; //File.ReadAllBytes("C:\\Users\\Mrgaton\\Downloads\\SKlauncher-3.2.exe");

            //DocumentSettings.DefaultLifeTime = TimeSpan.FromMinutes(1);
            DocumentSettings.DefaultKeyLength = 20;
            DocumentSettings.DefaultSecret = "un chiste";
            DocumentSettings.DefaultPassword = "chiste";

            var res = JSPasteClient.Publish(data, new()
            {
                LifeTime = TimeSpan.MaxValue,
            }).Result;

            Console.WriteLine();

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

            Process.Start(new ProcessStartInfo()
            {
                FileName = res.Url,
                UseShellExecute = true
            });

            Console.ReadLine();

            for (int i = 0; i < 929; i++)
            {
                var doc = res.ContentRaw().Result;

                bool re = JSPasteClient.Edit(res, Encoding.UTF8.GetBytes(new Random().NextInt64().ToString())).Result;

                Console.WriteLine(data.Length);
                Console.WriteLine(doc.Length);
                //Console.WriteLine(Encoding.UTF8.GetString(doc));

                Thread.Sleep(1000);
            }
        }
    }
}