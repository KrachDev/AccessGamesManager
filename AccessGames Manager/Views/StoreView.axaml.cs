using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaWebView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccessGamesManager.Misc;

namespace AccessGames_Manager.Views
{
    public partial class StoreView : UserControl
    {
        public StoreView()
        {
            InitializeComponent();
            Console.WriteLine("StoreView initialized");
        }

        public async Task LoadStoreAsync()
        {
            await StoreManager.LoadAsync();

            if (this.FindControl<WebView>("StoreWebView") is not WebView webView)
            {
                Console.WriteLine("ERROR: StoreWebView not found!");
                return;
            }

            try
            {
                // Check if Node.js is available
                if (!NodeDownloader.IsNodeReady())
                {
                    Console.WriteLine("Node.js not found. Showing download dialog...");

                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel is not Window parentWindow)
                    {
                        Console.WriteLine("✗ Cannot show download dialog: no parent window");
                        return;
                    }

                    var downloadDialog = new AccessGames_Manager.Views.NodeDownloadDialog();
                    // Show the dialog (non-blocking) then kick off the download inside it
                    var downloadTask = downloadDialog.StartDownloadAsync();
                    await downloadDialog.ShowDialog(parentWindow);
                    bool success = await downloadTask;
                    if (!success)
                    {
                        Console.WriteLine("✗ Node.js download cancelled or failed");
                        return;
                    }
                }

                Console.WriteLine("Ensuring Node.js backend is running...");
                bool serverStarted = await NodeServerManager.StartServerAsync();

                if (!serverStarted)
                {
                    Console.WriteLine("✗ Could not start Node.js server.");
                    return;
                }

                // Load from the Node.js backend server
                string backendUrl = "http://localhost:3000/catalogue.html";
                Console.WriteLine($"Loading store from: {backendUrl}");
                webView.Url = new Uri(backendUrl);
                Console.WriteLine("✓ Store page loaded from Node.js backend");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading store: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

       

        private async void RefreshBtn_Click(object? sender, RoutedEventArgs e)
        {
            await LoadStoreAsync();
        }

        private async void AddStoreOfferBtn_Click(object? sender, RoutedEventArgs e)
        {
            var dlg = new AccessGamesManager.Misc.AdminPanelDialog();
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null && topLevel is Window window)
            {
                await dlg.ShowDialog(window);
                await LoadStoreAsync();
            }
        }

        private async void StoreAdminBtn_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null && topLevel is Window window)
            {
                await AccessGamesManager.Misc.AdminPanelDialog.TryOpenAsync(window);
                await LoadStoreAsync();
            }
        }
        
        // JS Interop Target
        private class InteropTarget
        {
            public void postMessage(string message)
            {
                Console.WriteLine("WebView message: " + message);
                if (message != null && message.StartsWith("offer:"))
                {
                    string id = message.Substring("offer:".Length);
                    var offer = StoreManager.Offers.FirstOrDefault(o => o.Id == id);
                    if (offer != null && !string.IsNullOrEmpty(offer.StoreUrl))
                    {
                        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = offer.StoreUrl, UseShellExecute = true }); }
                        catch { }
                    }
                }
            }
        }
    }
}
