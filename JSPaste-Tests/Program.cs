using JSPasteNet;
using System.Text;

namespace JSPaste_Tests
{
    internal class Program
    {
        //No esperes nada profesional en tests
        private static void Main(string[] args)
        {
         
                JSPasteClient.ServerEndPoint = "http://[::1]:4000";

            var data = File.ReadAllBytes("C:\\Users\\Mrgaton\\Downloads\\SKlauncher-3.2.exe");

            DocumentSettings settings = new DocumentSettings()
            {
                Password = "whatt",
                Secret = "COME PINGAS",
                KeyLength = 2
            };


            var res = JSPasteClient.Publish(data, settings).Result;

            Console.WriteLine();

            foreach (var b in res.KeyBytes)
            {
                Console.Write(b + ", ");
            }
            Console.WriteLine();

            /*Process.Start(new ProcessStartInfo()
            {
                FileName = res.RawUrl,
                UseShellExecute = true
            });*/

            Console.WriteLine(res.Check().Result);

            Console.WriteLine(res.Key);
            Console.ReadLine();
            for (int i = 0; i < 929; i++)
            {
                var doc = res.DataRaw().Result;

                bool re = JSPasteClient.Edit(res, Encoding.UTF8.GetBytes(new Random().NextInt64().ToString())).Result;

                Console.WriteLine(data.Length);
                Console.WriteLine(doc.Length);
                //Console.WriteLine(Encoding.UTF8.GetString(doc));

                Thread.Sleep(1000);
            }
        }
    }
}