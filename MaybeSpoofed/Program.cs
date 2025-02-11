using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MaybeSpoofed
{
    internal class Program
    {
        public static HardwareID _hwid;
        public static HardwareID _spoofed = null;

        static void Main(string[] args)
        {
            Hardware.Init();

            if(_spoofed == null)
            {
                Custom.WriteLine("Please spoof and restart program to see if you are spoofed", ConsoleColor.Yellow);
            }
            else
            {
                Custom.WriteLine("---------------------------------------", ConsoleColor.Magenta);
                Custom.WriteLine("Hardware result:");
                Custom.WriteLine("---------------------------------------", ConsoleColor.Magenta);

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

                if(_hwid.isTPMPresent || _hwid.isBluetoothPresent || _hwid.isWifiCardPresent)
                {
                    Custom.WriteLine("If you have disabled it in BIOS and keep seeing this error, delete config/hardware.json", ConsoleColor.Green);
                }

                if (_hwid.baseBoardSerialNumber == _spoofed.baseBoardSerialNumber)
                {
                    Custom.WriteLine($"baseBoard serial '{_spoofed.baseBoardSerialNumber}' not spoofed", ConsoleColor.Red);
                }

                if (_hwid.systemUuid == _spoofed.systemUuid)
                {
                    Custom.WriteLine($"systemUuid serial '{_spoofed.systemUuid}' not spoofed", ConsoleColor.Red);
                }

                foreach (var serial in _hwid.ramSerials)
                {
                    if (serial == "00000000")
                        continue;

                    if (_spoofed.ramSerials.Contains(serial))
                    {
                        Custom.WriteLine($"Ram serial '{serial}' not spoofed", ConsoleColor.Red);
                    }
                }

                foreach(var serial in _hwid.diskDriveSerials)
                {
                    if(_spoofed.diskDriveSerials.Contains(serial))
                    {
                        Custom.WriteLine($"Disk drive serial '{serial}' not spoofed", ConsoleColor.Red);
                    }
                }

                //foreach (var serial in _hwid.videoController)
                //{
                //    var index = _spoofed.videoController.FindIndex(m => m.PNPDeviceID == serial.PNPDeviceID);
                //
                //    if(index > -1)
                //    {
                //        Custom.WriteLine($"GPU serial '{serial.PNPDeviceID}' not spoofed", ConsoleColor.Red);
                //    }
                //}

                foreach(var serial in _hwid.macAddresses)
                {
                    if(_spoofed.macAddresses.Contains(serial))
                    {
                        Custom.WriteLine($"Mac serial '{serial}' not spoofed", ConsoleColor.Red);
                    }
                }

                foreach(var serial in _hwid.monitorSerials)
                {
                    if (serial.Length <= 1)
                        continue;

                    if (_spoofed.monitorSerials.Contains(serial))
                    {
                        Custom.WriteLine($"Monitor serial '{serial}' not spoofed", ConsoleColor.Red);
                    }
                }

            }

            Console.ReadLine();
        }
    }
}
