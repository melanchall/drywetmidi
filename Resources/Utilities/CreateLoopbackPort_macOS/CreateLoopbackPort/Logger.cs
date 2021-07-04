using System.IO;

namespace CreateLoopbackPort
{
    internal static class Logger
    {
        public static void Write(string text)
        {
            using (var sw = File.AppendText("loopback.log"))
            {
                sw.Write(text);
            }
        }

        public static void WriteLine(string text)
        {
            using (var sw = File.AppendText("loopback.log"))
            {
                sw.WriteLine(text);
            }
        }
    }
}
