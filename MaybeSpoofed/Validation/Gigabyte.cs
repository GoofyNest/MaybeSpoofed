using MaybeSpoofed.Classes;
using MaybeSpoofed.Helpers;

namespace MaybeSpoofed.Validation
{
    public class Gigabyte
    {
        public static void SystemInformation(Components.System system)
        {
            if (system == null)
            {
                Custom.WriteLine("Could not validate system information cause its null", ConsoleColor.Cyan);
                return;
            }

            if (!system.SystemSerialNumber.Equals("default string", StringComparison.CurrentCultureIgnoreCase))
            {
                Custom.WriteLine("SystemSerialNumber is perm spoofed", ConsoleColor.DarkRed);
            }

            if (!system.SystemVersion.Equals("default string", StringComparison.CurrentCultureIgnoreCase))
            {
                Custom.WriteLine("SystemVersion is perm spoofed", ConsoleColor.DarkRed);
            }

            if (system.ProductName.Contains("(MS-"))
            {
                Custom.WriteLine("ProductName is perm spoofed", ConsoleColor.DarkRed);
            }
        }

        public static void MotherboardInformation(Components.Motherboard motherboard)
        {
            if (motherboard == null)
            {
                Custom.WriteLine("Could not validate motherboard information cause its null", ConsoleColor.Cyan);
                return;
            }

            if (!motherboard.SerialNumber.Equals("default string", StringComparison.CurrentCultureIgnoreCase))
            {
                Custom.WriteLine($"[SerialNumber] is perm spoofed", ConsoleColor.Red);
            }

            if (!motherboard.Version.Equals("x.x", StringComparison.CurrentCultureIgnoreCase))
            {
                Custom.WriteLine($"[Version] is perm spoofed", ConsoleColor.Red);
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
