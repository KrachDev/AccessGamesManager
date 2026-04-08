using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace AccessGamesManager.Misc
{
    /// <summary>
    /// Simple analytics - tracks unique users and app usage
    /// Stores data locally and can be uploaded to a server
    /// </summary>
    public static class Analytics
    {
        private static readonly string _analyticsFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccessGames",
            "analytics.txt"
        );

        private static string _machineId = "";

        static Analytics()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_analyticsFile));
                _machineId = GetOrCreateMachineId();
            }
            catch { }
        }

        /// <summary>
        /// Get a unique machine ID (based on Windows machine GUID)
        /// </summary>
        private static string GetOrCreateMachineId()
        {
            try
            {
                string idFile = Path.Combine(
                    Path.GetDirectoryName(_analyticsFile),
                    ".machine_id"
                );

                if (File.Exists(idFile))
                    return File.ReadAllText(idFile).Trim();

                // Try to get Windows machine GUID
                string machineId = GetWindowsMachineGuid();
                if (string.IsNullOrEmpty(machineId))
                    machineId = Guid.NewGuid().ToString();

                File.WriteAllText(idFile, machineId);
                return machineId;
            }
            catch { return Guid.NewGuid().ToString(); }
        }

        /// <summary>
        /// Get Windows machine GUID from registry
        /// </summary>
        private static string GetWindowsMachineGuid()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Cryptography"))
                {
                    return key?.GetValue("MachineGuid")?.ToString() ?? "";
                }
            }
            catch { return ""; }
        }

        /// <summary>
        /// Record a user session - call this on app startup
        /// </summary>
        public static void RecordSession()
        {
            try
            {
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                string line = $"SESSION|{_machineId}|{timestamp}|{GetAppVersion()}";

                lock (_analyticsFile)
                {
                    File.AppendAllText(_analyticsFile, line + Environment.NewLine);
                }

                Console.WriteLine($"[Analytics] Session recorded: {_machineId}");
            }
            catch { }
        }

        /// <summary>
        /// Record a feature usage (e.g., "OpenStore", "LaunchGame", etc)
        /// </summary>
        public static void RecordEvent(string eventName, string? details = null)
        {
            try
            {
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                string line = $"EVENT|{_machineId}|{timestamp}|{eventName}|{details ?? ""}";

                lock (_analyticsFile)
                {
                    File.AppendAllText(_analyticsFile, line + Environment.NewLine);
                }
            }
            catch { }
        }

        /// <summary>
        /// Get count of unique users (unique machine IDs)
        /// </summary>
        public static int GetUniqueUserCount()
        {
            try
            {
                if (!File.Exists(_analyticsFile))
                    return 0;

                var uniqueUsers = new HashSet<string>();

                lock (_analyticsFile)
                {
                    foreach (var line in File.ReadAllLines(_analyticsFile))
                    {
                        if (line.StartsWith("SESSION|") || line.StartsWith("EVENT|"))
                        {
                            var parts = line.Split('|');
                            if (parts.Length >= 2)
                                uniqueUsers.Add(parts[1]);
                        }
                    }
                }

                return uniqueUsers.Count;
            }
            catch { return 0; }
        }

        /// <summary>
        /// Get today's session count
        /// </summary>
        public static int GetTodaySessionCount()
        {
            try
            {
                if (!File.Exists(_analyticsFile))
                    return 0;

                string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
                int count = 0;

                lock (_analyticsFile)
                {
                    foreach (var line in File.ReadAllLines(_analyticsFile))
                    {
                        if (line.StartsWith("SESSION|") && line.Contains(today))
                            count++;
                    }
                }

                return count;
            }
            catch { return 0; }
        }

        /// <summary>
        /// Export analytics data as CSV
        /// </summary>
        public static string ExportAsCSV()
        {
            try
            {
                if (!File.Exists(_analyticsFile))
                    return "No data";

                var csv = "Type,MachineID,Timestamp,Event,Details\n";

                lock (_analyticsFile)
                {
                    foreach (var line in File.ReadAllLines(_analyticsFile))
                    {
                        csv += line.Replace("|", ",") + "\n";
                    }
                }

                return csv;
            }
            catch { return "Error reading data"; }
        }

        /// <summary>
        /// Get app version
        /// </summary>
        private static string GetAppVersion()
        {
            try
            {
                var version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
                return version?.ToString() ?? "unknown";
            }
            catch { return "unknown"; }
        }

        /// <summary>
        /// Clear all analytics data (for testing)
        /// </summary>
        public static void ClearData()
        {
            try
            {
                if (File.Exists(_analyticsFile))
                    File.Delete(_analyticsFile);
            }
            catch { }
        }

        /// <summary>
        /// Get the path where analytics are stored
        /// </summary>
        public static string GetDataPath() => _analyticsFile;
    }
}
