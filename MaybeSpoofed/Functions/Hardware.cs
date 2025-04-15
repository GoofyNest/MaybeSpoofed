using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MaybeSpoofed.Classes;
using Microsoft.Management.Infrastructure;
using Microsoft.Win32;
using Spectre.Console;
using static MaybeSpoofed.Classes.Components;

namespace MaybeSpoofed.Functions
{
    public partial class Hardware
    {
        [GeneratedRegex(@"(\d+\.\d+\.\d+\.\d+)\s+([a-fA-F0-9:-]{17})", RegexOptions.IgnoreCase)]
        private static partial Regex ArpEntryRegex();

        private static Components HardwareID = new();

        public static readonly Dictionary<string, string> ManufacturerMap = new()
        {
            { "BNQ", "BenQ" }, { "ACR", "Acer" }, { "DEL", "Dell" }, { "HWP", "HP" },
            { "SAM", "Samsung" }, { "LGD", "LG" }, { "NEC", "NEC Display" }, { "PHL", "Philips" },
            { "VSC", "ViewSonic" }, { "ASU", "ASUS" }, { "EPI", "AOC" }
        };

        public static Components GetHardwareID()
        {
            HardwareID = new();

            AnsiConsole.Status()
                .Start("Grabbing serials...", ctx =>
                {
                    // Optional intro
                    AnsiConsole.MarkupLine("[gray]Please wait while we scan your hardware...[/]");
                    Thread.Sleep(1000);

                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("green"));

                    // SYSTEM
                    ctx.Status("Grabbing system information...");
                    HardwareID.SystemInformation = GetSystemInformation();

                    // OS
                    ctx.Status("Scanning operating system...");
                    HardwareID.OSInformation = GetOperatingSystem();

                    // GPU
                    ctx.Status("Grabbing video controllers...");
                    HardwareID.GPUs = GetVideoControllers();

                    // Monitors
                    ctx.Status("Grabbing monitor information...");
                    HardwareID.Monitors = GetMonitors();

                    // Bluetooth
                    ctx.Status("Fetching bluetooth devices...");
                    HardwareID.BluetoothDevices = GetBluetoothDevices();

                    // TPM
                    ctx.Status("Grabbing TPM information...");
                    HardwareID.TPM = GetTrustedPlatFormModule();

                    // CPUs
                    ctx.Status("Fetching processor info...");
                    HardwareID.CPUs = GetCPUs();

                    // Motherboard
                    ctx.Status("Fetching motherboard info...");
                    HardwareID.MotherboardInformation = GetMotherboard();

                    // BIOS
                    ctx.Status("Fetching BIOS info...");
                    HardwareID.BIOS = GetBios();

                    // RAM
                    ctx.Status("Fetching RAM info...");
                    HardwareID.Ram = GetRam();

                    // Drives
                    ctx.Status("Scanning hard drives...");
                    HardwareID.DiskDrives = GetHardDrives();

                    // Network
                    ctx.Status("Grabbing network adapters...");
                    HardwareID.NetworkAdapters = GetNetworkAdapters();

                    // ARP / Nearby Devices
                    ctx.Status("Scanning for nearby devices...");
                    HardwareID.NearbyDevices = GetAllArpEntries();

                    // Windows Fast Startup
                    ctx.Status("Checking Windows Fast Startup...");
                    HardwareID.WindowsFastStartup = GetWindowsFastStartup();

                    // Partitions
                    ctx.Status("Listing drive partitions...");
                    HardwareID.Partitions = GetPartitions();

                    // Partitions
                    ctx.Status("Checking system identifiers...");
                    GetSystemIdentifiers();

                    Thread.Sleep(500); // tiny delay to smooth the final transition
                });

            

            return HardwareID;
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
                catch(Exception ex) { AnsiConsole.WriteException(ex); }
            }
            return false;
        }

        public static List<Components.NetworkAdapter> GetNetworkAdapters()
        {
            List<Components.NetworkAdapter> _networkadapters = [];

            try
            {
                using var session = CimSession.Create(null);

                // Dictionary to map network adapter Index to SettingID (GUID)
                Dictionary<uint, string> adapterGuids = [];

                // Query to get the GUIDs from Win32_NetworkAdapterConfiguration
                foreach (var obj in session.QueryInstances("root\\cimv2", "WQL", "SELECT Index, SettingID FROM Win32_NetworkAdapterConfiguration"))
                {
                    uint? index = SafeGet<uint>(obj, "Index");

                    string? settingId = SafeGet<string>(obj, "SettingID");

                    if (index == null)
                        continue;

                    if (string.IsNullOrWhiteSpace(settingId))
                        continue;

                    adapterGuids[index.Value] = settingId;
                }

                // Query to get network adapter information
                foreach (var obj in session.QueryInstances("root\\cimv2", "WQL", "SELECT * FROM Win32_NetworkAdapter"))
                {
                    // Skip adapters without MAC address
                    var macAddress = SafeGet<string>(obj, "MACAddress");
                    if (string.IsNullOrWhiteSpace(macAddress))
                        continue; // Skip this adapter if MAC address is not available

                    uint? index = SafeGet<uint>(obj, "Index");

                    Components.NetworkAdapter adapter = new()
                    {
                        Name = SafeGet(obj, "Name"),
                        Mac = macAddress,
                        Type = SafeGet(obj, "AdapterType"),
                        Manufacturer = SafeGet(obj, "Manufacturer"),
                        Guid = index.HasValue && adapterGuids.TryGetValue(index.Value, out string? value) ? value : string.Empty
                    };

                    _networkadapters.Add(adapter);
                }

                return _networkadapters;
            }
            catch(Exception ex) { AnsiConsole.WriteException(ex); }
            return null!;
        }

        public static List<Components.Partition> GetPartitions()
        {
            List<Components.Partition> _partitions = [];
            try
            {
                using var session = CimSession.Create(null);

                // Get partition serial numbers
                foreach (var obj in session.QueryInstances("root\\cimv2", "WQL", "SELECT * FROM Win32_LogicalDisk"))
                {
                    string volumeSerialNumber = SafeGet(obj, "VolumeSerialNumber");
                    string volumeDeviceID = SafeGet(obj, "DeviceID");

                    _partitions.Add(new() { DeviceID = volumeDeviceID, SerialNumber = volumeSerialNumber });

                }
                return _partitions;
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }

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
                    string Partitions = SafeGet(obj, "Partitions");
                    string BytesPerSector = SafeGet(obj, "BytesPerSector");

                    // Get DeviceID (to correlate with logical disk)
                    string deviceID = SafeGet(obj, "DeviceID");

                    Components.Storage Disk = new()
                    {
                        DeviceID = deviceID,
                        Model = SafeGet(obj, "Model"),
                        SerialNumber = SafeGet(obj, "SerialNumber"),
                        Size = SafeGet(obj, "Size"),
                        Type = SafeGet(obj, "MediaType"),

                        Partitions = int.Parse(Partitions),

                        Manufacturer = SafeGet(obj, "Manufacturer"),
                        BytesPerSector = int.Parse(BytesPerSector),
                        FirmwareRevision = SafeGet(obj, "FirmwareRevision"),
                    };

                    _disks.Add(Disk);
                }

                return _disks;
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }

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
                        Location = SafeGet(obj, "DeviceLocator"),
                        Capacity = SafeGet(obj, "Capacity"),
                        Manufacturer = SafeGet(obj, "Manufacturer"),
                        Speed = SafeGet(obj, "Speed"),
                        SerialNumber = SafeGet(obj, "SerialNumber"),
                        PartNumber = SafeGet(obj, "PartNumber"),
                        Voltage = SafeGet(obj, "ConfiguredVoltage"),
                        ClockSpeed = SafeGet(obj, "ConfiguredClockSpeed"),
                    };

                    _ram.Add(Ram);
                }
                return _ram;
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }

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
                        Manufacturer = SafeGet(obj, "Manufacturer"),
                        Version = SafeGet(obj, "Version"),
                        ReleaseDate = SafeGet(obj, "ReleaseDate"),
                        SerialNumber = SafeGet(obj, "SerialNumber"),
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
            catch (Exception ex) { AnsiConsole.WriteException(ex); }
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
                        Manufacturer = SafeGet(obj, "Manufacturer"),
                        Product = SafeGet(obj, "Product"),
                        SerialNumber = SafeGet(obj, "SerialNumber"),
                        Version = SafeGet(obj, "Version"),
                        Caption = SafeGet(obj, "Caption"),
                    };

                    return motherboard;
                }
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }
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
                    string coreCount = SafeGet(obj, "NumberOfCores");
                    string threadCount = SafeGet(obj, "NumberOfLogicalProcessors");
                    string Virtualization = SafeGet(obj, "VirtualizationFirmwareEnabled");

                    Components.Processor Processor = new()
                    {
                        Name = SafeGet(obj, "Name"),
                        Manufacturer = SafeGet(obj, "Manufacturer"),
                        MaxClockSpeed = SafeGet(obj, "MaxClockSpeed"),
                        SerialNumber = SafeGet(obj, "ProcessorId"),
                        CoreCount = int.Parse(coreCount),
                        ThreadCount = int.Parse(threadCount),
                        SocketDesignation = SafeGet(obj, "SocketDesignation"),
                        CurrentClockSpeed = SafeGet(obj, "CurrentClockSpeed"),
                        Virtualization = bool.Parse(Virtualization),
                        Family = SafeGet(obj, "Family"),
                        DataWidth = SafeGet(obj, "DataWidth"),
                        AddressWidth = SafeGet(obj, "AddressWidth"),
                    };

                    _cpus.Add(Processor);
                }
                return _cpus;
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }

            return null!;
        }

        public static Components.OperatingSystem GetOperatingSystem()
        {
            try
            {
                using var session = CimSession.Create(null);
                // Query to get all details about the operating system
                foreach (var obj in session.QueryInstances("root\\cimv2", "WQL", "SELECT * FROM Win32_OperatingSystem"))
                {
                    Components.OperatingSystem OSInformation = new()
                    {
                        Caption = SafeGet(obj, "Caption"),
                        Version = SafeGet(obj, "Version"),
                        BuildNumber = SafeGet(obj, "BuildNumber"),
                        ProductType = SafeGet(obj, "ProductType"),
                        Manufacturer = SafeGet(obj, "Manufacturer"),
                        Architecture = SafeGet(obj, "OSArchitecture"),

                        LastBoot = SafeGet(obj, "LastBootUpTime"),
                        Username = SafeGet(obj, "RegisteredUser"),
                        SerialNumber = SafeGet(obj, "SerialNumber"),
                    };

                    // Check SecureBootEnabled property
                    var secureBoot = SafeGet(obj, "SecureBootEnabled");
                    if (!string.IsNullOrWhiteSpace(secureBoot))
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
            catch (Exception ex) { AnsiConsole.WriteException(ex); }

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
                        ProductName = SafeGet(obj, "Name"),
                        SystemSerialNumber = SafeGet(obj, "IdentifyingNumber"),
                        Vendor = SafeGet(obj, "Vendor"),
                        SystemVersion = SafeGet(obj, "Version"),
                        UUID = SafeGet(obj, "UUID"),
                    };

                    return SystemInformation;
                }
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }

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

                    ushort[]? manufacturerBytes = SafeGet<ushort[]>(obj, "ManufacturerName");
                    ushort[]? productBytes = SafeGet<ushort[]>(obj, "productBytes");
                    ushort[]? serialBytes = SafeGet<ushort[]>(obj, "SerialNumberID");

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
            catch (Exception ex) { AnsiConsole.WriteException(ex); }

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
                            Name = SafeGet(obj, "Name"),
                            Manufacturer = SafeGet(obj, "Manufacturer"),
                            Status = SafeGet(obj, "Status")
                        };
                        _bluetooth.Add(BluetoothDevice);
                    }
                }
                return _bluetooth;
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }

            return null!;
        }

        public static TrustedPlatFormModule GetTrustedPlatFormModule()
        {
            try
            {
                using var session = CimSession.Create(null);
                var instances = session.QueryInstances("root\\cimv2\\Security\\MicrosoftTpm", "WQL", "SELECT * FROM Win32_Tpm");

                if (instances == null || !instances.Any())
                {
                    return null!;
                }

                foreach (var obj in instances)
                {
                    return new TrustedPlatFormModule
                    {
                        ManufacturerID = SafeGet(obj, "ManufacturerID"),
                        ManufacturerVersion = SafeGet(obj, "ManufacturerVersion"),
                        Version = SafeGet(obj, "SpecVersion"),
                        IsTPMPResent = true,
                        Activated = SafeGet(obj, "IsActivated_InitialValue")
                    };
                }
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }

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
                        Name = SafeGet(obj, "Name"),
                        DeviceID = SafeGet(obj, "PNPDeviceID"),
                        Status = SafeGet(obj, "Status")
                    };

                    if (videoController.Name.Contains("NVIDIA"))
                    {
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
                        catch (Exception ex) { AnsiConsole.WriteException(ex); }
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
            return new string([.. data.Select(c => (char)c)]).TrimEnd('\0');
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
                ProcessStartInfo pro = new("cmd", "/C arp -a")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process? process = Process.Start(pro);
                if (process == null)
                {
                    return null!;
                }

                using var reader = process.StandardOutput;
                string output = reader.ReadToEnd();

                // Regular expression to match IP and MAC addresses
                Regex arpEntryPattern = ArpEntryRegex();

                // Use Regex to find all matches in the output
                MatchCollection matches = arpEntryPattern.Matches(output);

                foreach (Match match in matches)
                {
                    string internetAddress = match.Groups[1].Value;
                    string physicalAddress = match.Groups[2].Value;

                    if (physicalAddress.StartsWith("01-00-5e"))
                        continue;

                    if (physicalAddress == "ff-ff-ff-ff-ff-ff")
                        continue;

                    routerMacs.Add(new ArpTable() { Address = internetAddress, Mac = physicalAddress });
                }

                return routerMacs;
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }

            return null!;
        }

        public static void GetSystemIdentifiers()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    // 1. Get MachineId (from SQMClient)
                    string machineId = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\SQMClient", "MachineId", null)?.ToString() ?? string.Empty;
                    HardwareID.OSInformation.MachineID = machineId;
                }
                catch (Exception ex) { AnsiConsole.WriteException(ex); }

                try
                {
                    // 2. Get MachineGuid (from Cryptography)
                    string machineGuid = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography", "MachineGuid", null)?.ToString() ?? string.Empty;
                    HardwareID.OSInformation.MachineGuid = machineGuid;
                }
                catch (Exception ex) { AnsiConsole.WriteException(ex); }

                try
                {
                    // 3. Get ProductId (from Windows NT CurrentVersion)
                    string productId = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductId", null)?.ToString() ?? string.Empty;
                    HardwareID.OSInformation.ProductID = productId;
                }
                catch (Exception ex) { AnsiConsole.WriteException(ex); }

                try
                {
                    string installDate = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "InstallDate", null)?.ToString() ?? "0";
                    HardwareID.OSInformation.InstallDate = installDate;
                }
                catch (Exception ex) { AnsiConsole.WriteException(ex); }

                try
                {
                    string SusClientId = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate", "SusClientId", null)?.ToString() ?? string.Empty;
                    HardwareID.OSInformation.SusClientId = SusClientId;
                }
                catch (Exception ex) { AnsiConsole.WriteException(ex); }
            }
        }

        private static T? SafeGet<T>(CimInstance obj, string propertyName)
        {
            if (obj.CimInstanceProperties[propertyName] is CimProperty prop && prop.Value is T value)
                return value;

            return default;
        }

        private static string SafeGet(CimInstance obj, string property)
        {
            try
            {
                return obj.CimInstanceProperties[property]?.Value?.ToString() ?? string.Empty;
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }

            return string.Empty;
        }
    }
}
