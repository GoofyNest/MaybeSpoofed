using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaybeSpoofed
{
    public class Hardware
    {
        public static void Init()
        {
            if(!Directory.Exists("config"))
                Directory.CreateDirectory("config");

            if(File.Exists("config/hardware.json"))
            {
                Custom.WriteLine("Loading existing hardware file", ConsoleColor.Green);

                Program._hwid = JsonConvert.DeserializeObject<HardwareID>(File.ReadAllText("config/hardware.json"));

                Custom.WriteLine("Grabbing spoofed serials", ConsoleColor.Cyan);

                Program._spoofed = GetHardwareID();

                File.WriteAllText("config/spoofed.json", JsonConvert.SerializeObject(Program._spoofed, Formatting.Indented));
            }
            else
            {
                Custom.WriteLine("Generating new hardware file", ConsoleColor.Cyan);

                Program._hwid = GetHardwareID();

                File.WriteAllText("config/hardware.json", JsonConvert.SerializeObject(Program._hwid, Formatting.Indented));
            }
        }

        public static HardwareID GetHardwareID()
        {
            HardwareID _hwid = new HardwareID();

            // Get the serial number of the baseboard
            List<string> baseboardSerialNumber = WMI.GetProperty("Win32_BaseBoard", "SerialNumber");
            _hwid.baseBoardSerialNumber = baseboardSerialNumber[0];

            // Get the UUID of the computer system product
            List<string> systemUuid = WMI.GetProperty("Win32_ComputerSystemProduct", "UUID");
            _hwid.systemUuid = systemUuid[0];

            // Get first Processor IDs
            List<string[]> processorIds = WMI.GetProperties("Win32_Processor", "ProcessorId");
            _hwid.processorID = processorIds.First()[0];

            // Get all Memory Chip Serial Numbers
            List<string[]> memoryChipSerialNumbers = WMI.GetProperties("Win32_PhysicalMemory", "SerialNumber");
            foreach (var ram in memoryChipSerialNumbers)
            {
                _hwid.ramSerials.Add(ram[0]);
            }

            // Get all Disk Drive Serial Numbers
            List<string[]> diskDriveSerialNumbers = WMI.GetProperties("Win32_DiskDrive", "SerialNumber");
            foreach (var drive in diskDriveSerialNumbers)
            {
                _hwid.diskDriveSerials.Add(drive[0]);
            }

            // Get all Video Controller unique identifiers
            List<string[]> videoControllers = WMI.GetProperties(
                "Win32_VideoController",
                "Description",
                "PNPDeviceID"
            );

            foreach (var controller in videoControllers)
            {
                GraphicCard card = new GraphicCard();

                card.Description = controller[0];
                card.PNPDeviceID = controller[1];

                _hwid.videoController = card;

                break;
            }

            // Get MAC Addresses of all active PCI network adapters
            List<string> macAddresses = WMI.GetPropertyWithCondition(
                "Win32_NetworkAdapter",
                "MACAddress",
                "PNPDeviceID LIKE '%PCI%' AND NetConnectionStatus=2 AND AdapterTypeID=0"
            );

            foreach (var mac in macAddresses)
                _hwid.macAddresses.Add(mac);

            // Get Monitor Serial Numbers
            string script = @"
            Get-WmiObject -Namespace root\wmi -Class WmiMonitorID | ForEach-Object {
                $serialBytes = $_.SerialNumberID
                $serialString = [System.Text.Encoding]::ASCII.GetString($serialBytes)
                $serialString = $serialString.TrimEnd([char]0)
                $serialString
            }
        ";

            var output = WMI.ExecutePowerShellScript(script);

            foreach (var line in output)
            {
                _hwid.monitorSerials.Add(line);
            }

            // Check if TPM module is present
            script = @"
            $tpm = Get-CimInstance -Namespace 'Root\CIMv2\Security\MicrosoftTpm' -ClassName Win32_Tpm
            if ($tpm.IsActivated_Initially -eq $true -and $tpm.IsEnabled_Initially -eq $true) {
                'enabled'
            } else {
                'disabled'
            }
        ";

            // Execute the PowerShell script and get the result
            output = WMI.ExecutePowerShellScript(script);
            foreach (var line in output)
            {
                if (line == "enabled")
                {
                    _hwid.isTPMPresent = true;
                }
                else if(line == "disabled")
                {
                    _hwid.isTPMPresent = false;
                }

                break;
            }

            // Check if Bluetooth card is present
            script = @"
            $devices = Get-CimInstance -ClassName Win32_PnPSignedDriver -Filter 'DeviceName LIKE ''%Bluetooth%'''
            if ($devices) {
                $devices | ForEach-Object { 'Bluetooth device found: ' + $_.DeviceName }
            } else {
                'No Bluetooth device found.'
            }
        ";

            output = WMI.ExecutePowerShellScript(script);

            // Print the result
            foreach (var line in output)
            {
                if (line == "No Bluetooth device found.")
                {
                    _hwid.isBluetoothPresent = false;
                }
                else
                    _hwid.isBluetoothPresent = true;
            }

            // Check if WIFI card is present
            script = @"
            $adapters = Get-CimInstance -ClassName Win32_NetworkAdapter
            $wifiAdapters = $adapters | Where-Object { $_.Description -like '*Wi-Fi*' }
            if ($wifiAdapters) {
                $wifiAdapters | ForEach-Object { 'Wi-Fi device found: ' + $_.Description }
            } else {
                'No Wi-Fi device found.'
            }
        ";

            output = WMI.ExecutePowerShellScript(script);

            // Print the result
            foreach (var line in output)
            {
                if (line == "No Wi-Fi device found.")
                {
                    _hwid.isWifiCardPresent = false;
                }
                else
                    _hwid.isWifiCardPresent = true;
            }

            if (_hwid.isTPMPresent)
            {
                Custom.WriteLine("Trusted platform module(TPM) is present, should disable in BIOS", ConsoleColor.Red);
            }

            if (_hwid.isBluetoothPresent)
            {
                Custom.WriteLine("Bluetooth card is present, should disable in BIOS", ConsoleColor.Red);
            }

            if (_hwid.isWifiCardPresent)
            {
                Custom.WriteLine("Wifi card is present, should disable in BIOS", ConsoleColor.Red);
            }

            Custom.WriteLine("Completed task", ConsoleColor.DarkGreen);

            return _hwid;
        }
    }
}
