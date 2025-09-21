namespace SleepAutoManager;

public interface IProcessRunner
{
    int Run(string command);
    (int ExitCode, string StdOut) RunWithOutput(string command);
}
