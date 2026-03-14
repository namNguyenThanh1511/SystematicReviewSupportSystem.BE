using Serilog;

namespace Shared.Logs
{
    public static class LogExceptions //mark as static ( no DI needed )
    {
        public static void LogException(Exception ex)
        {
            LogToFile(ex.Message);
            LogToConsole(ex.Message);
            LogToDebugger(ex.Message);
        }
        public static void LogToFile(string message)
        {
            Log.Information(message);
        }
        public static void LogToConsole(string message)
        {
            Log.Warning(message);
        }
        public static void LogToDebugger(string message)
        {
            Log.Debug(message);
        }


    }
}
