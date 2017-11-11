using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LineProxy
{
    public static class Extensions
    {
        #region NetworkStream

        public static async Task<string> ReadStringAsync(this NetworkStream stream, byte[] buffer)
        {
            var read = await stream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.Default.GetString(buffer, 0, read);
        }

        #endregion
    }
}