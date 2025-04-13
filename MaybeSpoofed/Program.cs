using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using MaybeSpoofed.Classes;
using MaybeSpoofed.Functions;
using Newtonsoft.Json;
using Spectre.Console;

namespace MaybeSpoofed
{
    internal class Program
    {
        static void Main()
        {
            Console.SetWindowSize(Console.LargestWindowWidth-30, 40); // Width 120 characters, Height 40 rows

            if (!Directory.Exists("config"))
                Directory.CreateDirectory("config");

            if (!Debugger.IsAttached)
            {
                if (!IsAdministrator())
                {
                    AnsiConsole.Write(
                        new Panel("[bold darkred]Something went wrong[/]\n[red]This application requires Administrator privs to read some data[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] Run as Administrator [/]", Justify.Center)
                            .Padding(1, 0));
                    Console.ReadLine();

                    return;
                }
            }

            Console.Title = "MaybeSpoofed || Discord = @enter_my_username";

            AnsiConsole.Write(
                new Panel("[bold green]Thank you for using MaybeSpoofed[/]")
                    .Border(BoxBorder.Rounded)
                    .Header("[blue]Startup[/]", Justify.Center)
                    .Padding(1, 1)
                    .BorderStyle(new Style(Color.Green)));

            AnsiConsole.MarkupLine("[italic]Our goal is to prevent spoofer issues and support [bold]EAC Rust[/], though it may work for other games.[/]");
            AnsiConsole.MarkupLine("[grey]We are constantly updating to stay ahead of detection.[/]");
            AnsiConsole.MarkupLine("[bold yellow]Discord:[/] [underline]@enter_my_username[/]");
            AnsiConsole.MarkupLine("[bold yellow]Note:[/] Some games use traces to catch ban evasion attempts.");
            AnsiConsole.WriteLine();

            AnsiConsole.Write(new Rule("[cyan]Starting Program[/]").RuleStyle("cyan"));
            AnsiConsole.MarkupLine("[cyan]Initializing modules...[/]");
            Thread.Sleep(500); // small dramatic pause

            Components HardwareID;
            Components SpoofedHardwareID = null!;

            //Components CheckForPermSpoof = Hardware.GetHardwareID();

            if (File.Exists("config/hardware.json"))
            {
                AnsiConsole.MarkupLine("[cyan]Loading pre-existing hardware file...[/]");

                var fileContent = File.ReadAllText("config/hardware.json", Encoding.UTF8);

                var tempSettings = JsonConvert.DeserializeObject<Components>(fileContent);

                if (tempSettings == null)
                {
                    AnsiConsole.Write(
                        new Panel("[bold darkred]Something went wrong[/]\n[red]Please delete the config folder and restart the application[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] ERROR [/]", Justify.Center)
                            .Padding(1, 0));
                    Console.ReadLine();
                    return;
                }

                if (tempSettings.ProgramVersion != "v0.6")
                {
                    AnsiConsole.Write(
                        new Panel("[bold red]Outdated program json[/]\n[red]Please delete the config folder and restart the application[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] ERROR [/]", Justify.Center)
                            .Padding(1, 0));
                    Console.ReadLine();
                    return;
                }

                HardwareID = tempSettings;

                SpoofedHardwareID = Hardware.GetHardwareID();

                File.WriteAllText("config/spoofed.json", JsonConvert.SerializeObject(SpoofedHardwareID, Formatting.Indented), new UTF8Encoding(true));
            }
            else
            {
                AnsiConsole.MarkupLine("[cyan]Generating new hardware file...[/]");

                HardwareID = Hardware.GetHardwareID();

                File.WriteAllText("config/hardware.json", JsonConvert.SerializeObject(HardwareID, Formatting.Indented), new UTF8Encoding(true));
            }

            if (SpoofedHardwareID == null)
            {
                AnsiConsole.Write(
                    new Panel("[bold yellow]Please spoof and restart program[/]\n[yellow]To see if you are spoofed[/]")
                        .Border(BoxBorder.Double)
                        .BorderStyle(new Style(Color.GreenYellow))
                        .Header("[bold yellow] INFO [/]", Justify.Center)
                        .Padding(1, 0));

                Console.ReadLine();
                return;
            }

            //HardwareID.MotherboardInformation.Product = "Concept";
            //HardwareID.MotherboardInformation.SerialNumber = "123456";
            //SpoofedHardwareID.MotherboardInformation.Product = "Concept";
            //SpoofedHardwareID.MotherboardInformation.SerialNumber = "123456";
            //
            //HardwareID.SystemInformation.UUID = "00000000-0000-0000-0000-000000000000";
            //SpoofedHardwareID.SystemInformation.UUID = "00000000-0000-0000-0000-000000000000";
            //
            //HardwareID.SystemInformation.SystemSerialNumber = "123456";
            //SpoofedHardwareID.SystemInformation.SystemSerialNumber = "123456";
            //
            //HardwareID.DiskDrives[0].SerialNumber = "123456";
            //HardwareID.GPUs[0].UUID = "123456";
            //HardwareID.NetworkAdapters[0].Guid = "123456";
            //HardwareID.NetworkAdapters[0].Mac = "ff:ff:ff:ff:ff:ff";
            //HardwareID.Monitors[0].SerialNumber = "123456";
            //HardwareID.Monitors[1].SerialNumber = "123456";
            //HardwareID.Monitors[2].SerialNumber = "123456";
            //SpoofedHardwareID.WindowsFastStartup = true;
            //
            //HardwareID.NearbyDevices[0].Mac = "ff:ff:ff:ff:ff:ff";
            //
            //HardwareID.Partitions[0].SerialNumber = "123456";
            //SpoofedHardwareID.Partitions[0].SerialNumber = "123456";
            //
            //SpoofedHardwareID.DiskDrives[0].SerialNumber = "123456";
            //SpoofedHardwareID.GPUs[0].UUID = "123456";
            //SpoofedHardwareID.NetworkAdapters[0].Guid = "123456";
            //SpoofedHardwareID.NetworkAdapters[0].Mac = "ff:ff:ff:ff:ff:ff";
            //SpoofedHardwareID.Monitors[0].SerialNumber = "123456";
            //SpoofedHardwareID.Monitors[1].SerialNumber = "123456";
            //SpoofedHardwareID.Monitors[2].SerialNumber = "123456";
            //
            //SpoofedHardwareID.NearbyDevices[0].Mac = "ff:ff:ff:ff:ff:ff";
            //
            //HardwareID.OSInformation.MachineGuid = "8a96d405-3b6c-4cb0-adea-746bcf2fbb89";
            //HardwareID.OSInformation.MachineID = "8a96d405-3b6c-4cb0-adea-746bcf2fbb89";
            //HardwareID.OSInformation.InstallDate = "123456";
            //
            //SpoofedHardwareID.OSInformation.MachineGuid = "8a96d405-3b6c-4cb0-adea-746bcf2fbb89";
            //SpoofedHardwareID.OSInformation.MachineID = "8a96d405-3b6c-4cb0-adea-746bcf2fbb89";
            //SpoofedHardwareID.OSInformation.InstallDate = "123456";

            Validation.EAC.Rust.Validate(HardwareID, SpoofedHardwareID);

            AnsiConsole.Write(new Rule("[blue]Motherboard validation[/]").RuleStyle("cyan"));
            Thread.Sleep(500);

            Validation.Validation.Start(HardwareID);

            AnsiConsole.Write(
                new Panel("[bold green]Thank you for using MaybeSpoofed[/]")
                    .Border(BoxBorder.Rounded)
                    .Header("[blue]Done[/]", Justify.Center)
                    .Padding(1, 1)
                    .BorderStyle(new Style(Color.Green)));

            Console.ReadLine();
        }

        static bool IsAdministrator()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            return false;
        }
    }
}