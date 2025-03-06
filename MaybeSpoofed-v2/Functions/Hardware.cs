using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MaybeSpoofed;
using MaybeSpoofed_v2.Classes;
using Microsoft.Management.Infrastructure;
using Microsoft.Win32;
using static MaybeSpoofed_v2.Classes.Components;

namespace MaybeSpoofed_v2.Functions
{
    public class Hardware
    {
        public static readonly Dictionary<string, string> ManufacturerMap = new()
        {
            { "BNQ", "BenQ" }, { "ACR", "Acer" }, { "DEL", "Dell" }, { "HWP", "HP" },
            { "SAM", "Samsung" }, { "LGD", "LG" }, { "NEC", "NEC Display" }, { "PHL", "Philips" },
            { "VSC", "ViewSonic" }, { "ASU", "ASUS" }, { "EPI", "AOC" }
        };

        public static Components GetHardwareID()
        {
            Components _hwid = new()
            {
                SystemInformation = GetSystemInformation(),
                OSInformation = GetOperatingSystem(),
                GPUs = GetVideoControllers(),
                Monitors = GetMonitors(),
                BluetoothDevices = GetBluetoothDevices(),
                TPM = GetTrustedPlatFormModule(),
                CPUs = GetCPUs(),
                MotherboardInformation = GetMotherboard(),
                BIOS = GetBios(),
                Ram = GetRam(),
                DiskDrives = GetHardDrives(),
                NetworkAdapters = GetNetworkAdapters(),
                RouterMacs = GetAllArpEntries(),
                WindowsFastStartup = GetWindowsFastStartup()
            };

            return _hwid;
        }

        public static bool GetWindowsFastStartup()
        {
            // Access the registry key where the Fast Startup setting is stored
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Power");

                    if (key != null)
                    {
                        var value = key.GetValue("HiberbootEnabled");

                        if (value != null && value.ToString() == "1")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch { }
            }
            return false;
        }

        public static List<string> GetRouterMacs()
        {
            List<string> RouterMacs = [];

            GetAllArpEntries();

            return null;

            try
            {
                using var session = CimSession.Create(null);

                // Query to get IP configuration details (for enabled adapters)
                foreach (var obj in session.QueryInstances("root\\cimv2", "WQL", "SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = true"))
                {
                    // Skip adapters without a MAC address
                    var macAddress = obj.CimInstanceProperties["MACAddress"]?.Value?.ToString();
                    if (string.IsNullOrEmpty(macAddress))
                        continue; // Skip this adapter if MAC address is not available

                    // Get Default Gateway addresses
                    var defaultGateway = obj.CimInstanceProperties["DefaultIPGateway"]?.Value as string[];
                    if (defaultGateway != null && defaultGateway.Length > 0)
                    {
                        foreach (var gateway in defaultGateway)
                        {
                            RouterMacs.Add(GetMacAddressFromArpCache(gateway));
                        }
                    }
                }

                return RouterMacs;
            }
            catch { }
            return null!;
        }

        public static List<Components.NetworkAdapter> GetNetworkAdapters()
        {
            List<Components.NetworkAdapter> _networkadapters = [];

            try
            {
                using var session = CimSession.Create(null);

                // Query to get network adapter information
                foreach (var obj in session.QueryInstances("root\\cimv2", "WQL", "SELECT * FROM Win32_NetworkAdapter"))
                {
                    // Skip adapters without MAC address
                    var macAddress = obj.CimInstanceProperties["MACAddress"]?.Value?.ToString();
                    if (string.IsNullOrEmpty(macAddress))
                        continue; // Skip this adapter if MAC address is not available

                    Components.NetworkAdapter adapter = new()
                    {
                        Name = obj.CimInstanceProperties["Name"]?.Value.ToString() ?? string.Empty,
                        Mac = macAddress,
                        Type = obj.CimInstanceProperties["AdapterType"]?.Value.ToString() ?? string.Empty,
                        Manufacturer = obj.CimInstanceProperties["Manufacturer"]?.Value.ToString() ?? string.Empty,
                    };

                    _networkadapters.Add(adapter);
                }

                return _networkadapters;
            }
            catch { }
            return null!;
        }

        public static List<Components.Storage> GetHardDrives()
        {
            List<Components.Storage> _disks = [];

            try
            {
                using var session = CimSession.Create(null);
                // Query to get all disk drive information
                foreach (var obj in session.QueryInstances("root\\cimv2", "WQL", "SELECT * FROM Win32_DiskDrive"))
                {
                    string Partitions = obj.CimInstanceProperties["Partitions"]?.Value.ToString() ?? "0";
                    string BytesPerSector = obj.CimInstanceProperties["BytesPerSector"]?.Value.ToString() ?? "0";

                    Components.Storage Disk = new()
                    {
                        DeviceID = obj.CimInstanceProperties["DeviceID"]?.Value.ToString() ?? string.Empty,
                        Model = obj.CimInstanceProperties["Model"]?.Value.ToString() ?? string.Empty,
                        SerialNumber = obj.CimInstanceProperties["SerialNumber"]?.Value.ToString() ?? string.Empty,
                        Size = obj.CimInstanceProperties["Size"]?.Value.ToString() ?? string.Empty,
                        Type = obj.CimInstanceProperties["MediaType"]?.Value.ToString() ?? string.Empty,

                        Partitions = int.Parse(Partitions),

                        Manufacturer = obj.CimInstanceProperties["Manufacturer"]?.Value.ToString() ?? string.Empty,
                        BytesPerSector = int.Parse(BytesPerSector),
                        FirmwareRevision = obj.CimInstanceProperties["FirmwareRevision"]?.Value.ToString() ?? string.Empty,
                    };

                    _disks.Add(Disk);
                }

                return _disks;
            }
            catch { }

            return null!;
        }

        public static List<Components.PhysicalMemory> GetRam()
        {
            List<Components.PhysicalMemory> _ram = [];

            try
            {
                using var session = CimSession.Create(null);
                // Query to get all physical memory information
                foreach (var obj in session.QueryInstances("root\\cimv2", "WQL", "SELECT * FROM Win32_PhysicalMemory"))
                {
                    Components.PhysicalMemory Ram = new()
                    {
                        Location = obj.CimInstanceProperties["DeviceLocator"]?.Value.ToString() ?? string.Empty,
                        Capacity = obj.CimInstanceProperties["Capacity"]?.Value.ToString() ?? string.Empty,
                        Manufacturer = obj.CimInstanceProperties["Manufacturer"]?.Value.ToString() ?? string.Empty,
                        Speed = obj.CimInstanceProperties["Speed"]?.Value.ToString() ?? string.Empty,
                        SerialNumber = obj.CimInstanceProperties["SerialNumber"]?.Value.ToString() ?? string.Empty,
                        PartNumber = obj.CimInstanceProperties["PartNumber"]?.Value.ToString() ?? string.Empty,
                        Voltage = obj.CimInstanceProperties["ConfiguredVoltage"]?.Value.ToString() ?? string.Empty,
                        ClockSpeed = obj.CimInstanceProperties["ConfiguredClockSpeed"]?.Value.ToString() ?? string.Empty,
                    };

                    _ram.Add(Ram);
                }
                return _ram;
            }
            catch { }

            return null!;
        }

        public static Components.Bios GetBios()
        {
            try
            {
                using var session = CimSession.Create(null); // Local session
                foreach (var obj in session.QueryInstances("root\\cimv2", "WQL", "SELECT * FROM Win32_BIOS"))
                {
                    Components.Bios Bios = new()
                    {
                        Manufacturer = obj.CimInstanceProperties["Manufacturer"]?.Value.ToString() ?? string.Empty,
                        Version = obj.CimInstanceProperties["Version"]?.Value.ToString() ?? string.Empty,
                        ReleaseDate = obj.CimInstanceProperties["ReleaseDate"]?.Value.ToString() ?? string.Empty,
                        SerialNumber = obj.CimInstanceProperties["SerialNumber"]?.Value.ToString() ?? string.Empty,
                    };

                    // Check if the EFI system partition exists
                    string efiPath = @"\\?\Volume{xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}\EFI\";

                    if (Directory.Exists(efiPath))
                    {
                        Bios.Mode = "UEFI";
                    }
                    else
                    {
                        Bios.Mode = "Legacy";
                    }

                    return Bios;
                }
            }
            catch { }
            return null!;
        }

        public static Components.Motherboard GetMotherboard()
        {
            try
            {
                using var session = CimSession.Create(null); // Local session
                foreach (var obj in session.QueryInstances("root\\cimv2", "WQL", "SELECT * FROM Win32_BaseBoard"))
                {
                    Components.Motherboard motherboard = new()
                    {
                        Manufacturer = obj.CimInstanceProperties["Manufacturer"]?.Value.ToString() ?? string.Empty,
                        Product = obj.CimInstanceProperties["Product"]?.Value.ToString() ?? string.Empty,
                        SerialNumber = obj.CimInstanceProperties["SerialNumber"]?.Value.ToString() ?? string.Empty,
                        Version = obj.CimInstanceProperties["Version"]?.Value.ToString() ?? string.Empty,
                        Caption = obj.CimInstanceProperties["Caption"]?.Value.ToString() ?? string.Empty,
                    };

                    return motherboard;
                }
            }
            catch { }
            return null!;
        }

        public static List<Components.Processor> GetCPUs()
        {
            List<Components.Processor> _cpus = [];

            try
            {
                using var session = CimSession.Create(null);
                foreach (var obj in session.QueryInstances("root\\cimv2", "WQL", "SELECT * FROM Win32_Processor"))
                {
                    string coreCount = obj.CimInstanceProperties["NumberOfCores"]?.Value.ToString() ?? "0";
                    string threadCount = obj.CimInstanceProperties["NumberOfLogicalProcessors"]?.Value.ToString() ?? "0";
                    string Virtualization = obj.CimInstanceProperties["VirtualizationFirmwareEnabled"]?.Value.ToString() ?? "False";

                    Components.Processor Processor = new()
                    {
                        Name = obj.CimInstanceProperties["Name"]?.Value.ToString() ?? string.Empty,
                        Manufacturer = obj.CimInstanceProperties["Manufacturer"]?.Value.ToString() ?? string.Empty,
                        MaxClockSpeed = obj.CimInstanceProperties["MaxClockSpeed"]?.Value.ToString() ?? string.Empty,
                        SerialNumber = obj.CimInstanceProperties["ProcessorId"]?.Value.ToString() ?? string.Empty,
                        CoreCount = int.Parse(coreCount),
                        ThreadCount = int.Parse(threadCount),
                        SocketDesignation = obj.CimInstanceProperties["SocketDesignation"]?.Value.ToString() ?? string.Empty,
                        CurrentClockSpeed = obj.CimInstanceProperties["CurrentClockSpeed"]?.Value.ToString() ?? string.Empty,
                        Virtualization = bool.Parse(Virtualization),
                        Family = obj.CimInstanceProperties["Family"]?.Value.ToString() ?? string.Empty,
                        DataWidth = obj.CimInstanceProperties["DataWidth"]?.Value.ToString() ?? string.Empty,
                        AddressWidth = obj.CimInstanceProperties["AddressWidth"]?.Value.ToString() ?? string.Empty,
                    };

                    _cpus.Add(Processor);
                }
                return _cpus;
            }
            catch { }

            return null!;
        }

        public static Components.OperatingSystem GetOperatingSystem()
        {
            try
            {
                using (var session = CimSession.Create(null))
                {
                    // Query to get all details about the operating system
                    foreach (var obj in session.QueryInstances("root\\cimv2", "WQL", "SELECT * FROM Win32_OperatingSystem"))
                    {
                        Components.OperatingSystem OSInformation = new()
                        {
                            Caption = obj.CimInstanceProperties["Caption"]?.Value.ToString() ?? string.Empty,
                            Version = obj.CimInstanceProperties["Version"]?.Value.ToString() ?? string.Empty,
                            BuildNumber = obj.CimInstanceProperties["BuildNumber"]?.Value.ToString() ?? string.Empty,
                            ProductType = obj.CimInstanceProperties["ProductType"]?.Value.ToString() ?? string.Empty,
                            Manufacturer = obj.CimInstanceProperties["Manufacturer"]?.Value.ToString() ?? string.Empty,
                            Architecture = obj.CimInstanceProperties["OSArchitecture"]?.Value.ToString() ?? string.Empty,

                            LastBoot = obj.CimInstanceProperties["LastBootUpTime"]?.Value.ToString() ?? string.Empty,
                            Username = obj.CimInstanceProperties["RegisteredUser"]?.Value.ToString() ?? string.Empty,
                            SerialNumber = obj.CimInstanceProperties["SerialNumber"]?.Value.ToString() ?? string.Empty,
                        };

                        // Check SecureBootEnabled property
                        var secureBoot = obj.CimInstanceProperties["SecureBootEnabled"]?.Value;
                        if (secureBoot != null)
                        {
                            OSInformation.SecureBoot = true;
                        }
                        else
                        {
                            OSInformation.SecureBoot = false;
                        }

                        return OSInformation;
                    }
                }
            }
            catch { }

            return null!;
        }
        
        public static Components.System GetSystemInformation()
        {
            try
            {
                using var session = CimSession.Create(null);
                foreach (var obj in session.QueryInstances("root\\cimv2", "WQL", "SELECT * FROM Win32_ComputerSystemProduct"))
                {
                    Components.System SystemInformation = new()
                    {
                        ProductName = obj.CimInstanceProperties["Name"]?.Value.ToString() ?? string.Empty,
                        SystemSerialNumber = obj.CimInstanceProperties["IdentifyingNumber"]?.Value.ToString() ?? string.Empty,
                        Vendor = obj.CimInstanceProperties["Vendor"]?.Value.ToString() ?? string.Empty,
                        SystemVersion = obj.CimInstanceProperties["Version"]?.Value.ToString() ?? string.Empty,
                        UUID = obj.CimInstanceProperties["UUID"]?.Value.ToString() ?? string.Empty,
                    };

                    return SystemInformation;
                }
            }
            catch { }

            return null!;
        }

        public static List<Components.Monitor> GetMonitors()
        {
            List<Components.Monitor> _monitors = [];

            try
            {
                using var session = CimSession.Create(null);
                var instances = session.QueryInstances("root\\wmi", "WQL", "SELECT * FROM WmiMonitorID");

                if (!instances.Any())
                    return null!;

                foreach (var obj in instances)
                {
                    Components.Monitor Monitor = new();

                    var manufacturerBytes = obj.CimInstanceProperties["ManufacturerName"]?.Value as ushort[];
                    var productBytes = obj.CimInstanceProperties["ProductCodeID"]?.Value as ushort[];
                    var serialBytes = obj.CimInstanceProperties["SerialNumberID"]?.Value as ushort[];

                    string manufacturerCode = Hardware.ConvertToString(manufacturerBytes);
                    string manufacturer = Hardware.ManufacturerMap.TryGetValue(manufacturerCode, out string? fullName)
                        ? fullName : manufacturerCode; // Use full name if found
                    string product = Hardware.ConvertToString(productBytes);
                    string serial = Hardware.ConvertToString(serialBytes);

                    Monitor.Manufacturer = manufacturer;
                    Monitor.SerialNumber = serial;

                    _monitors.Add(Monitor);
                }

                return _monitors;
            }
            catch { }

            return null!;
        }

        public static List<BluetoothDevice> GetBluetoothDevices()
        {
            List<BluetoothDevice> _bluetooth = [];

            try
            {
                using var session = CimSession.Create(null);
                var instances = session.QueryInstances("root\\cimv2", "WQL", "SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%Bluetooth%'");

                if (instances.Any())
                {
                    foreach (var obj in instances)
                    {
                        BluetoothDevice BluetoothDevice = new()
                        {
                            Name = obj.CimInstanceProperties["Name"]?.Value.ToString() ?? string.Empty,
                            Manufacturer = obj.CimInstanceProperties["Manufacturer"]?.Value.ToString() ?? string.Empty,
                            Status = obj.CimInstanceProperties["Status"]?.Value.ToString() ?? string.Empty
                        };
                        _bluetooth.Add(BluetoothDevice);
                    }
                }
                return _bluetooth;
            }
            catch { }

            return null!;
        }

        public static TrustedPlatFormModule GetTrustedPlatFormModule()
        {
            try
            {
                using var session = CimSession.Create(null);
                // Query to get TPM information
                var instances = session.QueryInstances("root\\cimv2", "WQL", "SELECT * FROM Wintr32_Tpm");

                // If no TPM is found, instances will be empty
                if (instances == null || !instances.Any())
                {
                    Console.WriteLine("No TPM module found or it may be disabled in BIOS/UEFI.");
                    return null!;
                }

                foreach (var obj in instances)
                {
                    TrustedPlatFormModule _tpm = new()
                    {
                        ManufacturerID = obj.CimInstanceProperties["ManufacturerID"]?.Value.ToString() ?? string.Empty,
                        ManufacturerVersion = obj.CimInstanceProperties["ManufacturerVersion"]?.Value.ToString() ?? string.Empty,
                        Version = obj.CimInstanceProperties["Version"]?.Value.ToString() ?? string.Empty,
                        VersionInfo = obj.CimInstanceProperties["VersionInfo"]?.Value.ToString() ?? string.Empty,
                        isTPMPResent = true,
                        Manufacturer = obj.CimInstanceProperties["Manufacturer"]?.Value.ToString() ?? string.Empty,
                        Activated = obj.CimInstanceProperties["IsActivated"]?.Value.ToString() ?? string.Empty,
                    };

                    return _tpm;
                }
            }
            catch { }

            return null!;
        }

        public static List<VideoController> GetVideoControllers()
        {
            List<VideoController> _gpus = [];

            try
            {
                using var session = CimSession.Create(null);
                var instances = session.QueryInstances("root\\cimv2", "WQL", "SELECT * FROM Win32_VideoController");

                foreach (var obj in instances)
                {
                    VideoController videoController = new()
                    {
                        Name = obj.CimInstanceProperties["Name"]?.Value.ToString() ?? string.Empty,
                        DeviceID = obj.CimInstanceProperties["PNPDeviceID"]?.Value.ToString() ?? string.Empty,
                        Status = obj.CimInstanceProperties["Status"]?.Value.ToString() ?? string.Empty
                    };

                    if (videoController.Name.Contains("NVIDIA"))
                    {
                        Custom.WriteLine($"Supported GPU found {videoController.Name}", ConsoleColor.Green);

                        try
                        {
                            // Run the nvidia-smi command and capture the output
                            var startInfo = new ProcessStartInfo
                            {
                                FileName = "nvidia-smi",
                                Arguments = "-q", // Query the full information
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };

                            using Process? process = Process.Start(startInfo);
                            if (process != null)
                            {
                                using var reader = process.StandardOutput;

                                var output = reader.ReadToEnd();

                                // Look for Serial Number, GPU UUID, and vBIOS version in the output
                                videoController.SerialNumber = ExtractValue(output, "Serial Number");
                                videoController.UUID = ExtractValue(output, "GPU UUID");
                                videoController.Version = ExtractValue(output, "VBIOS Version");
                            }
                        }
                        catch
                        {
                            Custom.WriteLine($"Error grabbing information from nvidia-smi", ConsoleColor.Red);
                        }
                    }
                    else
                    {
                        Custom.WriteLine($"Not supported GPU found {videoController.Name}", ConsoleColor.Red);
                    }

                    _gpus.Add(videoController);
                }

                return _gpus;
            }
            catch { }

            return null!;
        } 

        public static string ConvertToString(ushort[]? data)
        {
            if (data == null) return string.Empty;
            return new string(data.Select(c => (char)c).ToArray()).TrimEnd('\0');
        }

        public static string ExtractValue(string output, string label)
        {
            // This method extracts the value corresponding to a given label
            int index = output.IndexOf(label);
            if (index == -1) return string.Empty;

            int startIndex = output.IndexOf(":", index) + 1;
            int endIndex = output.IndexOf("\n", startIndex);
            string value = output.Substring(startIndex, endIndex - startIndex).Trim();

            return string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public static List<string> GetAllArpEntries()
        {
            List<string> routerMacs = new();

            try
            {
                ProcessStartInfo pro = new ProcessStartInfo("cmd", "/C arp -a")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process? process = Process.Start(pro);
                if (process == null)
                {
                    Custom.WriteLine("GetAllArpEntries - Maybe permission error?", ConsoleColor.Red);
                    return null!;
                }

                using var reader = process.StandardOutput;
                string output = reader.ReadToEnd();

                // Regular expression to match IP and MAC addresses
                Regex arpEntryPattern = new(@"(\d+\.\d+\.\d+\.\d+)\s+([a-fA-F0-9:-]{17})", RegexOptions.IgnoreCase);

                // Use Regex to find all matches in the output
                MatchCollection matches = arpEntryPattern.Matches(output);

                Custom.WriteLine($"GetAllArpEntries matches: {matches.Count}", ConsoleColor.DarkMagenta);

                foreach (Match match in matches)
                {
                    string internetAddress = match.Groups[1].Value;
                    string physicalAddress = match.Groups[2].Value;

                    // Ignore multicast/broadcast addresses (224.x.x.x, 239.x.x.x, 255.x.x.x)
                    //if (internetAddress.StartsWith("224.0.0") || internetAddress.StartsWith("239.255") || internetAddress.StartsWith("255"))
                    //    continue;

                    if (physicalAddress.StartsWith("01-00-5e"))
                        continue;

                    if (physicalAddress == "ff-ff-ff-ff-ff-ff")
                        continue;

                    Custom.WriteLine($"GetAllArpEntries found {physicalAddress}");
                    routerMacs.Add(physicalAddress);
                }

                return routerMacs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while retrieving ARP entries: {ex.Message}");
            }

            Custom.WriteLine("GetAllArpEntries is null - Maybe permission error?", ConsoleColor.Red);

            return null!;
        }

        public static string GetMacAddressFromArpCache(string ipAddress)
        {
            try
            {
                // Run the "arp -a" command and capture the output
                ProcessStartInfo pro = new ProcessStartInfo("cmd", $"/C arp -a {ipAddress}")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process? process = Process.Start(pro);

                if (process == null) return null!;

                using var reader = process.StandardOutput;
                string output = reader.ReadToEnd();
                string[] lines = output.Split(Environment.NewLine);
                foreach (string line in lines)
                {
                    // Look for the line containing the MAC address
                    if (line.Contains(ipAddress))
                    {
                        var columns = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                        if (columns.Length >= 3)
                        {
                            return columns[1]; // MAC Address is in the second column
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while retrieving MAC address: {ex.Message}");
            }
            return null!;
        }
    }
}
