namespace MaybeSpoofed.Classes
{
    public class Components
    {
        public string ProgramVersion { get; set; } = "v0.2";
        public System SystemInformation { get; set; } = null!;

        public OperatingSystem OSInformation { get; set; } = null!;

        public Bios BIOS { get; set; } = null!;

        public Motherboard MotherboardInformation { get; set; } = null!;

        public List<Processor> CPUs { get; set; } = [];

        public TrustedPlatFormModule TPM { get; set; } = null!;

        public List<ArpTable> NearbyDevices { get; set; } = null!;

        public List<NetworkAdapter> NetworkAdapters { get; set; } = [];

        public List<VideoController> GPUs { get; set; } = [];

        public List<Storage> DiskDrives { get; set; } = [];

        public List<PhysicalMemory> Ram { get; set; } = [];

        public List<Monitor> Monitors { get; set; } = [];

        public List<BluetoothDevice> BluetoothDevices { get; set; } = [];

        public bool WindowsFastStartup { get; set; } = false;

        public class ArpTable()
        {
            public string Address { get; set; } = string.Empty;
            public string Mac { get; set; } = string.Empty;
        }

        public class System()
        {
            public string ProductName { get; set; } = string.Empty;
            public string SystemSerialNumber { get; set; } = string.Empty;
            public string Vendor { get; set; } = string.Empty;
            public string SystemVersion { get; set; } = string.Empty;
            public string UUID { get; set; } = string.Empty;
        }

        public class OperatingSystem()
        {
            public string Caption { get; set; } = string.Empty; // Microsoft Windows 10/11
            public string Version { get; set; } = string.Empty; // 10.0.19044 Build 19044
            public string BuildNumber { get; set; } = string.Empty;
            public string ProductType { get; set; } = string.Empty;
            public string Manufacturer { get; set; } = string.Empty;
            public string Architecture { get; set; } = string.Empty;
            public string LastBoot { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string SerialNumber { get; set; } = string.Empty;
            public bool SecureBoot { get; set; } = false;
        }

        public class BluetoothDevice()
        {
            public string Name { get; set; } = string.Empty;
            public string Manufacturer { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }

        public class TrustedPlatFormModule()
        {
            public string ManufacturerID { get; set; } = string.Empty;
            public string ManufacturerVersion { get; set; } = string.Empty;
            public string Version { get; set; } = string.Empty;
            public string VersionInfo { get; set; } = string.Empty;
            public string Manufacturer { get; set; } = string.Empty;
            public string Activated { get; set; } = string.Empty;
            public bool IsTPMPResent { get; set; } = false;
        }

        public class VideoController()
        {
            public string Name { get; set; } = string.Empty;
            public string DeviceID { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string SerialNumber { get; set; } = string.Empty;
            public string UUID { get; set; } = string.Empty;
            public string Version { get; set; } = string.Empty;
        }

        public class Monitor()
        {
            public string Manufacturer { get; set; } = string.Empty;
            public string SerialNumber { get; set; } = string.Empty;
        }

        public class Motherboard()
        {
            public string Manufacturer { get; set; } = string.Empty; // ASUSTeK
            public string Product { get; set; } = string.Empty; // PRIME
            public string SerialNumber { get; set; } = string.Empty;
            public string Version { get; set; } = string.Empty; // Rev 1.xx
            public string Caption { get; set; } = string.Empty; // Base Board
        }

        public class Bios()
        {
            public string Manufacturer { get; set; } = string.Empty;
            public string Version { get; set; } = string.Empty;
            public string ReleaseDate { get; set; } = string.Empty;
            public string SerialNumber { get; set; } = string.Empty;
            public string Mode { get; set; } = string.Empty; // UEFI etc
        }

        public class PhysicalMemory()
        {
            public string Location { get; set; } = string.Empty;
            public string Capacity { get; set; } = string.Empty;
            public string Manufacturer { get; set; } = string.Empty;
            public string Speed { get; set; } = string.Empty;
            public string SerialNumber { get; set; } = string.Empty;
            public string PartNumber { get; set; } = string.Empty;
            public string Voltage { get; set; } = string.Empty;
            public string ClockSpeed { get; set; } = string.Empty;
        }

        public class Storage()
        {
            public string DeviceID { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public string SerialNumber { get; set; } = string.Empty;
            public string Size { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public int Partitions { get; set; } = 0;
            public string Manufacturer { get; set; } = string.Empty;
            public int BytesPerSector { get; set; } = 0;
            public string FirmwareRevision { get; set; } = string.Empty;
        }

        public class NetworkAdapter()
        {
            public string Name { get; set; } = string.Empty;
            public string Mac { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string Manufacturer { get; set; } = string.Empty;
            public string Guid { get; set; } = string.Empty;
        }

        public class IPConfig()
        {
            public string RouterMac { get; set; } = string.Empty;
            public string DNSHostName { get; set; } = string.Empty;
        }

        public class Processor()
        {
            public string Name { get; set; } = string.Empty;
            public string Manufacturer { get; set; } = string.Empty;
            public string MaxClockSpeed { get; set; } = string.Empty;
            public string SerialNumber { get; set; } = string.Empty;
            public int CoreCount { get; set; } = 0;
            public int ThreadCount { get; set; } = 0;
            public string SocketDesignation { get; set; } = string.Empty;
            public string CurrentClockSpeed { get; set; } = string.Empty;
            public bool Virtualization { get; set; } = false;
            public string Family { get; set; } = string.Empty;
            public string DataWidth { get; set; } = string.Empty;
            public string AddressWidth { get; set; } = string.Empty;
        }
    }
}
