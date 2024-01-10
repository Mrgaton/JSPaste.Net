using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace JSPaste_Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string json = @"
{
    ""employee"": {
        ""name"":       ""sonoo"",
        ""salary"":      56000,
        ""married"":    true
    },
    ""cosa"": true,
    ""number"": 4949,
    ""casa"": ""hola \\  que tyal\"" "",
    ""lista"": [
    [1, 2, 3],
    [4, 5, 6],
    [7, 8, 9]
]
}
";

            string x = "hola que tal  \\\" xd deveria funcionar\\\\";

            var result = MinimalJsonParser.ParseJson(json);

            /*foreach (var element in result)
            {
                var obj = element.Value;

                if (obj == null)
                {
                    Console.WriteLine("null");
                    continue;
                }

                Console.WriteLine(obj.GetType());
            }

            Console.WriteLine(result);*/


            /*foreach(var subList in result["lista"])
            {
                foreach (var nums in subList)
                {
                    Console.Write(nums + ", ");
                }
                Console.WriteLine();
            }*/

            //Console.WriteLine(result["lista"][0]);
            Console.ReadLine();
        }


     
    }
}