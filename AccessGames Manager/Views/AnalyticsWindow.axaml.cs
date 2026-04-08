using System;
using System.IO;

namespace AccessGames_Manager.Views
{
    /// <summary>
    /// Shows simple analytics - no UI needed, just a display
    /// </summary>
    public static class AnalyticsDisplay
    {
        public static string GetAnalyticsReport()
        {
            try
            {
                var uniqueUsers = AccessGamesManager.Misc.Analytics.GetUniqueUserCount();
                var todaysSessions = AccessGamesManager.Misc.Analytics.GetTodaySessionCount();
                var csv = AccessGamesManager.Misc.Analytics.ExportAsCSV();

                string report = $@"
════════════════════════════════════════════════════════════
                    ANALYTICS DASHBOARD
════════════════════════════════════════════════════════════

📊 STATISTICS:
  • Unique Users: {uniqueUsers}
  • Today's Sessions: {todaysSessions}

📋 DATA LOCATION:
  {AccessGamesManager.Misc.Analytics.GetDataPath()}

════════════════════════════════════════════════════════════
RECENT ACTIVITY (Last 50 entries):
════════════════════════════════════════════════════════════

{csv}
";
                return report;
            }
            catch (Exception ex)
            {
                return $"Error loading analytics: {ex.Message}";
            }
        }
    }
}
