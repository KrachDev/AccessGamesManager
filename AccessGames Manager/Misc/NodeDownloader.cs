using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AccessGamesManager.Misc
{
    /// <summary>
    /// Downloads and manages Node.js runtime
    /// Downloads once, caches locally, extracted and ready to use
    /// </summary>
    public static class NodeDownloader
    {
        // Node.js version to download
        private const string NODE_VERSION = "v20.11.1";
        private const string NODE_DOWNLOAD_URL = "https://nodejs.org/dist/v20.11.1/node-v20.11.1-win-x64.zip";
        
        private static readonly string _nodeDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccessGames",
            "node"
        );

        // node.zip is downloaded here then deleted after extraction
        private static readonly string _zipPath = Path.Combine(_nodeDir, "node.zip");

        // Cached result of FindNodeExe() so we don't scan every call
        private static string? _resolvedNodeExe = null;

        /// <summary>
        /// Find node.exe: system PATH first, then cached download, then scan download dir.
        /// Returns null if not found anywhere.
        /// </summary>
        public static string? FindNodeExe()
        {
            if (_resolvedNodeExe != null && File.Exists(_resolvedNodeExe))
                return _resolvedNodeExe;

            // 1. Check system PATH (user already has Node installed)
            try
            {
                var result = Process.Start(new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "node",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });
                string? line = result?.StandardOutput.ReadLine()?.Trim();
                result?.WaitForExit();
                if (!string.IsNullOrEmpty(line) && File.Exists(line))
                {
                    Console.WriteLine($"✓ System Node.js found: {line}");
                    _resolvedNodeExe = line;
                    return line;
                }
            }
            catch { }

            // 2. Check the exact flat path (for users who had the old version)
            string flat = Path.Combine(_nodeDir, "node.exe");
            if (File.Exists(flat))
            {
                _resolvedNodeExe = flat;
                return flat;
            }

            // 3. Scan one level deep inside _nodeDir (handles node-vXX.XX-win-x64\ subfolder)
            if (Directory.Exists(_nodeDir))
            {
                foreach (var sub in Directory.GetDirectories(_nodeDir))
                {
                    string candidate = Path.Combine(sub, "node.exe");
                    if (File.Exists(candidate))
                    {
                        Console.WriteLine($"✓ Downloaded Node.js found: {candidate}");
                        _resolvedNodeExe = candidate;
                        return candidate;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Check if Node.js is already available (system or downloaded)
        /// </summary>
        public static bool IsNodeReady()
        {
            return FindNodeExe() != null;
        }

        /// <summary>
        /// Download Node.js if not already cached
        /// Returns true if ready, false if download needed
        /// </summary>
        public static bool CheckNode()
        {
            if (IsNodeReady())
            {
                Console.WriteLine("✓ Node.js already cached and ready");
                return true;
            }

            Console.WriteLine("⚠ Node.js not found. Download required on first startup.");
            return false;
        }

        /// <summary>
        /// Download Node.js with progress reporting
        /// </summary>
        public static async Task<bool> DownloadNodeAsync(IProgress<(int percent, string status)> progress)
        {
            try
            {
                Directory.CreateDirectory(_nodeDir);

                Console.WriteLine($"Downloading Node.js {NODE_VERSION}...");
                progress?.Report((0, "Starting download..."));

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10);

                    // Get file with progress
                    var response = await client.GetAsync(NODE_DOWNLOAD_URL, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var canReportProgress = totalBytes != -1;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = File.Create(_zipPath))
                    {
                        var totalRead = 0L;
                        var buffer = new byte[8192];
                        int read;

                        while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, read);
                            totalRead += read;

                            if (canReportProgress)
                            {
                                var percent = (int)((totalRead * 100) / totalBytes);
                                progress?.Report((percent, $"Downloading... {FormatBytes(totalRead)} / {FormatBytes(totalBytes)}"));
                            }
                        }
                    }
                }

                Console.WriteLine("Download complete. Extracting...");
                progress?.Report((90, "Extracting files..."));

                // Extract ZIP
                await ExtractZipAsync(_zipPath, _nodeDir);

                // Clean up ZIP
                File.Delete(_zipPath);

                // Invalidate cache so FindNodeExe rescans after extraction
                _resolvedNodeExe = null;

                if (FindNodeExe() == null)
                {
                    Console.WriteLine("ERROR: Node.exe not found after extraction");
                    return false;
                }

                Console.WriteLine($"✓ Node.js ready at: {FindNodeExe()}");
                progress?.Report((100, "Done! Node.js is ready."));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error downloading Node.js: {ex.Message}");
                progress?.Report((0, $"Error: {ex.Message}"));
                return false;
            }
        }

        /// <summary>
        /// Extract ZIP file using built-in Windows support
        /// </summary>
        private static async Task ExtractZipAsync(string zipPath, string extractPath)
        {
            await Task.Run(() =>
            {
                try
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath, overwriteFiles: true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Extraction error: {ex.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// Get the path to node.exe (system or downloaded). Throws if not found.
        /// </summary>
        public static string GetNodeExePath()
        {
            return FindNodeExe() ?? throw new FileNotFoundException("Node.js not found. Call IsNodeReady() first.");
        }

        /// <summary>
        /// Get the Node.js installation directory
        /// </summary>
        public static string GetNodeDir()
        {
            return _nodeDir;
        }

        /// <summary>
        /// Format bytes to human readable
        /// </summary>
        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
