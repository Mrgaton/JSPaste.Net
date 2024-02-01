using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JSPaste.Net
{
    public class MinimalJsonParser
    {
        //Should return dynamic but i guess that Net Standard 2.0 doest support it ¯\_(ツ)_/¯
        public static Dictionary<string, object> ParseJson(string json)
        {
            var stringsParsed = ParseElements(json);

            var result = new Dictionary<string, object>();

            foreach (var element in stringsParsed)
            {
                string content = element.Value;

                dynamic? value = null;

                if (!string.IsNullOrWhiteSpace(content))
                {
                    if (FirstAndLastEqualTo(content, '\"')) value = CleanString(CutEdges(content));
                    else if (FirstAndLastEqualTo(content, '{', '}')) value = ParseJson(content);
                    else if (FirstAndLastEqualTo(content, '[', ']')) value = ParseArray(content);
                    else if (content.All(char.IsDigit)) value = long.Parse(content);
                    else if (content.Length <= 5 && bool.TryParse(content, out bool b)) value = b;
                    else value = content;
                }

                //Console.WriteLine(element.Key + " | " + (object)value);

                result.Add(element.Key, (object)value);
            }

            return result;
        }

        private static List<object> ParseArray(string arr)
        {
            var sr = new StringStream(CutEdges(arr.Trim()));
            var elements = new List<string>();
            while (sr.LeftLength > 0) elements.Add(ReadValue(ref sr));

            List<dynamic> list = new List<dynamic>();

            if (elements.Count <= 0) return null;

            int structType = 0;

            if (FirstAndLastEqualTo(elements[0], '\"')) structType = 1;
            else if (elements[0].All(char.IsDigit)) structType = 2;
            else if (bool.TryParse(elements[0], out bool b)) structType = 3;
            else if (FirstAndLastEqualTo(elements[0], '{', '}')) structType = 4;
            else if (FirstAndLastEqualTo(elements[0], '[', ']')) structType = 5;

            foreach (var element in elements)
            {
                if (structType == 1) list.Add(CleanString(CutEdges(element)));
                else if (structType == 2) list.Add(long.Parse(element));
                else if (structType == 3) list.Add(bool.Parse(element));
                else if (structType == 4) list.Add((object)ParseJson(element));
                else if (structType == 5) list.Add(ParseArray(element));
            }

            sr.Dispose();

            return list;
        }

        private static Dictionary<string, string> ParseElements(string json)
        {
            var dict = new Dictionary<string, string>();

            var sr = new StringStream(json);

            sr.ReadUntil('{');

            while (sr.LeftLength > 0)
            {
                string key = sr.ReadBetween('\"');
                sr.ReadUntil(':');
                string value = ReadValue(ref sr);
                if (string.IsNullOrEmpty(key)) continue;
                dict.Add(key, value);
            }

            sr.Dispose();
            return dict;
        }

        private static string ReadValue(ref StringStream sr)
        {
            StringBuilder sb = new StringBuilder();

            int subElements = 0;
            char lastChar = default;
            bool inQuotes = false;

            while (sr.LeftLength > 0)
            {
                char c = sr.ReadChar();

                if (c == '\"' && lastChar != '\\') inQuotes = !inQuotes;

                if (c == ',' && subElements == 0 && !inQuotes) break;

                if (c == '{' || c == '[') subElements++;
                if (c == '}' || c == ']') subElements--;

                if (subElements < 0) break;

                sb.Append(lastChar = c);
            }

            return sb.ToString().Trim();
        }

        private static string CleanString(string str)
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < str.Length; i++) output.Append(str[i] == '\\' ? str[i += 1] : str[i]);
            return output.ToString();
        }

        private static bool FirstAndLastEqualTo(string str, char f, char s = default) => str[0] == f && str[str.Length - 1] == (s == default ? f : s);

        private static string CutEdges(string str, int lengh = 1) => str.Substring(lengh, str.Length - (lengh + 1));

        private sealed class StringStream : MemoryStream
        {
            public StringStream(string str, Encoding enc = null)
            {
                var data = (enc ?? Encoding.UTF8).GetBytes(str);
                Write(data, 0, data.Length);
                Position = 0;
            }

            public long LeftLength
            { get { return Length - Position; } }

            public char ReadChar()
            {
                int result = ReadByte();
                return result == -1 ? throw new EndOfStreamException() : (char)result;
            }

            public string ReadUntil(char c)
            {
                StringBuilder sb = new StringBuilder();
                char f;
                while (LeftLength > 0 && (f = ReadChar()) != c) sb.Append(f);
                return sb.ToString();
            }

            public string ReadBetween(char c)
            {
                ReadUntil(c);
                return ReadUntil(c);
            }
        }
    }
}