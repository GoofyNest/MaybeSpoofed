using System;
using System.Collections.Generic;
using System.Linq;
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
                    Custom.WriteLine("baseBoard not spoofed", ConsoleColor.Red);
                }

                if (_hwid.systemUuid == _spoofed.systemUuid)
                {
                    Custom.WriteLine("systemUuid not spoofed", ConsoleColor.Red);
                }

                if (_hwid.processorID == _spoofed.processorID)
                {
                    Custom.WriteLine("processorID not spoofed", ConsoleColor.Green);
                }

                // Ram serials
                for (var i = 0; i < _hwid.ramSerials.Count; i++)
                {
                    try
                    {
                        var ram = _hwid.ramSerials[i];
                        var spoofedRam = _spoofed.ramSerials[i];

                        if (ram == spoofedRam)
                        {
                            Custom.WriteLine($"ramSerials #{i} not spoofed", ConsoleColor.Red);
                        }
                    }
                    catch (Exception ex)
                    {
                        Custom.WriteLine("Maybe normal", ConsoleColor.Yellow);
                        Custom.WriteLine(ex.ToString(), ConsoleColor.Red);
                    }
                }

                // Diskdrive Serials
                for (var i = 0; i < _hwid.diskDriveSerials.Count; i++)
                {
                    try
                    {
                        var disk = _hwid.diskDriveSerials[i];
                        var spoofedDisk = _spoofed.diskDriveSerials[i];

                        if (disk == spoofedDisk)
                        {
                            Custom.WriteLine($"diskDriveSerials #{i} not spoofed", ConsoleColor.Red);
                        }
                    }
                    catch(Exception ex)
                    {
                        Custom.WriteLine("Maybe normal", ConsoleColor.Yellow);
                        Custom.WriteLine(ex.ToString(), ConsoleColor.Red);
                    }
                }

                var card1 = _hwid.videoController;
                var card2 = _spoofed.videoController;

                if(card1.PNPDeviceID == card2.PNPDeviceID)
                {
                    Custom.WriteLine($"videoController not spoofed", ConsoleColor.Yellow);
                }

                // Mac addresses
                for (var i = 0; i < _hwid.macAddresses.Count; i++)
                {
                    try
                    {
                        var mac = _hwid.macAddresses[i];
                        var spoofedMac = _spoofed.macAddresses[i];

                        if (mac == spoofedMac)
                        {
                            Custom.WriteLine($"macAddresses #{i} not spoofed", ConsoleColor.Red);
                        }
                    }
                    catch (Exception ex)
                    {
                        Custom.WriteLine("Maybe normal", ConsoleColor.Yellow);
                        Custom.WriteLine(ex.ToString(), ConsoleColor.Red);
                    }
                }



            }

            Console.ReadLine();
        }
    }
}
