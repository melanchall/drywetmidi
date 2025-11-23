namespace Melanchall.CheckDwmApi
{
    internal interface ITask
    {
        string GetTitle();

        string GetDescription();

        void Execute(ToolOptions toolOptions, ReportWriter reportWriter);
    }
}
