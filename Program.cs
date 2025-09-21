using SleepAutoManager;
using System;
using System.Collections.Generic;
using System.Threading;

Console.WriteLine("SleepAutoManager");

var core = new SleepAutoCore();

var devices = SleepAutoCore.GetWakeProgrammableDevices();
if (devices.Count == 0)
{
    Console.WriteLine("No wake-capable devices were found.");
    return;
}


// Show quick troubleshooting info before toggling
core.PrintTroubleshootingInfo();

if (core.AnyWakeEnabled())
{
    Console.WriteLine();
    Console.WriteLine("Wake-programmable devices:");
    int sel = 1;
    Dictionary<int, string> dic = [];
    foreach (var device in devices)
    {
        dic.Add(sel, device);
        Console.WriteLine($"{sel++}. - {device}");
    }

    // User selection
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  E - ENABLE wake for all devices (no sleep)");
    Console.WriteLine("  D - DISABLE wake for all devices, then Sleep");
    Console.WriteLine("  S - Sleep NOW with current wake settings (no changes)");
    Console.WriteLine("  n - Leave only device #n enabled, disable the rest, then Sleep");
    Console.WriteLine("  Enter with no input to abort");
    Console.Write("Selection [E/D/S/n]: ");

    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(input))
    {
        Console.WriteLine("Aborting...");
        Thread.Sleep(1000);
    }
    else
    {
        switch (input.ToUpperInvariant())
        {
            case "E":
                Console.WriteLine("Enabling wake permissions for all devices...");
                core.EnableWakeForAll();
                Thread.Sleep(1000);
                break;

            case "D":
                Console.WriteLine("Disabling wake permissions for devices...");
                core.DisableWakeAndSleep();
                break;

            case "S":
                core.NoWakeActionAndSleep();
                break;

            default:
                if (int.TryParse(input, out int index) && dic.TryGetValue(index, out string value))
                {
                    Console.WriteLine($"Disabling wake permissions for devices except #{index}. {value}");
                    core.DisableWakeAndSleep(dic, index);
                }
                else
                {
                    Console.WriteLine($"Invalid selection '{input}'. Aborting...");
                    Thread.Sleep(1000);
                }
                break;
        }
    }

 }
