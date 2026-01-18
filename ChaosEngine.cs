using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace ChaosLauncher
{
    /// <summary>
    /// CHAOS ENGINE - Core optimization and game launcher logic
    /// Implements GrokOptimizer functionality in C#
    /// </summary>
    public class ChaosEngine
    {
        // Power Plan GUIDs
        private const string UltimatePlan = "8c5e7fda-e8bf-45a6-a6cc-4b3c3f7e5efa";
        private const string BalancedPlan = "381b4222-f694-41f0-9685-ff5bb260df2e";
        private const string HighPerformancePlan = "381b4222-f694-41f0-9685-ff5bb260df2e";

        // Services to manage
        private List<string> servicesToStop = new List<string> 
        { 
            "DiagTrack", 
            "WSearch", 
            "dmwappushservice",
            "SysMain"
        };

        // Store original service states for restoration
        private Dictionary<string, ServiceStartMode> originalServiceStates = 
            new Dictionary<string, ServiceStartMode>();

        private Process currentGameProcess = null;

        // Event for status updates
        public event EventHandler<string> StatusUpdated;

        protected virtual void OnStatusUpdated(string message)
        {
            StatusUpdated?.Invoke(this, message);
        }

        /// <summary>
        /// Launch game with full optimization
        /// </summary>
        public async Task LaunchGame(string exePath, string gameName = "Game")
        {
            if (!File.Exists(exePath))
            {
                OnStatusUpdated($"❌ Game file not found: {exePath}");
                return;
            }

            try
            {
                OnStatusUpdated($"🎮 Launching {gameName}...");
                
                // Run boost before launching
                await RunBoost();
                OnStatusUpdated($"⚡ Boost activated!");

                // Start game process
                currentGameProcess = new Process();
                currentGameProcess.StartInfo.FileName = exePath;
                currentGameProcess.StartInfo.UseShellExecute = true;
                currentGameProcess.EnableRaisingEvents = true;

                // Subscribe to exit event for auto-restore
                currentGameProcess.Exited += (sender, e) =>
                {
                    OnStatusUpdated($"🎮 {gameName} closed. Restoring system...");
                    RestoreSystem();
                };

                currentGameProcess.Start();
                OnStatusUpdated($"✅ {gameName} started successfully!");
            }
            catch (Exception ex)
            {
                OnStatusUpdated($"❌ Error launching game: {ex.Message}");
                Debug.WriteLine($"LaunchGame Error: {ex}");
            }
        }

        /// <summary>
        /// Set Windows power plan
        /// </summary>
        private void SetPowerPlan(string guid)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("powercfg")
                {
                    Arguments = $"/setactive {guid}",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using (Process p = Process.Start(psi))
                {
                    p.WaitForExit();
                }

                OnStatusUpdated($"⚡ Power plan set to {guid}");
            }
            catch (Exception ex)
            {
                OnStatusUpdated($"⚠️ Power plan error: {ex.Message}");
                Debug.WriteLine($"SetPowerPlan Error: {ex}");
            }
        }

        /// <summary>
        /// Get current active power plan
        /// </summary>
        public string GetActivePowerPlan()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("powercfg")
                {
                    Arguments = "/getactivescheme",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                using (Process p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    return output.Contains("Ultimate") ? "Ultimate Performance" : "Balanced";
                }
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Manage services (stop/start)
        /// </summary>
        private void ManageServices(bool stop)
        {
            foreach (var serviceName in servicesToStop)
            {
                try
                {
                    using (ServiceController sc = new ServiceController(serviceName))
                    {
                        if (stop)
                        {
                            if (sc.Status == ServiceControllerStatus.Running)
                            {
                                // Store original state
                                originalServiceStates[serviceName] = sc.StartType;
                                
                                sc.Stop();
                                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
                                OnStatusUpdated($"⏹️ Stopped service: {serviceName}");
                            }
                        }
                        else
                        {
                            // Restore only if it was running before
                            if (originalServiceStates.ContainsKey(serviceName))
                            {
                                sc.Start();
                                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5));
                                OnStatusUpdated($"▶️ Started service: {serviceName}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Service management error for {serviceName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Apply registry tweaks for performance
        /// </summary>
        private void ApplyRegistryTweaks(bool apply)
        {
            try
            {
                // System Responsiveness - Reduce input lag
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", true))
                {
                    if (key != null)
                    {
                        if (apply)
                        {
                            key.SetValue("SystemResponsiveness", 0, RegistryValueKind.DWord);
                            key.SetValue("NetworkThrottlingIndex", 0xFFFFFFFF, RegistryValueKind.DWord);
                            OnStatusUpdated("📝 Registry tweaks applied");
                        }
                        else
                        {
                            key.SetValue("SystemResponsiveness", 10, RegistryValueKind.DWord);
                            key.SetValue("NetworkThrottlingIndex", 10, RegistryValueKind.DWord);
                            OnStatusUpdated("📝 Registry tweaks reverted");
                        }
                    }
                }

                // Disable Game DVR
                if (apply)
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                        @"System\GameConfigStore", true))
                    {
                        if (key != null)
                        {
                            key.SetValue("GameDVR_Enabled", 0, RegistryValueKind.DWord);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnStatusUpdated($"⚠️ Registry error: {ex.Message}");
                Debug.WriteLine($"ApplyRegistryTweaks Error: {ex}");
            }
        }

        /// <summary>
        /// Clean temporary files and prefetch
        /// </summary>
        public void CleanSystem()
        {
            try
            {
                string[] paths = new string[]
                {
                    Path.GetTempPath(),
                    @"C:\Windows\Temp",
                    @"C:\Windows\Prefetch"
                };

                int filesDeleted = 0;

                foreach (var path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        try
                        {
                            DirectoryInfo di = new DirectoryInfo(path);
                            foreach (FileInfo file in di.GetFiles())
                            {
                                try
                                {
                                    file.Delete();
                                    filesDeleted++;
                                }
                                catch { /* File in use, skip */ }
                            }
                        }
                        catch { /* Directory access error, skip */ }
                    }
                }

                OnStatusUpdated($"🧹 Cleaned {filesDeleted} temporary files");
            }
            catch (Exception ex)
            {
                OnStatusUpdated($"⚠️ Cleanup error: {ex.Message}");
                Debug.WriteLine($"CleanSystem Error: {ex}");
            }
        }

        private PerformanceCounter cpuCounter = null;

        /// <summary>
        /// Get real-time CPU usage
        /// </summary>
        public float GetCpuUsage()
        {
            try
            {
                if (cpuCounter == null)
                {
                    cpuCounter = new PerformanceCounter(
                        "Processor", "% Processor Time", "_Total", true);
                    cpuCounter.NextValue(); // First call is always 0
                    System.Threading.Thread.Sleep(100);
                }
                return cpuCounter.NextValue();
            }
            catch
            {
                return 0f;
            }
        }

        /// <summary>
        /// Get real-time RAM usage
        /// </summary>
        public (float used, float total) GetRamUsage()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");

                foreach (ManagementObject obj in searcher.Get())
                {
                    double total = Convert.ToDouble(obj["TotalVisibleMemorySize"]) / 1024 / 1024;
                    double free = Convert.ToDouble(obj["FreePhysicalMemory"]) / 1024 / 1024;
                    double used = total - free;

                    return ((float)used, (float)total);
                }
            }
            catch { }

            return (0f, 0f);
        }

        /// <summary>
        /// Get GPU usage (NVIDIA/AMD)
        /// </summary>
        public float GetGpuUsage()
        {
            try
            {
                PerformanceCounter gpuCounter = new PerformanceCounter(
                    "GPU Engine", "Utilization %", "_Total", true);
                gpuCounter.NextValue();
                System.Threading.Thread.Sleep(100);
                return gpuCounter.NextValue();
            }
            catch
            {
                return 0f;
            }
        }

        /// <summary>
        /// Run full boost sequence
        /// </summary>
        private async Task RunBoost()
        {
            try
            {
                OnStatusUpdated("🚀 Starting boost sequence...");

                // Clean system first
                CleanSystem();

                // Set power plan
                SetPowerPlan(UltimatePlan);
                await Task.Delay(500);

                // Stop services
                ManageServices(true);
                await Task.Delay(500);

                // Apply registry tweaks
                ApplyRegistryTweaks(true);

                OnStatusUpdated("✅ Boost sequence complete!");
            }
            catch (Exception ex)
            {
                OnStatusUpdated($"❌ Boost error: {ex.Message}");
                Debug.WriteLine($"RunBoost Error: {ex}");
            }
        }

        /// <summary>
        /// Restore system to default state
        /// </summary>
        public void RestoreSystem()
        {
            try
            {
                OnStatusUpdated("🔄 Restoring system...");

                // Restore power plan
                SetPowerPlan(BalancedPlan);

                // Restart services
                ManageServices(false);

                // Revert registry tweaks
                ApplyRegistryTweaks(false);

                OnStatusUpdated("✅ System restored!");
            }
            catch (Exception ex)
            {
                OnStatusUpdated($"❌ Restore error: {ex.Message}");
                Debug.WriteLine($"RestoreSystem Error: {ex}");
            }
        }

        /// <summary>
        /// Apply specific optimization mode
        /// </summary>
        public async Task ApplyMode(string mode)
        {
            try
            {
                OnStatusUpdated($"⚡ Applying {mode} mode...");

                switch (mode.ToLower())
                {
                    case "competitive":
                        SetPowerPlan(UltimatePlan);
                        ManageServices(true);
                        ApplyRegistryTweaks(true);
                        break;

                    case "graphics":
                        SetPowerPlan(HighPerformancePlan);
                        ApplyRegistryTweaks(false);
                        break;

                    case "balanced":
                        SetPowerPlan(BalancedPlan);
                        ManageServices(false);
                        ApplyRegistryTweaks(false);
                        break;

                    case "network":
                        // Network optimization
                        ProcessStartInfo psi = new ProcessStartInfo("netsh")
                        {
                            Arguments = "int tcp set global autotuninglevel=normal",
                            WindowStyle = ProcessWindowStyle.Hidden,
                            CreateNoWindow = true
                        };
                        Process.Start(psi).WaitForExit();
                        break;

                    case "color":
                        // Color boost
                        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                            @"Software\Microsoft\Windows\CurrentVersion\VideoSettings", true))
                        {
                            if (key != null)
                            {
                                key.SetValue("DisableHWGamma", 1, RegistryValueKind.DWord);
                            }
                        }
                        break;
                }

                OnStatusUpdated($"✅ {mode} mode applied!");
            }
            catch (Exception ex)
            {
                OnStatusUpdated($"❌ Mode error: {ex.Message}");
                Debug.WriteLine($"ApplyMode Error: {ex}");
            }
        }
    }
}
