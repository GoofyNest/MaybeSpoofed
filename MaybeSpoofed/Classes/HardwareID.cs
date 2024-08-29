using System.Collections.Generic;

namespace MaybeSpoofed
{
    public class HardwareID
    {
        public string baseBoardSerialNumber { get; set; }
        public string systemUuid { get; set; }

        public string processorID { get; set; } // Useless

        public List<string> ramSerials { get; set; } = new List<string>();

        public List<string> diskDriveSerials { get; set; } = new List<string>();

        public GraphicCard videoController { get; set; } = new GraphicCard();

        public List<string> macAddresses { get; set; } = new List<string>();

        public List<string> monitorSerials { get; set; } = new List<string>();

        public bool isTPMPresent { get; set; }

        public bool isBluetoothPresent { get; set; }

        public bool isWifiCardPresent { get; set; }
     }

    public class GraphicCard
    {
        public string Description { get; set; }
        public string PNPDeviceID { get; set; }
    }
}
