using JSPaste.Net;

namespace JSPaste_Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var res = JSPasteClient.Send("diosss").Result;

            Console.WriteLine(res.Key);

            var doc = res.Data().Result;
        }
    }
}