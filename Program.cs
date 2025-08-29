using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

class Program
{
    static void Main()
    {
        Console.WriteLine("SleepAutoManager - Device Wake Toggle");

        var runner = new RealProcessRunner();

        var devices = SleepAutoCore.GetWakeProgrammableDevices(runner);
        if (devices.Count == 0)
        {
            Console.WriteLine("No wake-capnable devices were found.");
            return;
        }

        var core = new SleepAutoCore(devices, runner);

        // Show quick troubleshooting info before toggling
        core.PrintTroubleshootingInfo();

        if (core.AnyWakeEnabled())
        {
            Console.WriteLine("The following devices will have wake permissions disabled:");
            foreach (var device in devices)
            {
                Console.WriteLine($" - {device}");
            }
            Console.Write("Proceed? Press 'Y' to continue, any other key to cancel: ");
            var key = Console.ReadKey(intercept: true);
            Console.WriteLine();

            if (key.KeyChar == 'y' || key.KeyChar == 'Y')
            {
                Console.WriteLine("Disabling wake permissions for devices...");
                core.DisableWakeForAllAndSleep();
            }
            else
            {
                Console.WriteLine("Aborting...");
                Thread.Sleep(2000);
            }
        }
        else
        {
            Console.WriteLine("Re-enabling wake permissions for devices...");
            core.EnableWakeForAll();
            Console.WriteLine("Wake permissions re-enabled. Exiting.");
        }
    }
}

public interface IProcessRunner
{
    int Run(string command);
    (int ExitCode, string StdOut) RunWithOutput(string command);
}

public sealed class RealProcessRunner : IProcessRunner
{
    public int Run(string command)
    {
        var psi = new ProcessStartInfo("cmd.exe", "/c " + command)
        {
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi);
        proc?.WaitForExit();
        return proc?.ExitCode ?? -1;
    }

    public (int ExitCode, string StdOut) RunWithOutput(string command)
    {
        var psi = new ProcessStartInfo("cmd.exe", "/c " + command)
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi);
        if (proc is null) return (-1, string.Empty);

        string output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();
        return (proc.ExitCode, output);
    }
}

public sealed class SleepAutoCore
{
    public const string SleepCommand = "rundll32.exe powrprof.dll,SetSuspendState 0,1,0";

    private readonly IReadOnlyList<string> _devices;
    private readonly IProcessRunner _runner;

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
            Console.WriteLine("wake_programmable devices found.");
            return new List<string>();
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
        var armed = GetWakeArmedDevices();
        // Exact line match, case-insensitive
        return _devices.Any(d => armed.Contains(d));
    }

    public void DisableWakeForAllAndSleep()
    {
        foreach (var device in _devices)
        {
            if (device.IndexOf("Microsoft Ergonomic", StringComparison.OrdinalIgnoreCase) >= 0)
             {
                Console.WriteLine($"Skipping mouse/keyboard device: {device}");
                continue;
            }
            //_runner.Run($@"powercfg -devicedisablewake ""{device}""");
        }
        Console.WriteLine("Putting the machine to sleep...");
        _runner.Run(SleepCommand);
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
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)];
    }
}
