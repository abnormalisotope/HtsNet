using System.Text;

namespace HtsNet.Extensions
{
    internal static class StringExtensions
    {
        internal static string[] GetFieldWithStreamType(this string str)
        {
            var streamField = new string[3];
            var values = str.Split(':');
            streamField[1] = values[1];
            var fieldType = values[0].Split('[');
            streamField[0] = fieldType[0];
            try { streamField[2] = fieldType[1].Trim(']'); } catch { }
            return streamField;
        }
        internal static byte[] ToByteArray(this string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }
    }
}
