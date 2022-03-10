using CommandLine;
using CommandLine.Text;

Console.WriteLine("Application started.");
TimeSpan delay = TimeSpan.Zero;
Guid lastExecutionId = Guid.Empty;
object executionLock = new();
string? Command = null;
bool IsRunning = false;
bool UpdateQueued = false;

var tcs = new TaskCompletionSource();

await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(async o =>
    {
        var targetDirectory = o.TargetDirectory ?? throw new ArgumentException("Target directory not configured.");
        if (!Directory.Exists(targetDirectory) || !Path.IsPathRooted(targetDirectory))
        {
            var absoluteDirectory = Path.Combine(Environment.CurrentDirectory, targetDirectory.TrimStart("./"));
            if (Directory.Exists(absoluteDirectory))
            {
                targetDirectory = absoluteDirectory;
            }
            else
            {
                Console.WriteLine($"Error: Target directory '{targetDirectory}' does not exist.");
                return;
            }
        }
        Console.WriteLine($"Target directory: {targetDirectory}");
        Console.WriteLine($"Command: {o.Command}");
        Console.WriteLine($"Wait time: {o.DelayInMilliseconds:F0}ms");


        delay = TimeSpan.FromMilliseconds(o.DelayInMilliseconds);
        Command = o.Command;

        var watcher = new FileSystemWatcher(targetDirectory);
        watcher.Changed += FileChanged;
        watcher.Created += FileChanged;
        watcher.Deleted += FileChanged;
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        Console.WriteLine($"Started watching for file changes. Press Ctrl+C to quit.");
        await Task.Delay(-1);
    }
);



async void FileChanged(object sender, FileSystemEventArgs e)
{
    bool printWaitMessage = false;
    lock (executionLock)
    {
        if (!UpdateQueued)
        {
            UpdateQueued = true;
            printWaitMessage = true;
            Console.WriteLine("Change detected.");
        }
    }

    if (delay != TimeSpan.Zero)
    {
        if (printWaitMessage)
        {
            Console.WriteLine($"Waiting for {delay.TotalMilliseconds:#,##0}ms of inactivity.");
        }

        Guid instanceId = Guid.NewGuid();
        lock (executionLock)
        {
            lastExecutionId = instanceId;
        }

        await Task.Delay(delay);
        lock (executionLock)
        {
            if (lastExecutionId != instanceId || IsRunning)
            {
                return;
            }
        }
    }

    lock (executionLock)
    {
        if (IsRunning) return;

        IsRunning = true;
        try
        {
            UpdateQueued = false;
            string output = RunCommand();
            Console.Write(output);

        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while running command: " + ex.Message);
        }
        finally
        {
            IsRunning = false;
        }
    }
}

string RunCommand()
{
    Console.Write($"Running command \"{Command}\" ...");
    if (Command == null) return string.Empty;

    var command = Command.Replace("\"", "\\\"");

    try
    {
        string result = "";
        using (System.Diagnostics.Process proc = new System.Diagnostics.Process())
        {
            if (OperatingSystem.IsWindows())
            {
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.Arguments = "/c \"" + command + "\"";

            }
            else if (OperatingSystem.IsLinux())
            {
                proc.StartInfo.FileName = "/bin/bash";
                proc.StartInfo.Arguments = "-c \" " + command + " \"";

            }

            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();

            result += proc.StandardOutput.ReadToEnd();
            result += proc.StandardError.ReadToEnd();

            proc.WaitForExit();
        }

        Console.WriteLine($"Done!");

        return result;
    }
    catch (Exception)
    {
        Console.WriteLine("Failed!");
        throw;
    }

}


public static class StringExtensions
{
    public static string TrimStart(this string input, string remove, StringComparison stringComparison = StringComparison.Ordinal)
    {
        if (!input.StartsWith(remove, stringComparison)) return input;
        return input[remove.Length..];
    }
}