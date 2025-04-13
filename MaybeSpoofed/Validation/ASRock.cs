using MaybeSpoofed.Classes;
using MaybeSpoofed.Helpers;

namespace MaybeSpoofed.Validation
{
    public class ASRock
    {
        public static void SystemInformation(Components.System system)
        {
            if (system == null)
            {
                Custom.WriteLine("Could not validate system information cause its null", ConsoleColor.Cyan);
                return;
            }

            if(system.SystemSerialNumber != "To Be Filled By O.E.M.")
            {
                Custom.WriteLine($"[SystemSerialNumber] is perm spoofed", ConsoleColor.DarkRed);
            }

            if(system.SystemVersion != "To Be Filled By O.E.M.")
            {
                Custom.WriteLine($"[SystemVersion] is perm spoofed", ConsoleColor.DarkRed);
            }

            if(!Validation.IsTailAllZeros(system.UUID))
            {
                Custom.WriteLine($"[UUID] is perm spoofed", ConsoleColor.DarkRed);
            }
        }

        public static void MotherboardInformation(Components.Motherboard motherboard)
        {
            if (motherboard == null)
            {
                Custom.WriteLine("Could not validate motherboard information cause its null", ConsoleColor.Cyan);
                return;
            }

            if(!motherboard.SerialNumber.StartsWith("BR80"))
            {
                Custom.WriteLine($"[SerialNumber] might be perm spoofed", ConsoleColor.DarkRed);
            }

            if (motherboard.Version != "                      ")
            {
                Custom.WriteLine($"[Version] might be perm spoofed", ConsoleColor.DarkRed);
            }

            if(motherboard.Product.Contains("(MS-"))
            {
                Custom.WriteLine($"[Product] is perm spoofed", ConsoleColor.DarkRed);
            }
        }

        public static void BIOS(Components.Bios bios)
        {
            if(bios == null)
            {
                Custom.WriteLine("Could not validate bios information cause its null", ConsoleColor.Cyan);
                return;
            }

            if(bios.SerialNumber != "To Be Filled By O.E.M.")
            {
                Custom.WriteLine($"[SerialNumber] is perm spoofed", ConsoleColor.DarkRed);
            }
        }
    }
}
