using System.Text;
using MaybeSpoofed.Classes;
using MaybeSpoofed.Functions;
using Newtonsoft.Json;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Diagnostics;
using MaybeSpoofed.Helpers;

namespace MaybeSpoofed
{
    internal class Program
    {
        static void Main()
        {
            if (!Directory.Exists("config"))
                Directory.CreateDirectory("config");

            if (!Debugger.IsAttached)
            {
                if (!IsAdministrator())
                {
                    Custom.WriteLine("Warning: This application requires administrator privileges.", ConsoleColor.Yellow);
                    Custom.WriteLine("Please restart it as an administrator.", ConsoleColor.Yellow);

                    Console.ReadLine();
                    Console.ReadLine();

                    return;
                }
            }

            Console.Title = "MaybeSpoofed || Discord = @enter_my_username";

            Console.WriteLine("Thank you for using MaybeSpoofed, our goal is to prevent spoofer issues\nWe are trying our best to stay updated\nDiscord: enter_my_username");
            Console.WriteLine("Our goal is only to support EAC Rust, but this might work for other games");
            Console.WriteLine("Remember that some games also uses traces to catch you ban evading");

            Custom.WriteLine("This can be ignored", ConsoleColor.Green);
            Custom.WriteLine("This means we are running a task on your PC", ConsoleColor.Cyan);
            Custom.WriteLine("This is very bad and you should reach out to us", ConsoleColor.DarkRed);
            Custom.WriteLine("This means goofy forgot to disable debug prints", ConsoleColor.DarkMagenta);
            Custom.WriteLine("This means goofy forgot to disable debug prints", ConsoleColor.Magenta);
            Custom.WriteLine("This is just useful hints", ConsoleColor.Yellow);
            Custom.WriteLine("This means your serials are not spoofed and will lead to bans", ConsoleColor.Red);
            Custom.WriteLine("This means that its not spoofed but not verified to cause bans", ConsoleColor.DarkYellow);
            Console.WriteLine();

            Custom.WriteLine("Starting program", ConsoleColor.Cyan);
            Components HardwareID = new();
            Components SpoofedHardwareID = null!;

            Components CheckForPermSpoof = Hardware.GetHardwareID();

            if (File.Exists("config/hardware.json"))
            {
                Custom.WriteLine("Loading existing hardware file", ConsoleColor.Green);

                var fileContent = File.ReadAllText("config/hardware.json", Encoding.UTF8);

                var tempSettings = JsonConvert.DeserializeObject<Components>(File.ReadAllText("config/hardware.json", Encoding.UTF8));

                if(tempSettings == null)
                {
                    Custom.WriteLine("Something went wrong, please delete config folder", ConsoleColor.DarkRed);
                    Console.ReadLine();
                    return;
                }

                if (tempSettings.ProgramVersion != "v0.6")
                {
                    Custom.WriteLine("Outdated program json, please delete config folder and restart application", ConsoleColor.Red);
                    Console.ReadLine();
                    return;
                }

                HardwareID = tempSettings;

                SpoofedHardwareID = CheckForPermSpoof;

                File.WriteAllText("config/spoofed.json", JsonConvert.SerializeObject(SpoofedHardwareID, Formatting.Indented), new UTF8Encoding(true));
            }
            else
            {
                Custom.WriteLine("Generating new hardware file", ConsoleColor.Cyan);

                HardwareID = Hardware.GetHardwareID();

                File.WriteAllText("config/hardware.json", JsonConvert.SerializeObject(HardwareID, Formatting.Indented), new UTF8Encoding(true));
            }

            Custom.WriteLine("---------------------------------------");
            Custom.WriteLine("Perm spoofer validation:");
            Custom.WriteLine("---------------------------------------");

            Validation.Validation.Start(CheckForPermSpoof);

            if (SpoofedHardwareID == null)
            {
                Custom.WriteLine("Please spoof and restart program to see if you are spoofed", ConsoleColor.Yellow);
            }
            else
            {
                Custom.WriteLine("---------------------------------------",ConsoleColor.Yellow);
                Custom.WriteLine("Hardware result:",ConsoleColor.Yellow);
                Custom.WriteLine("---------------------------------------",ConsoleColor.Yellow);

                try
                {
                    if (SpoofedHardwareID.TPM != null)
                    {
                        Custom.WriteLine("Trusted platform module(TPM) is enabled, please disable it in BIOS", ConsoleColor.Red);
                    }
                }
                catch(Exception ex) { Custom.WriteLine($"{ex.Message}", ConsoleColor.Red); }

                try
                {
                    if (SpoofedHardwareID.BluetoothDevices != null)
                    {
                        if (SpoofedHardwareID.BluetoothDevices.Count > 0)
                        {
                            Custom.WriteLine("Bluetooth card is present, please disable it in BIOS", ConsoleColor.Red);
                        }
                    }
                }
                catch (Exception ex) { Custom.WriteLine($"{ex.Message}", ConsoleColor.Red); }

                try
                {
                    if (SpoofedHardwareID.WindowsFastStartup)
                    {
                        Custom.WriteLine("Windows fast startup is enabled, can lead to bans if using `Shutdown pc`", ConsoleColor.Yellow);
                    }
                }
                catch (Exception ex) { Custom.WriteLine($"{ex.Message}", ConsoleColor.Red); }

                try
                {
                    if (HardwareID.MotherboardInformation != null)
                    {
                        if (HardwareID.MotherboardInformation.SerialNumber == SpoofedHardwareID.MotherboardInformation.SerialNumber)
                        {
                            Custom.WriteLine($"[Baseboard] {SpoofedHardwareID.MotherboardInformation.Product} => [{SpoofedHardwareID.MotherboardInformation.SerialNumber}]", ConsoleColor.Red);
                        }
                    }
                    else
                        Custom.WriteLine($"Error grabbing MotherboardInformation", ConsoleColor.DarkRed);
                }
                catch (Exception ex) { Custom.WriteLine($"{ex.Message}", ConsoleColor.Red); }

                try
                {
                    if (HardwareID.SystemInformation != null)
                    {
                        if (HardwareID.SystemInformation.UUID == SpoofedHardwareID.SystemInformation.UUID)
                        {
                            Custom.WriteLine($"[System] Uuid => {SpoofedHardwareID.SystemInformation.UUID}", ConsoleColor.Red);
                        }
                    }
                    else
                        Custom.WriteLine($"Error grabbing SystemInformation", ConsoleColor.DarkRed);
                }
                catch (Exception ex) { Custom.WriteLine($"{ex.Message}", ConsoleColor.Red); }

                try
                {
                    if (HardwareID.Ram != null)
                    {
                        foreach (var ram in HardwareID.Ram)
                        {
                            var serial = ram.SerialNumber;

                            if (serial == "00000000")
                                continue;

                            if (SpoofedHardwareID.Ram.FindAll(m => m.SerialNumber == serial).Count > 0)
                            {
                                Custom.WriteLine($"[Ram] {ram.Location} => {serial}", ConsoleColor.Red);
                            }
                        }


                    }
                    else
                        Custom.WriteLine($"Error grabbing Ram", ConsoleColor.DarkRed);
                }
                catch (Exception ex) { Custom.WriteLine($"{ex.Message}", ConsoleColor.Red); }

                try
                {
                    if (HardwareID.DiskDrives != null)
                    {
                        foreach (var disk in HardwareID.DiskDrives)
                        {
                            var serial = disk.SerialNumber;

                            if (SpoofedHardwareID.DiskDrives.FindAll(m => m.SerialNumber == serial).Count > 0)
                            {
                                Custom.WriteLine($"[Disk] {disk.Model} => {serial}", ConsoleColor.Red);
                            }
                        }
                    }
                    else
                        Custom.WriteLine($"Error grabbing DiskDrives", ConsoleColor.DarkRed);
                }
                catch (Exception ex) { Custom.WriteLine($"{ex.Message}", ConsoleColor.Red); }

                try
                {
                    if (HardwareID.GPUs != null)
                    {
                        foreach (var gpu in HardwareID.GPUs)
                        {
                            var serial = gpu.SerialNumber;
                            var UUID = gpu.UUID;

                            if (string.IsNullOrWhiteSpace(UUID))
                            {
                                Custom.WriteLine($"We dont support your GPU, trust your spoofer provider or check manually", ConsoleColor.DarkRed);
                            }
                            else
                            {
                                if (SpoofedHardwareID.GPUs.FindAll(m => m.UUID == UUID).Count > 0)
                                {
                                    Custom.WriteLine($"[GPU] {gpu.Name}(UUID) => {UUID}", ConsoleColor.Red);
                                }
                            }
                        }
                    }
                    else
                        Custom.WriteLine($"Error grabbing GPUs", ConsoleColor.DarkRed);
                }
                catch (Exception ex) { Custom.WriteLine($"{ex.Message}", ConsoleColor.Red); }

                try
                {
                    if (HardwareID.NetworkAdapters != null)
                    {
                        foreach (var network in HardwareID.NetworkAdapters)
                        {
                            var mac = network.Mac;

                            if (network.Name.StartsWith("WAN Miniport"))
                                continue;

                            if (network.Name.Contains("vpn", StringComparison.CurrentCultureIgnoreCase))
                                continue;

                            if (SpoofedHardwareID.NetworkAdapters.FindAll(m => m.Mac == mac).Count > 0)
                            {
                                Custom.WriteLine($"[Network] {network.Name}(Mac) => {mac}", ConsoleColor.Red);
                            }

                            if (SpoofedHardwareID.NetworkAdapters.FindAll(m => m.Guid == network.Guid).Count > 0)
                            {
                                Custom.WriteLine($"[Network] {network.Name}(Guid) => {network.Guid}", ConsoleColor.DarkYellow);
                            }
                        }
                    }
                    else
                        Custom.WriteLine($"Error grabbing NetworkAdapters", ConsoleColor.DarkRed);
                }
                catch (Exception ex) { Custom.WriteLine($"{ex.Message}", ConsoleColor.Red); }

                try
                {
                    if (HardwareID.Monitors != null)
                    {
                        foreach (var monitor in HardwareID.Monitors)
                        {
                            var serial = monitor.SerialNumber;

                            if (serial.Length <= 1)
                                continue;

                            if (SpoofedHardwareID.Monitors.FindAll(m => m.SerialNumber == serial).Count > 0)
                            {
                                Custom.WriteLine($"[Monitor] {monitor.Manufacturer} => {serial}", ConsoleColor.Red);
                            }
                        }
                    }
                    else
                        Custom.WriteLine($"Error grabbing Monitors", ConsoleColor.DarkRed);
                }
                catch (Exception ex) { Custom.WriteLine($"{ex.Message}", ConsoleColor.Red); }

                Custom.WriteLine($"---");

                try
                {
                    if (HardwareID.Partitions != null)
                    {
                        foreach (var partition in HardwareID.Partitions)
                        {
                            var serial = partition.SerialNumber;

                            if (SpoofedHardwareID.Partitions.FindAll(m => m.SerialNumber == serial).Count > 0)
                            {
                                Custom.WriteLine($"[Partition] {partition.DeviceID} => {serial}", ConsoleColor.DarkYellow);
                            }
                        }
                    }
                    else
                        Custom.WriteLine($"Error grabbing Partitions", ConsoleColor.DarkRed);
                }
                catch (Exception ex) { Custom.WriteLine($"{ex.Message}", ConsoleColor.Red); }

                try
                {
                    if (HardwareID.NearbyDevices != null)
                    {
                        foreach (var arp in HardwareID.NearbyDevices)
                        {
                            if (string.IsNullOrEmpty(arp.Mac))
                                continue;

                            if (arp.Mac.Length < 3)
                                continue;

                            if (SpoofedHardwareID.NearbyDevices != null)
                            {
                                if (SpoofedHardwareID.NearbyDevices.FindAll(m => m.Mac == arp.Mac).Count > 0)
                                {
                                    Custom.WriteLine($"[NearbyDevice] {arp.Address} => {arp.Mac.Replace("-", ":")}", ConsoleColor.DarkYellow);
                                }
                            }
                        }
                    }
                    else
                        Custom.WriteLine($"Error grabbing Nearby devices", ConsoleColor.DarkRed);
                }
                catch (Exception ex) { Custom.WriteLine($"{ex.Message}", ConsoleColor.Red); }

                try
                {
                    if (SpoofedHardwareID.OSInformation != null)
                    {
                        if (HardwareID.OSInformation.MachineID == SpoofedHardwareID.OSInformation.MachineID)
                        {
                            Custom.WriteLine($"[Windows] MachineID => {SpoofedHardwareID.OSInformation.MachineID}", ConsoleColor.DarkYellow);
                        }

                        if (HardwareID.OSInformation.MachineGuid == SpoofedHardwareID.OSInformation.MachineGuid)
                        {
                            Custom.WriteLine($"[Windows] MachineGuid => {SpoofedHardwareID.OSInformation.MachineGuid}", ConsoleColor.Red);
                        }

                        if (HardwareID.OSInformation.ProductID == SpoofedHardwareID.OSInformation.ProductID)
                        {
                            Custom.WriteLine($"[Windows] ProductID => {SpoofedHardwareID.OSInformation.ProductID}", ConsoleColor.Red);
                        }

                        if (HardwareID.OSInformation.InstallDate == SpoofedHardwareID.OSInformation.InstallDate)
                        {
                            Custom.WriteLine($"[Windows] InstallDate => {SpoofedHardwareID.OSInformation.InstallDate}", ConsoleColor.Red);
                        }

                        if(HardwareID.OSInformation.SusClientId == SpoofedHardwareID.OSInformation.SusClientId)
                        {
                            Custom.WriteLine($"[Windows] SusClientId => {SpoofedHardwareID.OSInformation.SusClientId}", ConsoleColor.Red);
                        }

                        Custom.WriteLine($"---");
                        Custom.WriteLine("[Windows] Username Security Identifiers:");
                        Custom.WriteLine($"---");
                        foreach (var user in HardwareID.OSInformation.SIDs)
                        {
                            var sid = user.SID;


                            if (SpoofedHardwareID.OSInformation.SIDs.FindAll(m => m.SID == sid).Count > 0)
                            {
                                Custom.WriteLine($"{user.Username}(SID) => {sid}", ConsoleColor.DarkYellow);
                            }
                        }

                        Custom.WriteLine($"---");

                        if (SpoofedHardwareID.OSInformation.Username.Contains('@'))
                        {
                            Custom.WriteLine($"Windows Username '{SpoofedHardwareID.OSInformation.Username}' contains email, should be offline account", ConsoleColor.Yellow);
                        }

                        if (!SpoofedHardwareID.OSInformation.SecureBoot)
                        {
                            Custom.WriteLine($"Secureboot is disabled, could raise flags to EasyAntiCheat", ConsoleColor.Yellow);
                        }
                    }
                    else
                        Custom.WriteLine($"Error grabbing OSInformation", ConsoleColor.DarkRed);
                }
                catch (Exception ex) { Custom.WriteLine($"{ex.Message}", ConsoleColor.Red); }

                try
                {
                    if (SpoofedHardwareID.BIOS != null)
                    {
                        if (!SpoofedHardwareID.BIOS.ReleaseDate.Contains("2024") && !SpoofedHardwareID.BIOS.ReleaseDate.Contains("2025"))
                        {
                            Custom.WriteLine($"Consider updating BIOS: '{SpoofedHardwareID.BIOS.ReleaseDate}'", ConsoleColor.Yellow);
                        }
                    }
                    else
                        Custom.WriteLine($"Error grabbing Bios", ConsoleColor.DarkRed);
                }
                catch (Exception ex) { Custom.WriteLine($"{ex.Message}", ConsoleColor.Red); }
            }

            Custom.WriteLine("Program complete");

            Console.ReadLine();
        }

        static bool IsAdministrator()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            return false;
        }
    }
}