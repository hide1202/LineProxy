using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ForwordProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            var listener = new GateListener(8081);

            try
            {
                listener.Start().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.ReadKey();

            listener.Stop();
        }
    }
}