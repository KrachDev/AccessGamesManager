using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Text;

namespace AccessGamesManager.Misc
{
    /// <summary>
    /// Simple internal HTTP server to serve static files for the WebView
    /// </summary>
    public class LocalWebServer : IDisposable
    {
        private HttpListener? _httpListener;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _serverTask;
        private readonly string _assetsPath;
        public int Port { get; private set; }

        public LocalWebServer(string assetsPath)
        {
            _assetsPath = assetsPath;
            Port = FindAvailablePort();
            Console.WriteLine($"LocalWebServer initialized:");
            Console.WriteLine($"  Assets path: {_assetsPath}");
            Console.WriteLine($"  Assets exists: {Directory.Exists(_assetsPath)}");
            Console.WriteLine($"  Port reserved: {Port}");

            // Try to list files in Assets directory
            try
            {
                if (Directory.Exists(_assetsPath))
                {
                    var files = Directory.GetFiles(_assetsPath);
                    Console.WriteLine($"  Files found: {string.Join(", ", files.Select(Path.GetFileName))}");
                }
                else
                {
                    Console.WriteLine($"  WARNING: Assets directory does not exist!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error listing directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Find an available port on localhost
        /// </summary>
        private static int FindAvailablePort()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
                return ((IPEndPoint)socket.LocalEndPoint!).Port;
            }
        }

        /// <summary>
        /// Start the HTTP server
        /// </summary>
        public async Task StartAsync()
        {
            if (_httpListener != null)
            {
                Console.WriteLine("Server already running, skipping StartAsync");
                return;
            }

            try
            {
                Console.WriteLine($"Starting HTTP server on port {Port}...");
                _httpListener = new HttpListener();
                string prefix = $"http://127.0.0.1:{Port}/";
                Console.WriteLine($"Adding prefix: {prefix}");

                try
                {
                    _httpListener.Prefixes.Add(prefix);
                }
                catch (Exception prefixEx)
                {
                    Console.WriteLine($"Failed to add prefix: {prefixEx.Message}");
                    throw;
                }

                try
                {
                    Console.WriteLine("Calling HttpListener.Start()...");
                    _httpListener.Start();
                    Console.WriteLine("HttpListener.Start() succeeded");
                }
                catch (HttpListenerException listenerEx)
                {
                    Console.WriteLine($"HttpListenerException: {listenerEx.ErrorCode} - {listenerEx.Message}");
                    if (listenerEx.ErrorCode == 5)
                        Console.WriteLine("ERROR: Access denied. HttpListener may require Administrator privileges.");
                    throw;
                }

                _cancellationTokenSource = new CancellationTokenSource();
                _serverTask = ListenForRequests(_cancellationTokenSource.Token);

                Console.WriteLine($"Local web server started on http://127.0.0.1:{Port}");
                // Give the server a moment to initialize
                await Task.Delay(100);
                Console.WriteLine("Server initialization complete");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting web server: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Listen for incoming HTTP requests
        /// </summary>
        private async Task ListenForRequests(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    HttpListenerContext context = await _httpListener!.GetContextAsync();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    try
                    {
                        string requestPath = request.Url!.AbsolutePath;
                        Console.WriteLine($"HTTP Request: {requestPath}");

                        // Handle API endpoints
                        if (requestPath == "/api/store-data")
                        {
                            try
                            {
                                string storeJson = StoreManager.Serialize();
                                byte[] buffer = Encoding.UTF8.GetBytes(storeJson);
                                response.ContentType = "application/json; charset=utf-8";
                                response.ContentLength64 = buffer.Length;
                                response.OutputStream.Write(buffer, 0, buffer.Length);
                                response.StatusCode = 200;
                                Console.WriteLine($"  → 200 OK - API /store-data ({buffer.Length} bytes)");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"  ERROR getting store data: {ex.Message}");
                                response.StatusCode = 500;
                                string errorJson = "{\"error\":\"Failed to load store data\"}";
                                byte[] buffer = Encoding.UTF8.GetBytes(errorJson);
                                response.OutputStream.Write(buffer, 0, buffer.Length);
                            }
                        }
                        else
                        {
                            // Serve static files
                            string filePath = Path.Combine(_assetsPath, requestPath.TrimStart('/'));

                            // If no file specified, serve store.html
                            if (requestPath == "/" || requestPath.EndsWith("/"))
                            {
                                filePath = Path.Combine(_assetsPath, "store.html");
                            }

                            Console.WriteLine($"  Looking for file: {filePath}");
                            Console.WriteLine($"  Assets path: {_assetsPath}");
                            Console.WriteLine($"  File exists: {File.Exists(filePath)}");

                            if (File.Exists(filePath))
                            {
                                byte[] buffer = File.ReadAllBytes(filePath);
                                response.ContentType = GetContentType(filePath);
                                response.ContentLength64 = buffer.Length;
                                response.OutputStream.Write(buffer, 0, buffer.Length);
                                response.StatusCode = 200;
                                Console.WriteLine($"  → 200 OK ({buffer.Length} bytes)");
                            }
                            else
                            {
                                response.StatusCode = 404;
                                string notFoundHtml = $"<html><body><h1>404 Not Found</h1><p>File not found: {filePath}</p></body></html>";
                                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(notFoundHtml);
                                response.OutputStream.Write(buffer, 0, buffer.Length);
                                Console.WriteLine($"  → 404 Not Found - File doesn't exist");

                                // List files in Assets directory for debugging
                                try
                                {
                                    var files = Directory.GetFiles(_assetsPath);
                                    Console.WriteLine($"  Files in Assets directory: {string.Join(", ", files.Select(Path.GetFileName))}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"  Could not list Assets directory: {ex.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing request: {ex.Message}");
                        response.StatusCode = 500;
                    }
                    finally
                    {
                        response.OutputStream.Close();
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (!cancellationToken.IsCancellationRequested)
                        Console.WriteLine($"Server error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get the appropriate content type for a file
        /// </summary>
        private static string GetContentType(string filePath)
        {
            return Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".html" => "text/html; charset=utf-8",
                ".css" => "text/css; charset=utf-8",
                ".js" => "application/javascript; charset=utf-8",
                ".json" => "application/json; charset=utf-8",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".webp" => "image/webp",
                ".woff" => "font/woff",
                ".woff2" => "font/woff2",
                ".ttf" => "font/ttf",
                ".eot" => "application/vnd.ms-fontobject",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// Get the URL for accessing the server
        /// </summary>
        public string GetUrl(string? path = null)
        {
            string url = $"http://127.0.0.1:{Port}";
            if (!string.IsNullOrEmpty(path))
            {
                url += "/" + path.TrimStart('/');
            }
            return url;
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _serverTask?.Wait(1000);
            _cancellationTokenSource?.Dispose();
            _httpListener?.Close();
        }
    }
}
