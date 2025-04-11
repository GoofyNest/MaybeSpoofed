using MaybeSpoofed.Classes;
using MaybeSpoofed.Functions;
using MaybeSpoofed.Helpers;

namespace MaybeSpoofed.Validation
{
    public class Validation
    {
        public static void Start(Components hwid)
        {
            if(hwid == null)
            {
                Custom.WriteLine($"Could not validate, HWID is null", ConsoleColor.Cyan);
                return;
            }

            if(hwid.MotherboardInformation != null)
            {
                var motherboard = hwid.MotherboardInformation;

                switch(motherboard.Manufacturer.ToLower())
                {
                    case "asrock":
                        {
                            ASRock.MotherboardInformation(motherboard);
                            ASRock.SystemInformation(hwid.SystemInformation);
                            ASRock.BIOS(hwid.BIOS);
                        }
                        break;

                    case "asus":
                        {
                            ASUS.MotherboardInformation(motherboard);
                            ASUS.SystemInformation(hwid.SystemInformation);
                        }
                        break;

                    case "msi":
                        {
                            MSI.MotherboardInformation(motherboard);
                            MSI.SystemInformation(hwid.SystemInformation);
                        }
                        break;

                    case "gigabyte":
                        {
                            Gigabyte.MotherboardInformation(motherboard);
                            Gigabyte.SystemInformation(hwid.SystemInformation);
                        }
                        break;

                    default:
                        {
                            Custom.WriteLine("Have not yet dumped your motherboard, please do contact me on Discord to contribute", ConsoleColor.Cyan);
                        }
                        break;
                }
            }

            Custom.WriteLine($"Please contact me on Discord if you want help: @enter_my_username", ConsoleColor.Cyan);

        }

        public static bool IsTailAllZeros(string guid)
        {
            // Expected format: XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX
            string[] parts = guid.Split('-');

            if (parts.Length != 5)
                return false;

            // Only check the last 3 parts
            return parts[2] == "0000" &&
                   parts[3] == "0000" &&
                   parts[4] == "000000000000";
        }
    }
}
