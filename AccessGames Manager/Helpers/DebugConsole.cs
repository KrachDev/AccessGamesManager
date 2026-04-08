using System;
using System.Diagnostics;
using System.IO;

namespace AccessGamesManager.Helpers
{
    public static class DebugConsole
    {
        private static bool _isEnabled = true;
        private static readonly object _lock = new();

        // Log file at %AppData%\AccessGames\debug.log
        private static readonly string _logFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccessGames", "debug.log");

        static DebugConsole()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_logFile)!);
                // Keep log under 2 MB — truncate on startup if bigger
                if (File.Exists(_logFile) && new FileInfo(_logFile).Length > 2 * 1024 * 1024)
                    File.WriteAllText(_logFile, "");
                File.AppendAllText(_logFile, $"\n{'=',60}\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] App started\n");
            }
            catch { }
        }

        /// <summary>
        /// Enable or disable console debugging
        /// </summary>
        public static void Enable(bool enable = true)
        {
            _isEnabled = enable;
        }

        /// <summary>
        /// Check if console debugging is enabled
        /// </summary>
        public static bool IsEnabled => _isEnabled;

        /// <summary>
        /// Write a message to debug output if enabled
        /// </summary>
        public static void WriteLine(string message = "", string title = "DATA")
        {
            if (!_isEnabled) return;
            Log(message, title);
        }

        /// <summary>
        /// Internal log method: writes to Debug output, Console, and a log file.
        /// </summary>
        private static void Log(string message, string title)
        {
            if (!_isEnabled) return;
            lock (_lock)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    string output = $"[{timestamp} | {title}] {message}";

                    // 1. VS Output window (Debug builds only, silent in Release)
                    Debug.WriteLine(output);

                    // 2. Console (visible if a console window is attached)
                    Console.WriteLine(output);

                    // 3. File — always works regardless of build config
                    try { File.AppendAllText(_logFile, output + Environment.NewLine); }
                    catch { }
                }
                catch { }
            }
        }

        /// <summary>
        /// Write an info message
        /// </summary>
        public static void WriteInfo(string message, string title = "INFO")
        {
            Log(message, title);
        }

        /// <summary>
        /// Write a warning message
        /// </summary>
        public static void WriteWarning(string message, string title = "WARNING")
        {
            Log(message, title);
        }

        /// <summary>
        /// Write an error message
        /// </summary>
        public static void WriteError(string message, string title = "ERROR")
        {
            Log(message, title);
        }

        /// <summary>
        /// Write an exception with full details
        /// </summary>
        public static void WriteException(Exception ex, string context = "")
        {
            if (!_isEnabled) return;
            string msg = $"{(!string.IsNullOrEmpty(context) ? $"{context}: " : "")}{ex.Message}\n[STACK TRACE] {ex.StackTrace}";
            Log(msg, "EXCEPTION");
        }

        /// <summary>
        /// Write a success message
        /// </summary>
        public static void WriteSuccess(string message, string title = "SUCCESS")
        {
            Log(message, title);
        }

        /// <summary>
        /// Write a debug message
        /// </summary>
        public static void WriteDebug(string message, string title = "DEBUG")
        {
            Log(message, title);
        }

        /// <summary>
        /// Write a separator line
        /// </summary>
        public static void WriteSeparator(char character = '=', int length = 50)
        {
            if (!_isEnabled) return;
            string line = new string(character, length);
            Debug.WriteLine(line);
            Console.WriteLine(line);
            try { File.AppendAllText(_logFile, line + Environment.NewLine); } catch { }
        }

        /// <summary>
        /// Write a section header
        /// </summary>
        public static void WriteSection(string title)
        {
            if (!_isEnabled) return;
            WriteSeparator('=', 60);
            Log($"  {title.ToUpper()}", "SECTION");
            WriteSeparator('=', 60);
        }

        /// <summary>
        /// Path of the log file on disk.
        /// </summary>
        public static string LogFilePath => _logFile;

        /// <summary>
        /// Opens the log file in Notepad (useful for a debug menu button).
        /// </summary>
        public static void OpenLogFile()
        {
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = _logFile, UseShellExecute = true }); }
            catch { }
        }

        /// <summary>
        /// Clear the console (no-op for Debug output)
        /// </summary>
        public static void Clear()
        {
            if (!_isEnabled) return;
            Debug.WriteLine("=".PadRight(60, '='));
            Debug.WriteLine("Console cleared");
            Debug.WriteLine("=".PadRight(60, '='));
        }

        /// <summary>
        /// Write key-value pairs
        /// </summary>
        public static void WriteKeyValue(string key, object? value)
        {
            WriteLine($"{key}: {value ?? "null"}");
        }
    }
}
