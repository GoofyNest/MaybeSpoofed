# MaybeSpoofed
A small tiny program to check if you are Hardware spoofed for EAC Rust

https://dotnet.microsoft.com/en-us/download

![Maybe Spoofed](https://i.imgur.com/hrD4Q5Q.png)

## How to use
- Ensure you aren't spoofed, restart pc.
- Open program, a new file will be created in config/hardware.json
- Close program, use a spoofer you prefer.
- Open program and check the results

## Will warn for
- Trusted Platform Module(TPM) enabled
- Bluetooth devices count > 0
- Windows fast startup enabled
- Motherboard SerialNumber not being spoofed
- SystemInformation UUID not being spoofed
- Ram serial numbers not being spoofed
- Disk drive serials not being spoofed
- NVIDIA GPU UUID not spoofed
- Network MAC not spoofed
- Monitor serials not being spoofed
- Router MAC not spoofed
- Windows username not offline account
- SecureBoot disabled
- Bios version outdated
