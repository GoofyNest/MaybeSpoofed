using System;

namespace MaybeSpoofed
{
    public static class Custom
    {
        public static void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
        {
            var prefix = "";
            switch (color)
            {
                case ConsoleColor.Yellow:
                case ConsoleColor.DarkYellow:
                    prefix = "[WARN] ";
                    break;

                case ConsoleColor.Magenta:
                case ConsoleColor.DarkMagenta:
                    prefix = "[DEBUG] ";
                    break;

                case ConsoleColor.Red:
                case ConsoleColor.DarkRed:
                    prefix = "[ERROR] ";
                    break;

                case ConsoleColor.Cyan:
                    prefix = "[NOTIFY] ";
                    break;

                case ConsoleColor.Blue:
                case ConsoleColor.DarkBlue:
                    prefix = "[INFO] ";
                    break;

            }

            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = "[NORMAL] ";
                color = ConsoleColor.Green;
            }

            Console.ForegroundColor = color;
            Console.Write(prefix);
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
            Console.ResetColor();

            Console.WriteLine($"{message}");

        }
    }
}