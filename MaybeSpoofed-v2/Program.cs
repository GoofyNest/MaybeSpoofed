using MaybeSpoofed_v2.Classes;
using MaybeSpoofed_v2.Functions;
using Newtonsoft.Json;

namespace MaybeSpoofed
{
    internal class Program
    {
        static void Main()
        {
            if (!Directory.Exists("config"))
                Directory.CreateDirectory("config");

            Components HardwareID = new();
            Components SpoofedHardwareID = null!;

            if (File.Exists("config/hardware.json"))
            {
                Custom.WriteLine("Loading existing hardware file", ConsoleColor.Green);

                var fileContent = File.ReadAllText("config/hardware.json");

                var tempSettings = JsonConvert.DeserializeObject<Components>(File.ReadAllText("config/hardware.json"));

                if(tempSettings == null)
                {
                    Custom.WriteLine("Something went wrong, please delete config folder", ConsoleColor.DarkRed);
                    Console.ReadLine();
                    return;
                }

                if (tempSettings.ProgramVersion != "v0.1")
                {
                    Custom.WriteLine("Outdated program json, please delete config folder and restart application", ConsoleColor.Red);
                    Console.ReadLine();
                    return;
                }

                HardwareID = tempSettings;

                Custom.WriteLine("Grabbing spoofed serials", ConsoleColor.Cyan);

                SpoofedHardwareID = Hardware.GetHardwareID();

                File.WriteAllText("config/spoofed.json", JsonConvert.SerializeObject(SpoofedHardwareID, Formatting.Indented));
            }
            else
            {
                Custom.WriteLine("Generating new hardware file", ConsoleColor.Cyan);

                HardwareID = Hardware.GetHardwareID();

                File.WriteAllText("config/hardware.json", JsonConvert.SerializeObject(HardwareID, Formatting.Indented));
            }

            if(SpoofedHardwareID == null)
            {
                Custom.WriteLine("Please spoof and restart program to see if you are spoofed", ConsoleColor.Yellow);
            }
            else
            {
                Custom.WriteLine("---------------------------------------", ConsoleColor.Magenta);
                Custom.WriteLine("Hardware result:");
                Custom.WriteLine("---------------------------------------", ConsoleColor.Magenta);

                if(SpoofedHardwareID.TPM != null)
                {
                    Custom.WriteLine("Trusted platform module(TPM) is enabled, please disable it in BIOS", ConsoleColor.Red);
                }

                if(SpoofedHardwareID.BluetoothDevices.Count > 0)
                {
                    Custom.WriteLine("Bluetooth card is present, please disable it in BIOS", ConsoleColor.Red);
                }

                if(SpoofedHardwareID.WindowsFastStartup)
                {
                    Custom.WriteLine("Windows fast startup is enabled, can lead to bans if using `Shutdown pc`", ConsoleColor.Yellow);
                }

                if(HardwareID.MotherboardInformation.SerialNumber == SpoofedHardwareID.MotherboardInformation.SerialNumber)
                {
                    Custom.WriteLine($"MotherboardInformation SerialNumber '{SpoofedHardwareID.MotherboardInformation.SerialNumber}' not spoofed", ConsoleColor.Red);
                }

                if(HardwareID.SystemInformation.UUID == SpoofedHardwareID.SystemInformation.UUID)
                {
                    Custom.WriteLine($"SystemInformation UUID '{SpoofedHardwareID.MotherboardInformation.SerialNumber}' not spoofed", ConsoleColor.Red);
                }

                foreach(var ram in HardwareID.Ram)
                {
                    var serial = ram.SerialNumber;

                    if (serial == "00000000")
                        continue;

                    if (SpoofedHardwareID.Ram.FindAll(m => m.SerialNumber == serial).Count > 0)
                    {
                        Custom.WriteLine($"Ram serial '{serial}' not spoofed", ConsoleColor.Red);
                    }
                }

                foreach(var disk in HardwareID.DiskDrives)
                {
                    var serial = disk.SerialNumber;

                    if(SpoofedHardwareID.DiskDrives.FindAll(m => m.SerialNumber == serial).Count > 0)
                    {
                        Custom.WriteLine($"Disk drive serial '{serial}' not spoofed", ConsoleColor.Red);
                    }
                }

                foreach(var gpu in HardwareID.GPUs)
                {
                    var serial = gpu.SerialNumber;
                    var UUID = gpu.UUID;

                    if(string.IsNullOrWhiteSpace(UUID))
                    {
                        Custom.WriteLine($"We dont support your GPU, trust your spoofer provider or check manually", ConsoleColor.DarkRed);
                    }
                    else
                    {
                        if (SpoofedHardwareID.GPUs.FindAll(m => m.UUID == UUID).Count > 0)
                        {
                            Custom.WriteLine($"GPU UUID '{UUID}' not spoofed", ConsoleColor.Red);
                        }
                    }
                }

                foreach(var network in HardwareID.NetworkAdapters)
                {
                    var mac = network.Mac;

                    if (network.Name.StartsWith("WAN Miniport"))
                        continue;

                    if (SpoofedHardwareID.NetworkAdapters.FindAll(m => m.Mac == mac).Count > 0)
                    {
                        Custom.WriteLine($"Network Mac '{mac}' not spoofed", ConsoleColor.Red);
                    }
                }

                foreach(var monitor in HardwareID.Monitors)
                {
                    var serial = monitor.SerialNumber;

                    if (serial.Length <= 1)
                        continue;

                    if (SpoofedHardwareID.Monitors.FindAll(m => m.SerialNumber == serial).Count > 0)
                    {
                        Custom.WriteLine($"Monitor serial '{serial}' not spoofed", ConsoleColor.Red);
                    }
                }

                foreach(var mac in HardwareID.RouterMacs)
                {
                    if (string.IsNullOrEmpty(mac))
                        continue;

                    if(SpoofedHardwareID.RouterMacs.Contains(mac))
                    {
                        Custom.WriteLine($"Router Mac '{mac}' not spoofed", ConsoleColor.Yellow);
                    }
                }

                if(SpoofedHardwareID.OSInformation.Username.Contains("@"))
                {
                    Custom.WriteLine($"Windows username '{SpoofedHardwareID.OSInformation.Username}' contains email, should be offline account", ConsoleColor.Yellow);
                }

                if(!SpoofedHardwareID.OSInformation.SecureBoot)
                {
                    Custom.WriteLine($"Secureboot is disabled, will raise flags to EAC", ConsoleColor.Yellow);
                }

                if(!SpoofedHardwareID.BIOS.ReleaseDate.Contains("2024") && !SpoofedHardwareID.BIOS.ReleaseDate.Contains("2025"))
                {
                    Custom.WriteLine($"Recommended to update BIOS Version '{SpoofedHardwareID.BIOS.ReleaseDate}'", ConsoleColor.Yellow);
                }
            }

            Console.ReadLine();
        }
    }
}