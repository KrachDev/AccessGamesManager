using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Avalonia.Controls.Notifications;

namespace HandyControl.Controls
{
    public static class MessageBox
    {
        public static void Show(string messageBoxText, string caption = "Information", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard(caption, messageBoxText, (ButtonEnum)button, (Icon)icon);
                box.ShowAsync();
            });
        }
        
        public static void Error(string messageBoxText) => Show(messageBoxText, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        public static void Warning(string messageBoxText) => Show(messageBoxText, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    
    public enum MessageBoxButton { OK, OKCancel, YesNoCancel, YesNo }
    public enum MessageBoxImage { None, Error, Hand, Stop, Question, Exclamation, Warning, Asterisk, Information }

    public static class Growl
    {
        public static WindowNotificationManager? NotificationManager { get; set; }

        private static void ShowNativeToast(string title, string msg, NotificationType type)
        {
            System.Diagnostics.Debug.WriteLine($"[{type}] {title}: {msg}");
            
            Dispatcher.UIThread.Post(() =>
            {
                NotificationManager?.Show(new Notification(title, msg, type));
            });
        }

        public static void Info(string msg) => ShowNativeToast("Info", msg, NotificationType.Information);
        public static void Success(string msg) => ShowNativeToast("Success", msg, NotificationType.Success);
        public static void Warning(string msg) => ShowNativeToast("Warning", msg, NotificationType.Warning);
        public static void Error(string msg) => ShowNativeToast("Error", msg, NotificationType.Error);

        public static void InfoGlobal(string msg) => Info(msg);
        public static void SuccessGlobal(string msg) => Success(msg);
        public static void WarningGlobal(string msg) => Warning(msg);
        public static void ErrorGlobal(string msg) => Error(msg);
        public static void ClearGlobal() {}
    }
}

namespace System.Windows
{
    public static class Clipboard
    {
        public static void SetText(string text)
        {
            try
            {
                if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow?.Clipboard != null)
                {
                    _ = desktop.MainWindow.Clipboard.SetTextAsync(text);
                }
            } catch {}
        }
    }
}
