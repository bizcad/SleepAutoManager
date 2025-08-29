# SleepAutoManager - Current Wake Devices #

## Wake Capable Decices the current setup ##

To find the current wake up devices run the following command from the developer Powershell.

```powershell

powercfg -devicequery wake_programmable

```

Query: What are the following devices:

- 	HID Keyboard Device
- 	Realtek Gaming GbE Family Controller
- 	Logitech Download Assistant (001)
- 	HID-compliant mouse (001)
- 	HID-compliant consumer control device
- 	Intel(R) Wireless-AC 9260 160MHz
- 	Microsoft Ergonomic Mouse (Mouse and Keyboard Center)
- 	HID-compliant vendor-defined device (001)

## Active Devices Explanation ##
Here’s what those friendly names usually represent and their relevance to wake sources:
- 	HID Keyboard Device

 	Generic USB HID keyboard using the built-in Windows driver. Often allowed to wake the PC via key press.
- 	Realtek Gaming GbE Family Controller

 	Realtek PCIe Gigabit Ethernet adapter. Supports Wake-on-LAN; can wake the PC on magic packet or pattern match if enabled in adapter settings.
- 	Logitech Download Assistant (001)

 	Logitech helper component that prompts to install/update Logitech software. The “(001)” denotes a specific HID interface/instance. Not a physical device; typically safe to disable as a wake source.
- 	HID-compliant mouse (001)

 	Generic USB HID mouse instance. Often wakes the PC on movement or click if allowed.
- 	HID-compliant consumer control device

 	HID “Consumer Control” interface (media keys like volume/play/pause) exposed by many keyboards. It can be a separate wake-capable interface; disabling wake here prevents media keys from waking.
- 	Intel(R) Wireless-AC 9260 160MHz

 	Intel Wi‑Fi adapter. Supports WoWLAN on some systems; can wake the PC from network activity if driver/OS/firmware allow and settings are enabled.
- 	Microsoft Ergonomic Mouse (Mouse and Keyboard Center)

 	Microsoft-branded mouse managed by the Mouse and Keyboard Center software. Functionally a HID mouse with extra features; may be listed separately and can be a wake source.
- 	HID-compliant vendor-defined device (001)

 	A vendor-specific HID interface (extra buttons, features for a keyboard/mouse, or a USB receiver function). Not always needed as a wake source; safe to disable for wake in most setups.
## Tips ##
- 	The “(001)” suffix is an instance/interface index; you may see multiple entries for one physical device.
- 	To see which can wake vs. are armed:
- 	List armed: powercfg /devicequery wake_armed
- 	List programmable: powercfg /devicequery wake_programmable
- 	To inspect specifics: open Device Manager, locate the device, check Properties > Power Management to allow/prevent waking; network adapters also have Advanced settings (e.g., Wake on Magic Packet).