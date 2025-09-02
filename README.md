# SleepAutoManager

Windows console app to troubleshoot and toggle which devices are allowed to wake the computer. It shells out to `powercfg.exe` to:
- List devices that are user-configurable to wake the system (`wake_programmable`).
- Show devices currently armed to wake the system (`wake_armed`).
- Enable or disable wake permission per device.
- Display quick diagnostics: `powercfg /lastwake` and `powercfg /waketimers`.

Note: Most actions require an elevated console (Run as Administrator).
## Problem to solve 
I would put my Windows 11 machine to sleep. Later I would come back and the machine had woken up by itself; thereby wasting power and annoying me. After querying Github Copilot for a while I found the powercfg command which, among other functions, allows one to control what devices allow the machine to be awakened when it is asleep. 
## What it does
1. Discovers wake-programmable devices using:
   - `powercfg -devicequery wake_programmable`
   - Falls back to `powercfg -devicequery wake_armed` if needed.
2. Prints troubleshooting info:
   - `powercfg /lastwake`
   - `powercfg /waketimers`
3. Shows currently wake-enabled devices.
4. Presents a menu:
   - `E` to Enable wake for all wake-programmable devices.
   - `D` to Disable wake for all wake-programmable devices (the app keeps specific devices enabled per code rules, and then initiates Sleep).
   - A number to Disable wake for all devices except the selected device; then initiate Sleep.
   - Empty/other to Abort the sleep without changing the devices.

When disabling, the app calls:
- `powercfg -devicedisablewake "<device>"`
- Then requests Sleep via `rundll32.exe powrprof.dll,SetSuspendState 0,1,0`.

## Requirements
- Windows 11 (or Windows 10) with `powercfg.exe` (built-in).
- .NET SDK 9.0 or later (to build from source).
- Administrator privileges to enable/disable device wake permissions.

## Build
From the repository root:

- Restore and build
  - `dotnet build -c Release`

- Run (from source)
  - `dotnet run --project SleepAutoManager.csproj`

- Publish a single EXE (optional)
  - `dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true`
  - Output will be in `bin/Release/net9.0/win-x64/publish/`.

## Usage
1. <b><u>Important</u></b> Open in an elevated Windows Terminal / PowerShell / CMD (Run as Administrator).
2. Run the app (either `dotnet run` or the published EXE).
3. Review the troubleshooting info (`/lastwake`, `/waketimers`).
4. Review the device list and choose:
   - Press `E` to re-enable wake for  all.
   - Press `D` to disable wake for all (the app may leave certain devices enabled by design).
   - Type a number to leave just that device enabled and disable the rest.
   - Press Enter with no input to abort.

After disabling, the app will request Sleep. The process exits after the system wakes.

## Notes and tips
- Device names are "friendly names" as reported by `powercfg`. The app quotes them when invoking `powercfg`.
- Network adapters (e.g., Realtek/Intel) may require enabling Wake-on-LAN in Device Manager (Power Management and Advanced tabs) and sometimes in firmware/BIOS.
- On Modern Standby (S0) systems, sleep behavior and reports differ from legacy S3.

## Troubleshooting
You can always power down your machine and restart, which should reset the default wake-programmable devices.  If they are not all re-enabled, you can use the 'e' or 'E' command.

## Commands used
- `powercfg -devicequery wake_programmable`
- `powercfg -devicequery wake_armed`
- `powercfg -deviceenablewake "<device>"`
- `powercfg -devicedisablewake "<device>"`
- `powercfg /lastwake`
- `powercfg /waketimers`

## Disclaimer
This program was written for my Windows 11 machine. Your milage may vary. It uses an icon designed by a 3rd party.  If you decide to use the icon, you should probably buy him a coffee.

Use at your own risk. Changing device wake permissions can affect your ability to wake the PC from sleep.
## Attribution
I added the free version of a snooze.png icon by Pixel perfect (https://www.flaticon.com/authors/pixel-perfect) (<a href="https://www.flaticon.com/free-icon/snooze_3602333?term=snooze&page=1&position=5&origin=tag&related_id=3602333" title="snooze icons">Snooze icons created by Pixel perfect - Flaticon</a>) 
I did not buy him a coffee because this program is for non-commercial uses.
