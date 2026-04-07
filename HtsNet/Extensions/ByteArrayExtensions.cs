using System.Linq;

namespace HtsNet.Extensions
{
    internal static class ByteArrayExtensions
    {
        internal static byte[] ReadFromPosition(this byte[] fullData, string position)
        {
            var positions = position.Split('-');
            var index = int.Parse(positions[0]);
            var length = (int.Parse(positions[1]) + 1) - index;
            var data = fullData.Skip(index).Take(length).ToArray();
            return data;
        }
    }
}
