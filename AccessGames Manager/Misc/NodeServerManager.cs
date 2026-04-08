using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AccessGamesManager.Helpers;

namespace AccessGamesManager.Misc
{
    /// <summary>
    /// Manages the Node.js backend server process
    /// </summary>
    public class NodeServerManager
    {
        private static Process? _serverProcess;
        private const string SERVER_URL = "http://localhost:3000";
        private const int CHECK_TIMEOUT_MS = 5000;

        /// <summary>
        /// Check if the Node.js server is running and accessible
        /// </summary>
        public static async Task<bool> IsServerRunningAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMilliseconds(1000);
                    var response = await client.GetAsync($"{SERVER_URL}/");
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Start the Node.js server if it's not already running
        /// </summary>
        public static async Task<bool> StartServerAsync()
        {
            try
            {
                DebugConsole.WriteSection("NODE.JS SERVER STARTUP");

                // Check if already running
                if (await IsServerRunningAsync())
                {
                    DebugConsole.WriteSuccess("Node.js server already running on port 3000");
                    return true;
                }

                DebugConsole.WriteInfo("Starting Node.js server...");

                // Get the AccessGamesWeb directory path
                string baseDir = AppContext.BaseDirectory;
                DebugConsole.WriteKeyValue("Base directory", baseDir);

                string webPath = Path.Combine(Path.GetDirectoryName(baseDir), "AccessGamesWeb");
                DebugConsole.WriteKeyValue("Web path (primary)", webPath);
                DebugConsole.WriteKeyValue("Web path exists", Directory.Exists(webPath));

                if (!Directory.Exists(webPath))
                {
                    // Try alternate path in solution
                    webPath = Path.Combine(baseDir, "AccessGamesWeb");
                    DebugConsole.WriteKeyValue("Web path (alternate)", webPath);
                    DebugConsole.WriteKeyValue("Alternate exists", Directory.Exists(webPath));

                    if (!Directory.Exists(webPath))
                    {
                        DebugConsole.WriteError("AccessGamesWeb directory not found in either location");

                        // List what's in base directory
                        try
                        {
                            var dirs = Directory.GetDirectories(baseDir);
                            DebugConsole.WriteLine($"Directories available in {Path.GetFileName(baseDir)}:");
                            foreach (var d in dirs)
                                DebugConsole.WriteLine($"  - {Path.GetFileName(d)}");
                        }
                        catch (Exception ex)
                        {
                            DebugConsole.WriteError($"Could not list directories: {ex.Message}");
                        }
                        return false;
                    }
                }

                DebugConsole.WriteSuccess("✓ AccessGamesWeb directory located");

                // Check if node_modules exists
                string nodeModulesPath = Path.Combine(webPath, "node_modules");
                DebugConsole.WriteKeyValue("node_modules path", nodeModulesPath);
                DebugConsole.WriteKeyValue("node_modules exists", Directory.Exists(nodeModulesPath));

                if (!Directory.Exists(nodeModulesPath))
                {
                    DebugConsole.WriteError("node_modules not found. Run 'npm install' in AccessGamesWeb directory first.");

                    // List what's in webPath
                    try
                    {
                        var items = Directory.GetDirectories(webPath);
                        DebugConsole.WriteLine($"Contents of {Path.GetFileName(webPath)}:");
                        foreach (var item in items)
                            DebugConsole.WriteLine($"  - {Path.GetFileName(item)}/");
                    }
                    catch { }
                    return false;
                }

                DebugConsole.WriteSuccess("✓ node_modules directory found");

                // Check Node.js exe
                string nodeExe = NodeDownloader.GetNodeExePath();
                DebugConsole.WriteKeyValue("Node.exe path", nodeExe);
                DebugConsole.WriteKeyValue("Node.exe exists", File.Exists(nodeExe));

                if (!File.Exists(nodeExe))
                {
                    DebugConsole.WriteError("Node.js executable not found at expected location");
                    DebugConsole.WriteWarning("Attempting to download Node.js...");

                    var progressReporter = new Progress<(int percent, string status)>(report =>
                    {
                        DebugConsole.WriteDebug($"Download: {report.status}");
                    });

                    if (!await NodeDownloader.DownloadNodeAsync(progressReporter))
                    {
                        DebugConsole.WriteError("Failed to download Node.js");
                        return false;
                    }

                    DebugConsole.WriteSuccess("✓ Node.js downloaded successfully");
                }

                // Check server.js
                string serverJs = Path.Combine(webPath, "server.js");
                DebugConsole.WriteKeyValue("server.js path", serverJs);
                DebugConsole.WriteKeyValue("server.js exists", File.Exists(serverJs));

                if (!File.Exists(serverJs))
                {
                    DebugConsole.WriteError("server.js not found in AccessGamesWeb directory");
                    return false;
                }

                DebugConsole.WriteSuccess("✓ All paths verified");

                // Start the server process
                var psi = new ProcessStartInfo
                {
                    FileName = nodeExe,
                    Arguments = Path.Combine(webPath, "server.js"),
                    WorkingDirectory = webPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false  // Show the Node window for debugging
                };

                DebugConsole.WriteInfo("Process startup info:");
                DebugConsole.WriteKeyValue("  Filename", psi.FileName);
                DebugConsole.WriteKeyValue("  Arguments", psi.Arguments);
                DebugConsole.WriteKeyValue("  Working Dir", psi.WorkingDirectory);
                DebugConsole.WriteInfo($"Full command: {psi.FileName} {psi.Arguments}");

                _serverProcess = Process.Start(psi);

                if (_serverProcess == null)
                {
                    DebugConsole.WriteError("Failed to start Node.js server process - Process.Start returned null");
                    return false;
                }

                DebugConsole.WriteSuccess($"✓ Node.js server process started (PID: {_serverProcess.Id})");

                // Capture output streams
                _ = Task.Run(() =>
                {
                    try
                    {
                        string line;
                        while ((line = _serverProcess.StandardOutput.ReadLine()) != null)
                        {
                            if (!string.IsNullOrEmpty(line))
                                DebugConsole.WriteInfo($"[Node stdout] {line}", "NODE_OUT");
                        }
                    }
                    catch { }
                });

                _ = Task.Run(() =>
                {
                    try
                    {
                        string line;
                        while ((line = _serverProcess.StandardError.ReadLine()) != null)
                        {
                            if (!string.IsNullOrEmpty(line))
                                DebugConsole.WriteError($"[Node stderr] {line}", "NODE_ERR");
                        }
                    }
                    catch { }
                });

                // Wait for server to be accessible
                var startTime = DateTime.Now;
                int attempts = 0;
                while ((DateTime.Now - startTime).TotalMilliseconds < CHECK_TIMEOUT_MS)
                {
                    attempts++;
                    if (await IsServerRunningAsync())
                    {
                        DebugConsole.WriteSuccess($"✓ Node.js server is accessible on http://localhost:3000 (after {attempts} attempts)");
                        DebugConsole.WriteSeparator('=', 60);
                        return true;
                    }
                    await Task.Delay(500);
                }

                DebugConsole.WriteWarning("Server started but not responding after 5 seconds. Checking process status...");

                if (_serverProcess.HasExited)
                {
                    DebugConsole.WriteError($"✗ Server process exited unexpectedly with code: {_serverProcess.ExitCode}");
                    DebugConsole.WriteSeparator('=', 60);
                    return false;
                }

                DebugConsole.WriteWarning("✓ Process is running but not responding on http://localhost:3000 yet");
                DebugConsole.WriteWarning("  The server may still be initializing. Try refreshing the page in a few seconds.");
                DebugConsole.WriteSeparator('=', 60);
                return true;
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, "NodeServerManager.StartServerAsync");
                DebugConsole.WriteSeparator('=', 60);
                return false;
            }
        }

        /// <summary>
        /// Stop the Node.js server process
        /// </summary>
        public static void StopServer()
        {
            try
            {
                if (_serverProcess != null && !_serverProcess.HasExited)
                {
                    DebugConsole.WriteInfo("Stopping Node.js server...");
                    _serverProcess.Kill();
                    _serverProcess.WaitForExit(5000);
                    _serverProcess.Dispose();
                    DebugConsole.WriteSuccess("✓ Node.js server stopped");
                }
            }
            catch (Exception ex)
            {
                DebugConsole.WriteException(ex, "NodeServerManager.StopServer");
            }
        }
    }
}
