using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccessGamesManager.Misc
{
    public static class LauncherNetworkManager
    {
        private static void RunNetsh(string args)
        {
            try
            {
                var psi = new ProcessStartInfo("netsh", args)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                var p = Process.Start(psi);
                p?.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Netsh error: {ex.Message}");
            }
        }

        public static bool IsRuleActive(string ruleName)
        {
            try
            {
                var psi = new ProcessStartInfo("netsh", $"advfirewall firewall show rule name=\"{ruleName}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };
                var p = Process.Start(psi);
                if (p == null) return false;
                string out_ = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                return out_.Contains(ruleName);
            }
            catch
            {
                return false;
            }
        }

        public static void BlockExecutables(string ruleName, params string[] exePaths)
        {
            // First delete existing rule
            RunNetsh($"advfirewall firewall delete rule name=\"{ruleName}\"");

            foreach (var exePath in exePaths)
            {
                if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                {
                    RunNetsh($"advfirewall firewall add rule name=\"{ruleName}\" dir=out action=block program=\"{exePath}\" enable=yes");
                }
            }
        }

        public static void UnblockRule(string ruleName)
        {
            RunNetsh($"advfirewall firewall delete rule name=\"{ruleName}\"");
        }

        // Specific Launchers
        public const string RuleUbisoft = "AGM_Block_Ubisoft";
        public const string RuleEpic = "AGM_Block_Epic";
        public const string RuleEA = "AGM_Block_EA";

        public static bool IsUbisoftBlocked() => IsRuleActive(RuleUbisoft);
        public static bool IsEpicBlocked() => IsRuleActive(RuleEpic);
        public static bool IsEABlocked() => IsRuleActive(RuleEA);

        public static void BlockUbisoft(string folderPath)
        {
            string webcore = Path.Combine(folderPath, "uplaywebcore.exe");
            // Delete any existing rule first
            RunNetsh($"advfirewall firewall delete rule name=\"{RuleUbisoft}\"");
            if (!string.IsNullOrEmpty(webcore) && File.Exists(webcore))
            {
                RunNetsh($"advfirewall firewall add rule name=\"{RuleUbisoft}\" dir=out action=block program=\"{webcore}\" enable=yes");
                RunNetsh($"advfirewall firewall add rule name=\"{RuleUbisoft}\" dir=in  action=block program=\"{webcore}\" enable=yes");
            }
        }

        public static void BlockEpic(string folderPath)
        {
            string exe = Path.Combine(folderPath, "EpicGamesLauncher.exe");
            BlockExecutables(RuleEpic, exe);
        }

        public static void BlockEA(string folderPath)
        {
            string exe = Path.Combine(folderPath, "EADesktop.exe");
            BlockExecutables(RuleEA, exe);
        }

        public static bool CheckUbisoftFound(string folderPath)
        {
            return File.Exists(Path.Combine(folderPath, "uplaywebcore.exe"));
        }

        public static bool CheckEpicFound(string folderPath)
        {
            return File.Exists(Path.Combine(folderPath, "EpicGamesLauncher.exe"));
        }

        public static bool CheckEAFound(string folderPath)
        {
            return File.Exists(Path.Combine(folderPath, "EADesktop.exe"));
        }

        public static List<string> GetEnabledNetworkAdapters()
        {
            var list = new List<string>();
            try
            {
                var psi = new ProcessStartInfo("powershell", "-NoProfile -ExecutionPolicy Bypass -Command \"Get-NetAdapter | Where-Object { $_.Status -eq 'Up' } | Select-Object -ExpandProperty Name\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using (var p = Process.Start(psi))
                {
                    if (p != null)
                    {
                        string output = p.StandardOutput.ReadToEnd();
                        p.WaitForExit();
                        foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            string name = line.Trim();
                            if (!string.IsNullOrEmpty(name))
                            {
                                list.Add(name);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting network adapters: {ex.Message}");
            }
            return list;
        }

        public static void DisableNetworkAdapters(List<string> adapterNames)
        {
            if (adapterNames == null || adapterNames.Count == 0) return;
            
            var escapedNames = new List<string>();
            foreach (var name in adapterNames)
            {
                escapedNames.Add($"'{name.Replace("'", "''")}'");
            }
            string namesJoined = string.Join(",", escapedNames);
            string command = $"Get-NetAdapter | Where-Object {{ $_.Name -in @({namesJoined}) }} | Disable-NetAdapter -Confirm:$false";
            
            try
            {
                var psi = new ProcessStartInfo("powershell", $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using (var p = Process.Start(psi))
                {
                    p?.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disabling network adapters: {ex.Message}");
            }
        }

        public static void EnableNetworkAdapters(List<string> adapterNames)
        {
            if (adapterNames == null || adapterNames.Count == 0) return;
            
            var escapedNames = new List<string>();
            foreach (var name in adapterNames)
            {
                escapedNames.Add($"'{name.Replace("'", "''")}'");
            }
            string namesJoined = string.Join(",", escapedNames);
            string command = $"Get-NetAdapter | Where-Object {{ $_.Name -in @({namesJoined}) }} | Enable-NetAdapter -Confirm:$false";
            
            try
            {
                var psi = new ProcessStartInfo("powershell", $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using (var p = Process.Start(psi))
                {
                    p?.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enabling network adapters: {ex.Message}");
            }
        }

        public static async Task<List<string>> GetOnRadiosAsync()
        {
            var onRadios = new List<string>();
            try
            {
                var access = await Windows.Devices.Radios.Radio.RequestAccessAsync();
                if (access == Windows.Devices.Radios.RadioAccessStatus.Allowed)
                {
                    var radios = await Windows.Devices.Radios.Radio.GetRadiosAsync();
                    foreach (var radio in radios)
                    {
                        if (radio.State == Windows.Devices.Radios.RadioState.On)
                        {
                            onRadios.Add(radio.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetOnRadiosAsync error: {ex.Message}");
            }
            return onRadios;
        }

        public static async Task SetRadiosStateAsync(List<string> radioNames, bool turnOn)
        {
            try
            {
                var access = await Windows.Devices.Radios.Radio.RequestAccessAsync();
                if (access == Windows.Devices.Radios.RadioAccessStatus.Allowed)
                {
                    var radios = await Windows.Devices.Radios.Radio.GetRadiosAsync();
                    foreach (var radio in radios)
                    {
                        if (radioNames.Contains(radio.Name))
                        {
                            await radio.SetStateAsync(turnOn ? Windows.Devices.Radios.RadioState.On : Windows.Devices.Radios.RadioState.Off);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetRadiosStateAsync error: {ex.Message}");
            }
        }

        public static async Task TurnOffAllRadiosAsync()
        {
            try
            {
                var access = await Windows.Devices.Radios.Radio.RequestAccessAsync();
                if (access == Windows.Devices.Radios.RadioAccessStatus.Allowed)
                {
                    var radios = await Windows.Devices.Radios.Radio.GetRadiosAsync();
                    foreach (var radio in radios)
                    {
                        await radio.SetStateAsync(Windows.Devices.Radios.RadioState.Off);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TurnOffAllRadiosAsync error: {ex.Message}");
            }
        }
    }
}
