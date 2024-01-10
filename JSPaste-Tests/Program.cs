using JSPaste.Net;

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

            for(int i = 0; i < 929; i++)
            {
                var doc = res.DataRaw().Result;

                Console.WriteLine(data.Length);
                Console.WriteLine(doc.Length);
                Console.WriteLine(data.SequenceEqual(doc));
            }
        }
    }
}