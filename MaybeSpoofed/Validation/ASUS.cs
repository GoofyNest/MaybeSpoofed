using MaybeSpoofed.Classes;
using MaybeSpoofed.Helpers;

namespace MaybeSpoofed.Validation
{
    public class ASUS
    {
        public static void SystemInformation(Components.System system)
        {
            if (system == null)
            {
                Custom.WriteLine("Could not validate system information cause its null", ConsoleColor.Cyan);
                return;
            }

            if(system.ProductName != "System Product Name")
            {
                Custom.WriteLine($"[ProductName] is perm spoofed", ConsoleColor.Red);
            }

            if (system.SystemVersion != "System Version")
            {
                Custom.WriteLine($"[SystemVersion] is perm spoofed", ConsoleColor.Red);
            }

            if (system.SystemSerialNumber != "System Serial Number")
            {
                Custom.WriteLine($"[SystemSerialNumber] is perm spoofed", ConsoleColor.Red);
            }
        }

        public static void MotherboardInformation(Components.Motherboard motherboard)
        {
            if (motherboard == null)
            {
                Custom.WriteLine("Could not validate motherboard information cause its null", ConsoleColor.Cyan);
                return;
            }

            if (!motherboard.SerialNumber.StartsWith("190"))
            {
                Custom.WriteLine($"[SerialNumber] might be perm spoofed", ConsoleColor.DarkYellow);
            }

            if(!motherboard.Version.Equals("rev 1.xx", StringComparison.InvariantCultureIgnoreCase))
            {
                Custom.WriteLine($"[Version] might be perm spoofed", ConsoleColor.DarkYellow);
            }

            if (motherboard.Product.Contains("(MS-"))
            {
                Custom.WriteLine($"[Product] is perm spoofed", ConsoleColor.Red);
            }
        }

        public static void BIOS(Components.Bios bios)
        {

        }
    }
}
