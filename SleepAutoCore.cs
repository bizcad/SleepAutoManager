using System;
using System.Collections.Generic;
using System.Linq;

namespace SleepAutoManager;

public sealed class SleepAutoCore
{
    public const string SleepCommand = "rundll32.exe powrprof.dll,SetSuspendState 0,1,0";

    private readonly IReadOnlyList<string> _devices;
    private readonly IProcessRunner _runner;
    private static readonly char[] separator = ['\r', '\n'];

    public SleepAutoCore(IEnumerable<string> devices, IProcessRunner runner)
    {
        _devices = devices?.ToList() ?? throw new ArgumentNullException(nameof(devices));
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
    }

    public static List<string> GetWakeProgrammableDevices(IProcessRunner runner)
    {
        // Primary: all devices capable of waking
        var (_, capableOut) = runner.RunWithOutput("powercfg -devicequery wake_programmable");
        if (string.IsNullOrWhiteSpace(capableOut))
        {
            Console.WriteLine("No wake_programmable devices found.");
            return  [];
        }

        var devices = ParseDeviceList(capableOut);

        if (devices.Count == 0)
        {
            // Fallback: currently armed devices
            var (_, armedOut) = runner.RunWithOutput("powercfg -devicequery wake_armed");
            devices = ParseDeviceList(armedOut);
        }

        return devices;
    }

    public bool AnyWakeEnabled()
    {
        Console.WriteLine("Currently wake-enabled devices:");
        var armed = GetWakeArmedDevices();
        foreach (var device in armed)
        {
            Console.WriteLine($" - {device}");
        }
        // Exact line match, case-insensitive
        return _devices.Any(d => armed.Contains(d));
    }

    public void DisableWakeAndSleep()
    {
        foreach (var device in _devices)
        {
            if (device.Contains("Microsoft Ergonomic", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"The following device is a Microsoft Ergonomic device and will remain armed to awaken the machine:\n {device}");
                continue;
            }
            _runner.Run($@"powercfg -devicedisablewake ""{device}""");
        }
        Console.WriteLine("Putting the machine to sleep...");
        _runner.Run(SleepCommand);
        Environment.Exit(0);
    }

    public void DisableWakeAndSleep(Dictionary<int, string> choices, int key)
    {
        string selectedDevice = choices[key];
        foreach (var device in _devices)
        {
            if (string.Equals(device, selectedDevice, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"The following device will remain armed to awaken the machine:\n {device}");
                continue;
            }
            _runner.Run($@"powercfg -devicedisablewake ""{device}""");
        }
        Console.WriteLine("Putting the machine to sleep...");
        _runner.Run(SleepCommand);
        Environment.Exit(0);
    }

    public void NoWakeActionAndSleep()
    {
        Console.WriteLine("Sleeping now with current wake-enabled devices (no changes)...");
        _runner.Run(SleepCommand);
        Environment.Exit(0);
    }

    public void EnableWakeForAll()
    {
        foreach (var device in _devices)
        {
            _runner.Run($@"powercfg -deviceenablewake ""{device}""");
        }
    }

    public void PrintTroubleshootingInfo()
    {
        Console.WriteLine();
        Console.WriteLine("--- Troubleshooting ---");
        var (_, lastWake) = _runner.RunWithOutput("powercfg /lastwake");
        Console.WriteLine("powercfg /lastwake:");
        Console.WriteLine(string.IsNullOrWhiteSpace(lastWake) ? "(no output)" : lastWake.TrimEnd());
        Console.WriteLine();
        var (_, timers) = _runner.RunWithOutput("powercfg /waketimers");
        Console.WriteLine("powercfg /waketimers:");
        Console.WriteLine(string.IsNullOrWhiteSpace(timers) ? "(no wake timers)" : timers.TrimEnd());
        Console.WriteLine("-----------------------");
        Console.WriteLine();
    }

    private HashSet<string> GetWakeArmedDevices()
    {
        var (_, stdout) = _runner.RunWithOutput("powercfg -devicequery wake_armed");
        return new HashSet<string>(ParseDeviceList(stdout), StringComparer.OrdinalIgnoreCase);
    }

    private static List<string> ParseDeviceList(string output)
    {
        return [.. output
            .Split(separator, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)];
    }
}
