using System;

namespace alerting
{
    public static class Logger
    {
        public static void LogInfo(string message)
        {
            Console.WriteLine($"{DateTime.UtcNow:o}: {message}");
        }

        public static void LogError(string message)
        {
            Console.WriteLine($"{DateTime.UtcNow:o}: ERR: {message}", ConsoleColor.DarkRed);
        }
    }
}
