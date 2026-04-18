using Avalonia;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using AccessGamesManager.Misc;

namespace AccessGames_Manager
{
    internal sealed class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Record app session for analytics
            Analytics.RecordSession();

            // Clean up Node.js server on app exit
            AppDomain.CurrentDomain.ProcessExit += (s, e) => NodeServerManager.StopServer();

            // ── Self-replace bootstrap ────────────────────────────────────────
            // When the auto-updater downloads a new exe it launches it with:
            //   --update-replace "<path-to-old-exe>"
            // We wait for the old process to release the file, copy ourselves
            // over it, then relaunch from the original path and exit.
            if (args.Length >= 2 && args[0] == "--update-replace")
            {
                string oldExePath = args[1];
                string newExePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";

                // Wait up to 10 s for the old exe to be released
                for (int i = 0; i < 50; i++)
                {
                    try
                    {
                        File.Copy(newExePath, oldExePath, overwrite: true);
                        break;
                    }
                    catch (IOException) { Thread.Sleep(200); }
                }

                // Relaunch from the now-updated original path
                try { Process.Start(new ProcessStartInfo { FileName = oldExePath, UseShellExecute = true }); }
                catch { }
                return; // exit the temp copy
            }

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
