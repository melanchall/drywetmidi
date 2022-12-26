using System;
using System.Diagnostics;
using System.Threading;

namespace RunProcdump
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting procdump...");

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "procdump",
                Arguments = "-ma -mk -e 1 -w testhost.exe -g -f ACCESS_VIOLATION"
            });

            Thread.Sleep(5000);

            if (process.HasExited)
                throw new InvalidOperationException("procdump exited");

            Console.WriteLine("procdump started, waiting for crash...");
            Console.ReadKey();
        }
    }
}
