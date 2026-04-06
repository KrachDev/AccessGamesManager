using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using HandyControl.Controls;
using SteamKit2;
using SteamKit2.Authentication;

namespace AccessGamesManager.Misc
{
    /// <summary>
    /// Uses SteamKit2 to silently refresh a Steam account's auth token so the
    /// Steam client can stay in offline mode as long as possible without hitting
    /// the infinite-loading bug.
    ///
    /// Flow:
    ///   1. Try stored JWT refresh token  — no password, no Steam Guard needed.
    ///   2. If missing / expired          — fall back to username + password.
    ///   3. If Steam Guard required       — call <see cref="steamGuardCallback"/>
    ///      to show a dialog and get the code from the user.
    ///   4. On success                   — save the new refresh token and return true.
    /// </summary>
    public static class SteamAuthRefresher
    {
        /// <param name="username">Steam account name.</param>
        /// <param name="password">Plain-text password, or null if we only have a refresh token.</param>
        /// <param name="storedRefreshToken">Previously saved JWT, or null for first-time login.</param>
        /// <param name="steamGuardCallback">
        ///   Called with (prompt, hint) when a Steam Guard code is needed.
        ///   Must run the dialog on the UI thread and return the entered code, or null to abort.
        /// </param>
        /// <param name="onStatus">Optional progress callback for status bar updates.</param>
        /// <returns>True if auth was refreshed and a new token was saved.</returns>
        public static async Task<bool> RefreshAsync(
            string username,
            string? password,
            string? storedRefreshToken,
            Func<string, string, Task<string?>> steamGuardCallback,
            Action<string>? onStatus = null)
        {
            var client  = new SteamClient();
            var manager = new CallbackManager(client);
            var user    = client.GetHandler<SteamUser>()!;

            var connectedTcs    = new TaskCompletionSource<bool>();
            var loggedOnTcs     = new TaskCompletionSource<EResult>();

            manager.Subscribe<SteamClient.ConnectedCallback>(_ => connectedTcs.TrySetResult(true));
            manager.Subscribe<SteamClient.DisconnectedCallback>(_ =>
            {
                connectedTcs.TrySetResult(false);
                loggedOnTcs.TrySetResult(EResult.NoConnection);
            });
            manager.Subscribe<SteamUser.LoggedOnCallback>(cb => loggedOnTcs.TrySetResult(cb.Result));

            // Pump callbacks on a background thread
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var pump = Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                    manager.RunWaitAllCallbacks(TimeSpan.FromMilliseconds(200));
            }, cts.Token);

            try
            {
                onStatus?.Invoke("Connecting to Steam…");
                client.Connect();

                bool connected = await connectedTcs.Task.WaitAsync(TimeSpan.FromSeconds(15));
                if (!connected)
                {
                    onStatus?.Invoke("Could not connect to Steam.");
                    return false;
                }

                // ── Path A: use stored refresh token (fast, no password) ──────────
                if (!string.IsNullOrEmpty(storedRefreshToken))
                {
                    onStatus?.Invoke($"Refreshing auth for {username}…");
                    user.LogOn(new SteamUser.LogOnDetails
                    {
                        Username            = username,
                        AccessToken         = storedRefreshToken,   // JWT refresh token
                        ShouldRememberPassword = true,
                    });

                    var result = await loggedOnTcs.Task.WaitAsync(TimeSpan.FromSeconds(15));

                    if (result == EResult.OK)
                    {
                        // Steam issues an updated refresh token via LoggedOnCallback.WebAPIUserNonce
                        // The same token is still valid — save it again to reset its TTL.
                        AccountConfigManager.SaveRefreshToken(username, storedRefreshToken);
                        onStatus?.Invoke($"Auth refreshed for {username} ✔");
                        return true;
                    }

                    // Token expired or revoked — fall through to password login
                    AccountConfigManager.ClearRefreshToken(username);
                    onStatus?.Invoke("Stored token expired, trying password…");
                    loggedOnTcs = new TaskCompletionSource<EResult>();
                }

                // ── Path B: full credentials login ───────────────────────────────
                if (string.IsNullOrEmpty(password))
                {
                    onStatus?.Invoke($"No password stored for {username}, skipping auth refresh.");
                    return false;
                }

                onStatus?.Invoke($"Logging in as {username}…");

                string? newRefreshToken = null;

                try
                {
                    var authSession = await client.Authentication.BeginAuthSessionViaCredentialsAsync(
                        new AuthSessionDetails
                        {
                            Username           = username,
                            Password           = password,
                            IsPersistentSession = true,
                            Authenticator      = new CallbackAuthenticator(steamGuardCallback),
                        });

                    var poll = await authSession.PollingWaitForResultAsync();
                    newRefreshToken = poll.RefreshToken;

                    // Now do the actual LogOn with the fresh token so SteamKit fires callbacks
                    user.LogOn(new SteamUser.LogOnDetails
                    {
                        Username               = username,
                        AccessToken            = poll.RefreshToken,
                        ShouldRememberPassword = true,
                    });

                    var result = await loggedOnTcs.Task.WaitAsync(TimeSpan.FromSeconds(15));
                    if (result != EResult.OK)
                    {
                        onStatus?.Invoke($"Login result: {result}");
                        return false;
                    }
                }
                catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
                {
                    onStatus?.Invoke("Steam Guard entry cancelled.");
                    return false;
                }

                if (!string.IsNullOrEmpty(newRefreshToken))
                {
                    AccountConfigManager.SaveRefreshToken(username, newRefreshToken);
                    onStatus?.Invoke($"Auth token saved for {username} ✔");
                }

                return true;
            }
            catch (Exception ex)
            {
                onStatus?.Invoke($"Auth refresh error: {ex.Message}");
                Growl.WarningGlobal($"Auth refresh failed for {username}: {ex.Message}");
                return false;
            }
            finally
            {
                cts.Cancel();
                try { user.LogOff(); } catch { }
                try { client.Disconnect(); } catch { }
                try { await pump; } catch { }
            }
        }

        // ── IAuthenticator adapter ────────────────────────────────────────────────

        private sealed class CallbackAuthenticator : IAuthenticator
        {
            private readonly Func<string, string, Task<string?>> _callback;
            public CallbackAuthenticator(Func<string, string, Task<string?>> cb) => _callback = cb;

            public async Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect)
            {
                string prompt = previousCodeWasIncorrect
                    ? "The previous code was incorrect. Enter your Steam Guard app code:"
                    : "Enter your Steam Guard authenticator code:";
                string? code = await _callback(prompt, "Open your Steam app → Steam Guard → enter the code.");
                if (string.IsNullOrEmpty(code)) throw new OperationCanceledException("User cancelled.");
                return code;
            }

            public async Task<string> GetEmailCodeAsync(string email, bool previousCodeWasIncorrect)
            {
                string prompt = previousCodeWasIncorrect
                    ? "The previous code was incorrect. Enter the new code sent to your email:"
                    : "Enter the Steam Guard code sent to your email:";
                string? code = await _callback(prompt, $"Check your inbox: {email}");
                if (string.IsNullOrEmpty(code)) throw new OperationCanceledException("User cancelled.");
                return code;
            }

            public Task<bool> AcceptDeviceConfirmationAsync()
                => Task.FromResult(false); // we don't support mobile confirmations silently
        }
    }
}
