using MaybeSpoofed.Classes;
using Spectre.Console;

namespace MaybeSpoofed.Validation.EAC
{
    public class Rust
    {
        public static Table ErrorTable { get; set; } = new();
        public static Table WarningTable { get; set; } = new();


        public static void AddError(string origin = "", string reference = "", string identifier = "", string status = "", string note = "")
        {
            ErrorTable.AddRow(origin, reference, identifier, status, note);
        }

        public static void AddWarning(string origin = "", string reference = "", string identifier = "", string status = "", string note = "")
        {
            WarningTable.AddRow(origin, reference, identifier, status, note);
        }

        public static void Validate(Components orig, Components spoof)
        {
            AnsiConsole.Write(new Rule("[red]Hardware result[/]").RuleStyle("red"));
            Thread.Sleep(500);

            ErrorTable.AddColumn("Origin");
            ErrorTable.AddColumn("Reference");
            ErrorTable.AddColumn("Identifier");
            ErrorTable.AddColumn("Status");
            ErrorTable.AddColumn("Note");

            WarningTable.AddColumn("Origin");
            WarningTable.AddColumn("Reference");
            WarningTable.AddColumn("Identifier");
            WarningTable.AddColumn("Status");
            WarningTable.AddColumn("Note");

            if (orig == null)
            {
                AnsiConsole.Write(
                    new Panel("[bold darkred]config/hardware.json[/]\n[red]Seems to be corrupted[/]")
                        .Border(BoxBorder.Double)
                        .BorderStyle(new Style(Color.Red))
                        .Header("[white on red] ERROR [/]", Justify.Center)
                        .Padding(1, 0));
                return;
            }

            if(spoof == null)
            {
                AnsiConsole.MarkupLine("[bold darkred]Could not confirm if spoofed cause everything is null[/]");
                return;
            }

            if (TPMEnabled(spoof))
            {
                AddError("TPM", "Trusted Platform Module", "", "[red]FAIL[/]", "Disable it in BIOS, we do not validate this");
            }

            if(BluetoothFound(spoof))
            {
                AddError("BT", "Bluetooth devices", "", "[red]FAIL[/]", "Disable in BIOS, we do not validate this");
            }

            // Check if BaseBoard is properly spoofed
            CheckBaseBoardSerials(orig, spoof);

            // Check if SystemInformation is properly spoofed
            CheckSystemInformation(orig, spoof);

            // Check if Ram serials is properly spoofed
            CheckRamSerials(orig, spoof);

            // Check if Disk serials is properly spoofed
            CheckDiskSerials(orig, spoof);

            // Check if Network adapters is spoofed
            CheckNetworkAdapters(orig, spoof);

            // Check if Monitors is spoofed
            CheckMonitorSerials(orig, spoof);

            // Check if operating system is spoofed
            CheckOperatingSystem(orig, spoof);

            AnsiConsole.Write(ErrorTable);

            AnsiConsole.Write(new Rule("[yellow]Hardware result Misc[/]").RuleStyle("yellow"));

            // Check for misc stuff that could lead to issues
            CheckForWarnings(orig, spoof);

            AnsiConsole.Write(WarningTable);
        }

        private static void CheckOperatingSystem(Components orig, Components spoof, int mode = 0)
        {
            try
            {
                var original = orig.OSInformation;
                var spoofed = spoof.OSInformation;

                if (original == null)
                {
                    AnsiConsole.MarkupLine("[bold darkred]config/Hardware.json seems to be corrupted[/]");
                    return;
                }

                if (spoofed == null)
                {
                    AnsiConsole.MarkupLine("[bold darkred]Could not validate Operating system[/]");
                    return;
                }

                if(mode == 0)
                {
                    if (original.MachineGuid == spoofed.MachineGuid)
                    {
                        AddError("Windows", "MachineGuid", $"[red]{original.MachineGuid}[/]", $"[red]FAIL[/]");
                    }
                    else
                        AddError("Windows", "MachineGuid", $"[green]{spoofed.MachineGuid}[/]", $"[green]OK[/]");

                    if (original.ProductID == spoofed.ProductID)
                    {
                        AddError("Windows", "ProductID", $"[red]{original.ProductID}[/]", $"[red]FAIL[/]");
                    }
                    else
                        AddError("Windows", "ProductID", $"[green]{spoofed.ProductID}[/]", $"[green]OK[/]");

                    if (original.InstallDate == spoofed.InstallDate)
                    {
                        AddError("Windows", "InstallDate", $"[red]{original.InstallDate}[/]", $"[red]FAIL[/]");
                    }
                    else
                        AddError("Windows", "InstallDate", $"[green]{spoofed.InstallDate}[/]", $"[green]OK[/]");

                    if (original.SusClientId == spoofed.SusClientId)
                    {
                        AddError("Windows", "SusClientId", $"[red]{original.SusClientId}[/]", $"[red]FAIL[/]");
                    }
                    else
                        AddError("Windows", "SusClientId", $"[green]{spoofed.SusClientId}[/]", $"[green]OK[/]");

                    return;
                }

                if (original.MachineID == spoofed.MachineID)
                    AddWarning("Windows", "MachineID", $"[yellow]{original.MachineID}[/]", "[yellow]WARN[/]");

                if (spoofed.Username.Contains('@'))
                    AddWarning("Windows", "Username", $"[yellow]{original.Username}[/]", "[yellow]WARN[/]", "Recommend offline account");

                if (spoof.BIOS != null)
                {
                    if (!spoof.BIOS.ReleaseDate.Contains("2024") && !spoof.BIOS.ReleaseDate.Contains("2025"))
                        AddWarning("Bios", "ReleaseDate", $"[yellow]{orig.BIOS.ReleaseDate}[/]", "[yellow]WARN[/]", "Recommend updating");
                }
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }
        }

        private static void CheckNearbyDevices(Components orig, Components spoof)
        {
            try
            {
                var original = orig.NearbyDevices;
                var spoofed = spoof.NearbyDevices;

                if (original == null)
                {
                    return;
                }

                if (original.Count == 0)
                {
                    return;
                }

                if (spoofed == null)
                {
                    return;
                }

                if (spoofed.Count == 0)
                {
                    return;
                }

                foreach(var arp in original)
                {
                    if (string.IsNullOrEmpty(arp.Mac))
                        continue;

                    if (arp.Mac.Length < 3)
                        continue;

                    if (spoofed.FindAll(m => m.Mac == arp.Mac).Count > 0)
                        AddWarning("ARP", "MAC", "[yellow]"+arp.Mac.Replace("-", ":")+ "[/]", "[yellow]WARN[/]");
                    else
                        AddWarning("ARP", "MAC", "[green]" + arp.Mac.Replace("-", ":")+"[/]", "[green]OK[/]");
                }

            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }
        }

        private static void CheckPartitionSerials(Components orig, Components spoof)
        {
            try
            {
                var original = orig.Partitions;
                var spoofed = spoof.Partitions;

                if (original == null)
                {
                    AnsiConsole.Write(
                        new Panel("[bold darkred]config/hardware.json[/]\n[red]Seems to be corrupted[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] Partitions [/]", Justify.Center)
                            .Padding(1, 0));
                    return;
                }

                if (original.Count == 0)
                {
                    AnsiConsole.Write(
                        new Panel("[bold darkred]config/hardware.json[/]\n[red]Seems to be corrupted[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] Partitions [/]", Justify.Center)
                            .Padding(1, 0));
                    return;
                }

                if (spoofed == null)
                {
                    AnsiConsole.MarkupLine("[bold darkred]config/Hardware.json seems to be corrupted[/]");
                    return;
                }

                if (spoofed.Count == 0)
                {
                    AnsiConsole.MarkupLine("[bold darkred]Could not validate Partition data[/]");
                    return;
                }

                foreach(var partition in original)
                {
                    var serial = partition.SerialNumber;

                    if (spoofed.FindAll(m => m.SerialNumber == serial).Count > 0)
                    {
                        AddWarning("Partition", partition.DeviceID, $"[yellow]{serial}[/]", "[yellow]WARN[/]");
                    }
                    else
                        AddWarning("Partition", partition.DeviceID, $"[green]{serial}[/]", "[green]OK[/]");
                }
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }
        }

        private static void CheckMonitorSerials(Components orig, Components spoof)
        {
            try
            {
                var original = orig.Monitors;
                var spoofed = spoof.Monitors;

                if (original == null)
                {
                    AnsiConsole.Write(
                        new Panel("[bold darkred]config/hardware.json[/]\n[red]Seems to be corrupted[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] Monitors [/]", Justify.Center)
                            .Padding(1, 0));
                    return;
                }

                if (original.Count == 0)
                {
                    AnsiConsole.Write(
                        new Panel("[bold darkred]config/hardware.json[/]\n[red]Seems to be corrupted[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] Monitors [/]", Justify.Center)
                            .Padding(1, 0));
                    return;
                }

                if (spoofed == null)
                {
                    AnsiConsole.MarkupLine("[bold darkred]Could not validate Monitor data[/]");
                    return;
                }

                if (spoofed.Count == 0)
                {
                    AnsiConsole.MarkupLine("[bold darkred]Could not validate Monitor data[/]");
                    return;
                }

                foreach(var monitor in original)
                {
                    var serial = monitor.SerialNumber;

                    if (serial.Length <= 1)
                    {
                        AddError($"Monitor", monitor.Manufacturer, $"[green]{serial}[/]", $"[green]OK[/]");
                        continue;
                    }

                    if (spoofed.FindAll(m => m.SerialNumber == serial).Count > 0)
                    {
                        AddError($"Monitor", monitor.Manufacturer, $"[red]{serial}[/]", $"[red]FAIL[/]");
                        //Custom.WriteLine($"[Monitor] {monitor.Manufacturer} => {serial}", ConsoleColor.Red);
                    }
                    else
                        AddError($"Monitor", monitor.Manufacturer, $"[green]{serial}[/]", $"[green]OK[/]");
                }
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }
        }

        private static void CheckNetworkAdapters(Components orig, Components spoof, int mode = 0)
        {
            try
            {
                var original = orig.NetworkAdapters;
                var spoofed = spoof.NetworkAdapters;

                if (original == null)
                {
                    AnsiConsole.Write(
                        new Panel("[bold darkred]config/hardware.json[/]\n[red]Seems to be corrupted[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] NetworkAdapters [/]", Justify.Center)
                            .Padding(1, 0));
                    return;
                }

                if (original.Count == 0)
                {
                    AnsiConsole.Write(
                        new Panel("[bold darkred]config/hardware.json[/]\n[red]Seems to be corrupted[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] NetworkAdapters [/]", Justify.Center)
                            .Padding(1, 0));
                    return;
                }

                if (spoofed == null)
                {
                    AnsiConsole.MarkupLine("[bold darkred]Could not validate NetworkAdapters[/]");
                    return;
                }

                if (spoofed.Count == 0)
                {
                    AnsiConsole.MarkupLine("[bold darkred]Could not validate NetworkAdapters[/]");
                    return;
                }

                foreach(var adapter in original)
                {
                    var mac = adapter.Mac;

                    if (adapter.Name.StartsWith("WAN Miniport"))
                        continue;

                    if (adapter.Name.Contains("vpn", StringComparison.CurrentCultureIgnoreCase))
                        continue;

                    if (mode == 0)
                    {
                        if (spoofed.FindAll(m => m.Mac == mac).Count > 0)
                        {
                            AddError($"Network", adapter.Name, $"[red]{mac}[/]", $"[red]FAIL[/]");
                        }
                        else
                            AddError($"Network", adapter.Name, $"[green]{mac}[/]", $"[green]OK[/]");

                        continue;
                    }

                    if (spoofed.FindAll(m => m.Guid == adapter.Guid).Count > 0)
                    {
                        AddWarning($"Network", adapter.Name, $"[yellow]{adapter.Guid}[/]", "[yellow]WARN[/]");
                    }
                    else
                        AddWarning($"Network", adapter.Name, $"[green]{adapter.Guid}[/]", "[green]OK[/]");
                }

            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }
        }

        private static void CheckGPUSerial(Components orig, Components spoof)
        {
            try
            {
                var original = orig.GPUs;
                var spoofed = spoof.GPUs;

                if (original == null)
                {
                    AnsiConsole.Write(
                        new Panel("[bold darkred]config/hardware.json[/]\n[red]Seems to be corrupted[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] VideoControllers [/]", Justify.Center)
                            .Padding(1, 0));
                    return;
                }

                if (original.Count == 0)
                {
                    AnsiConsole.Write(
                        new Panel("[bold darkred]config/hardware.json[/]\n[red]Seems to be corrupted[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] VideoControllers [/]", Justify.Center)
                            .Padding(1, 0));
                    return;
                }

                if (spoofed == null)
                {
                    AnsiConsole.MarkupLine("[bold darkred]Could not validate VideoControllers[/]");
                    return;
                }

                if (spoofed.Count == 0)
                {
                    AnsiConsole.MarkupLine("[bold darkred]Could not validate VideoControllers[/]");
                    return;
                }

                foreach(var gpu in original)
                {
                    var serial = gpu.SerialNumber;
                    var UUID = gpu.UUID;

                    if (string.IsNullOrWhiteSpace(UUID))
                    {
                        AddWarning($"GPU", gpu.Name, "[yellow]Unknown[/]", "[yellow]WARN[/]");
                        //Custom.WriteLine($"We dont support your GPU, trust your spoofer provider or check manually", ConsoleColor.DarkRed);
                    }
                    else
                    {
                        if (spoofed.FindAll(m => m.UUID == UUID).Count > 0)
                        {
                            AddWarning($"GPU", gpu.Name, $"[yellow]{gpu.UUID}[/]", "[yellow]WARN[/]");
                            //Custom.WriteLine($"[GPU] {gpu.Name}(UUID) => {UUID}", ConsoleColor.DarkYellow);
                        }
                        else
                            AddWarning($"GPU", gpu.Name, $"[green]???[/]", "[green]OK[/]");
                    }
                }


            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }
        }

        private static void CheckDiskSerials(Components orig, Components spoof)
        {
            try
            {
                var original = orig.DiskDrives;
                var spoofed = spoof.DiskDrives;

                if (original == null)
                {
                    AnsiConsole.Write(
                        new Panel("[bold darkred]config/hardware.json[/]\n[red]Seems to be corrupted[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] Disks [/]", Justify.Center)
                            .Padding(1, 0));
                    return;
                }

                if (original.Count == 0)
                {
                    AnsiConsole.Write(
                        new Panel("[bold darkred]config/hardware.json[/]\n[red]Seems to be corrupted[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] Disks [/]", Justify.Center)
                            .Padding(1, 0));
                    return;
                }

                if (spoofed == null)
                {
                    AnsiConsole.MarkupLine("[bold darkred]Could not validate Disks[/]");
                    return;
                }

                if (spoofed.Count == 0)
                {
                    AnsiConsole.MarkupLine("[bold darkred]Could not validate Disks[/]");
                    return;
                }

                foreach(var disk in original)
                {
                    var serial = disk.SerialNumber;

                    if (spoofed.FindAll(m => m.SerialNumber == serial).Count > 0)
                    {
                        AddError($"Disk", disk.Model, $"[red]{serial}[/]", $"[red]FAIL[/]");
                    }
                    else
                        AddError($"Disk", disk.Model, $"[green]???[/]", $"[green]OK[/]");
                }
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }
        }

        private static void CheckRamSerials(Components orig, Components spoof)
        {
            try
            {
                var original = orig.Ram;
                var spoofed = spoof.Ram;

                if (original == null)
                {
                    AnsiConsole.Write(
                        new Panel("[bold darkred]config/hardware.json[/]\n[red]Seems to be corrupted[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] Ram [/]", Justify.Center)
                            .Padding(1, 0));
                    return;
                }

                if (original.Count == 0)
                {
                    AnsiConsole.Write(
                        new Panel("[bold darkred]config/hardware.json[/]\n[red]Seems to be corrupted[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] Ram [/]", Justify.Center)
                            .Padding(1, 0));
                    return;
                }

                if (spoofed == null)
                {
                    AnsiConsole.MarkupLine("[bold darkred]Could not validate Ram[/]");
                    return;
                }

                if (spoofed.Count == 0)
                {
                    AnsiConsole.MarkupLine("[bold darkred]Could not validate Ram[/]");
                    return;
                }

                foreach (var ram in original)
                {
                    var serial = ram.SerialNumber;

                    if (serial == "00000000")
                    {
                        AddError($"Ram", ram.Location, $"[green]{serial}[/]", $"[green]OK[/]");
                        continue;
                    }

                    if (spoofed.FindAll(m => m.SerialNumber == serial).Count > 0)
                    {
                        AddError($"Ram", ram.Location, $"[red]{serial}[/]", $"[red]FAIL[/]");
                    }
                    else
                        AddError($"Ram", ram.Location, $"[green]???[/]", $"[green]OK[/]");
                }
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }
        }

        private static void CheckSystemInformation(Components orig, Components spoof)
        {
            try
            {
                var original = orig.SystemInformation;
                var spoofed = spoof.SystemInformation;

                if(original == null)
                {
                    AnsiConsole.Write(
                        new Panel("[bold darkred]config/hardware.json[/]\n[red]Seems to be corrupted[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] Ram [/]", Justify.Center)
                            .Padding(1, 0));
                    return;
                }

                if (spoofed == null)
                {
                    AnsiConsole.MarkupLine("[bold darkred]Could not validate System Information[/]");
                    return;
                }

                if(original.UUID == spoofed.UUID)
                {
                    AddError($"System", "UUID", $"[red]{original.UUID}[/]", $"[red]FAIL[/]");
                }
                else
                    AddError($"System", "UUID", $"[green]{spoofed.UUID}[/]", $"[green]OK[/]");
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }
        }

        private static void CheckBaseBoardSerials(Components orig, Components spoof)
        {
            try
            {
                var original = orig.MotherboardInformation;
                var spoofed = spoof.MotherboardInformation;

                if(original == null)
                {
                    AnsiConsole.Write(
                        new Panel("[bold darkred]config/hardware.json[/]\n[red]Seems to be corrupted[/]")
                            .Border(BoxBorder.Double)
                            .BorderStyle(new Style(Color.Red))
                            .Header("[white on red] BaseBoard [/]", Justify.Center)
                            .Padding(1, 0));
                    return;
                }

                if (spoofed == null)
                {
                    AnsiConsole.MarkupLine("[bold darkred]Could not validate BaseBoard[/]");
                    return;
                }

                if(original.SerialNumber == spoofed.SerialNumber)   
                {
                    AddError($"Baseboard", original.Product, $"[red]{original.SerialNumber}[/]", $"[red]FAIL[/]");
                    //Custom.WriteLine($"[Baseboard] {original.Product} => [{spoofed.SerialNumber}]", ConsoleColor.Red);
                }
                else
                    AddError($"Baseboard", original.Product, $"[green]{spoofed.SerialNumber}[/]", $"[green]OK[/]");
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }
        }

        private static void CheckForWarnings(Components orig, Components spoof)
        {
            if (WindowsFastStartupEnabled(spoof))
            {
                AddWarning($"Windows", "Fast Startup", "", "[yellow]WARN[/]", "Disable this");
                //Custom.WriteLine("Windows fast startup is enabled, can lead to bans if using `Shutdown pc`", ConsoleColor.Yellow);
            }

            // Check GPU serials (not used for EAC)
            CheckGPUSerial(orig, spoof);

            // Check network adapters warnings
            CheckNetworkAdapters(orig, spoof, 1);

            // Check for partition warnings
            CheckPartitionSerials(orig, spoof);

            // Check for non spoofed ARP connections
            CheckNearbyDevices(orig, spoof);

            // Check for Operating system warnings
            CheckOperatingSystem(orig, spoof, 1);
        }

        private static bool WindowsFastStartupEnabled(Components spoof)
        {
            try
            {
                return spoof.WindowsFastStartup;
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }

            return true;
        }

        private static bool BluetoothFound(Components spoof)
        {
            try
            {
                if (spoof.BluetoothDevices == null)
                    return false;

                if (spoof.BluetoothDevices.Count == 0)
                    return false;
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }

            return true;
        }

        private static bool TPMEnabled(Components spoof)
        {
            try
            {
                if (spoof.TPM == null)
                    return false;
            }
            catch (Exception ex) { AnsiConsole.WriteException(ex); }

            return true;
        }
    }
}
