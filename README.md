# MaybeSpoofed
- i have a talking alien, i don't need publicity

A small tiny program to check if you are Hardware spoofed for EAC Rust

https://dotnet.microsoft.com/en-us/download

![Maybe Spoofed](https://i.imgur.com/hrD4Q5Q.png)

## How to use
- Ensure you aren't spoofed, restart pc.
- Open program, a new file will be created in config/hardware.json
- Close program, use a spoofer you prefer.
- Open program and check the results

## Will not work for these type of spoofers:
- Hypervisors
- Network level spoofers

## Will validate your Motherboard and check if a spoofer perm-spoofed you
- AsRock
- ASUS
- Gigabyte
- MSI
- If it shows indication that you are perm spoofed incorrectly, you should contact me ASAP on Discord and I will help you for a small fee
- Discord is found in the Program executable

## Will confirm you are spoofed for EAC Rust
- Will throw error if TPM is enabled
- Will throw error if Bluetooth devices are found
- Will throw error if BaseBoard SerialNumber is not spoofed
- Will throw error if System UUID is not spoofed
- Will throw error if Ram serials is not spoofed
- Will throw error if Disk serials is not spoofed
- Will throw error if Network adapters is not spoofed
- Will throw error if Monitor serials is not spoofed
- Will throw error if Windows identifiers is not spoofed

## Will warn for misc serials (Not sure if EAC is grabbing these)
- Windows Machine ID not spoofed
- Windows Username not offline account
- Bios ReleaseDate is outdated
- ARP(NearbyDevices) MAC [This could be Router, another PC, Phone or any device that is communicating with ur PC]
- Disk partition serial not spoofed
- Network adapters GUID not spoofed
- Nvidia GPU UUID not spoofed
- Windows Fast Startup enabled [Could cause potential issues if not Restarting PC after using spoofer/cheat]
