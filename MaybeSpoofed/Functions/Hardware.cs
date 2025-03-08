using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MaybeSpoofed.Classes;
using Microsoft.Management.Infrastructure;
using Microsoft.Win32;
using static MaybeSpoofed.Classes.Components;

namespace MaybeSpoofed.Functions
{
    public partial class Hardware
    {
        [GeneratedRegex(@"(\d+\.\d+\.\d+\.\d+)\s+([a-fA-F0-9:-]{17})", RegexOptions.IgnoreCase)]
        private static partial Regex ArpEntryRegex();

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
                NearbyDevices = GetAllArpEntries(),
                WindowsFastStartup = GetWindowsFastStartup()
            };

            return _hwid;
        }

        public static bool GetWindowsFastStartup()
        {
            Custom.WriteLine($"Regedit -> SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Power", ConsoleColor.Cyan);

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
                catch { Custom.WriteLine($"Regedit -> SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Power", ConsoleColor.DarkRed); }
            }
            return false;
        }

        public static List<Components.NetworkAdapter> GetNetworkAdapters()
        {
            Custom.WriteLine($"Query -> Win32_NetworkAdapter", ConsoleColor.Cyan);

            List<Components.NetworkAdapter> _networkadapters = [];

            try
            {
                using var session = CimSession.Create(null);

                // Dictionary to map network adapter Index to SettingID (GUID)
                Dictionary<uint, string> adapterGuids = [];

                // Query to get the GUIDs from Win32_NetworkAdapterConfiguration
                foreach (var obj in session.QueryInstances("root\\cimv2", "WQL", "SELECT Index, SettingID FROM Win32_NetworkAdapterConfiguration"))
                {
                    if (obj.CimInstanceProperties["Index"]?.Value is uint index &&
                        obj.CimInstanceProperties["SettingID"]?.Value is string settingId)
                    {
                        adapterGuids[index] = settingId;
                    }
                }

                // Query to get network adapter information
                foreach (var obj in session.QueryInstances("root\\cimv2", "WQL", "SELECT * FROM Win32_NetworkAdapter"))
                {
                    // Skip adapters without MAC address
                    var macAddress = obj.CimInstanceProperties["MACAddress"]?.Value?.ToString();
                    if (string.IsNullOrEmpty(macAddress))
                        continue; // Skip this adapter if MAC address is not available

                    uint? index = obj.CimInstanceProperties["Index"]?.Value as uint?;

                    Components.NetworkAdapter adapter = new()
                    {
                        Name = obj.CimInstanceProperties["Name"]?.Value.ToString() ?? string.Empty,
                        Mac = macAddress,
                        Type = obj.CimInstanceProperties["AdapterType"]?.Value.ToString() ?? string.Empty,
                        Manufacturer = obj.CimInstanceProperties["Manufacturer"]?.Value.ToString() ?? string.Empty,
                        Guid = index.HasValue && adapterGuids.TryGetValue(index.Value, out string? value) ? value : string.Empty
                    };

                    _networkadapters.Add(adapter);
                }

                return _networkadapters;
            }
            catch { Custom.WriteLine($"Query -> Win32_NetworkAdapter", ConsoleColor.DarkRed); }
            return null!;
        }

        public static List<Components.Storage> GetHardDrives()
        {
            Custom.WriteLine($"Query -> Win32_DiskDrive", ConsoleColor.Cyan);

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
            catch { Custom.WriteLine($"Query -> Win32_DiskDrive", ConsoleColor.DarkRed); }

            return null!;
        }

        public static List<Components.PhysicalMemory> GetRam()
        {
            Custom.WriteLine($"Query -> Win32_PhysicalMemory", ConsoleColor.Cyan);
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
            catch { Custom.WriteLine($"Query -> Win32_PhysicalMemory", ConsoleColor.DarkRed); }

            return null!;
        }

        public static Components.Bios GetBios()
        {
            Custom.WriteLine($"Query -> Win32_BIOS", ConsoleColor.Cyan);
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
            catch { Custom.WriteLine($"Query -> Win32_BIOS", ConsoleColor.DarkRed); }
            return null!;
        }

        public static Components.Motherboard GetMotherboard()
        {
            Custom.WriteLine($"Query -> Win32_BaseBoard", ConsoleColor.Cyan);
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
            catch { Custom.WriteLine($"Query -> Win32_BaseBoard", ConsoleColor.DarkRed); }
            return null!;
        }

        public static List<Components.Processor> GetCPUs()
        {
            Custom.WriteLine($"Query -> Win32_Processor", ConsoleColor.Cyan);

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
            catch { Custom.WriteLine($"Query -> Win32_Processor", ConsoleColor.DarkRed);  }

            return null!;
        }

        public static Components.OperatingSystem GetOperatingSystem()
        {
            Custom.WriteLine($"Query -> Win32_OperatingSystem", ConsoleColor.Cyan);

            try
            {
                using var session = CimSession.Create(null);
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
            catch { Custom.WriteLine($"Query -> Win32_OperatingSystem", ConsoleColor.DarkRed); }

            return null!;
        }
        
        public static Components.System GetSystemInformation()
        {
            Custom.WriteLine($"Query -> Win32_ComputerSystemProduct", ConsoleColor.Cyan);

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
            catch { Custom.WriteLine($"Query -> Win32_ComputerSystemProduct", ConsoleColor.DarkRed); }

            return null!;
        }

        public static List<Components.Monitor> GetMonitors()
        {
            Custom.WriteLine($"Query -> WmiMonitorID", ConsoleColor.Cyan);

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
            catch { Custom.WriteLine($"Query -> WmiMonitorID", ConsoleColor.DarkRed); }

            return null!;
        }

        public static List<BluetoothDevice> GetBluetoothDevices()
        {
            Custom.WriteLine($"Query -> Win32_PnPEntity %Bluetooth%", ConsoleColor.Cyan);

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
            catch { Custom.WriteLine($"Query -> Win32_PnPEntity", ConsoleColor.DarkRed); }

            return null!;
        }

        public static TrustedPlatFormModule GetTrustedPlatFormModule()
        {
            Custom.WriteLine($"Query -> Win32_Tpm", ConsoleColor.Cyan);

            try
            {
                using var session = CimSession.Create(null);
                var instances = session.QueryInstances("root\\cimv2\\Security\\MicrosoftTpm", "WQL", "SELECT * FROM Win32_Tpm");

                if (instances == null || !instances.Any())
                {
                    Custom.WriteLine("Win32_Tpm is disabled", ConsoleColor.Cyan);
                    return null!;
                }

                foreach (var obj in instances)
                {
                    return new TrustedPlatFormModule
                    {
                        ManufacturerID = obj.CimInstanceProperties["ManufacturerID"]?.Value?.ToString() ?? string.Empty,
                        ManufacturerVersion = obj.CimInstanceProperties["ManufacturerVersion"]?.Value?.ToString() ?? string.Empty,
                        Version = obj.CimInstanceProperties["SpecVersion"]?.Value?.ToString() ?? string.Empty,
                        IsTPMPResent = true,
                        Activated = obj.CimInstanceProperties["IsActivated_InitialValue"]?.Value?.ToString() ?? string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                Custom.WriteLine($"Query -> Win32_Tpm: {ex.Message}", ConsoleColor.DarkRed);
            }

            return null!;
        }

        public static List<VideoController> GetVideoControllers()
        {
            List<VideoController> _gpus = [];

            Custom.WriteLine($"Query -> Win32_VideoController", ConsoleColor.Cyan);

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
                        Custom.WriteLine($"Supported GPU Found -> {videoController.Name}", ConsoleColor.Cyan);

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

                            Custom.WriteLine($"Command -> nvidia-smi", ConsoleColor.Cyan);
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
                            Custom.WriteLine($"Command -> nvidia-smi", ConsoleColor.DarkRed);
                        }
                    }
                    else
                    {
                        Custom.WriteLine($"Not supported GPU found -> {videoController.Name}", ConsoleColor.DarkRed);
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
            if (index == -1) 
                return string.Empty;

            int startIndex = output.IndexOf(':', index) + 1;
            int endIndex = output.IndexOf('\n', startIndex);
            string value = output[startIndex..endIndex].Trim();

            return string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public static List<ArpTable> GetAllArpEntries()
        {
            List<ArpTable> routerMacs = [];

            try
            {
                Custom.WriteLine($"Command -> arp -a", ConsoleColor.Cyan);
                ProcessStartInfo pro = new("cmd", "/C arp -a")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process? process = Process.Start(pro);
                if (process == null)
                {
                    Custom.WriteLine("Command -> Maybe permission error?", ConsoleColor.DarkRed);
                    return null!;
                }

                using var reader = process.StandardOutput;
                string output = reader.ReadToEnd();

                // Regular expression to match IP and MAC addresses
                Regex arpEntryPattern = ArpEntryRegex();

                // Use Regex to find all matches in the output
                MatchCollection matches = arpEntryPattern.Matches(output);

                Custom.WriteLine($"Found ARP entries: {matches.Count}", ConsoleColor.DarkMagenta);

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

                    Custom.WriteLine($"NearByDevice -> {internetAddress} {physicalAddress}", ConsoleColor.DarkMagenta);

                    routerMacs.Add(new ArpTable() { Address = internetAddress, Mac = physicalAddress });
                }

                return routerMacs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Command -> ARP: {ex.Message}");
            }

            Custom.WriteLine("Command -> Maybe permission error?", ConsoleColor.DarkRed);
            return null!;
        }
    }
}
