namespace Melanchall.CheckDwmApi
{
    internal static class Utilities
    {
        public static void WriteOperationTitle(
            this ReportWriter reportWriter,
            string title) =>
            reportWriter.WriteLine(title);

        public static void WriteOperationSubTitle(
            this ReportWriter reportWriter,
            string subtitle) =>
            reportWriter.WriteLine($"- {subtitle}");

        public static void WriteEventInfo(
            this ReportWriter reportWriter,
            string eventInfo) =>
            reportWriter.WriteLine($"~~~> {eventInfo}");
    }
}
