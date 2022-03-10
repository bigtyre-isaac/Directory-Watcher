using CommandLine;

public class Options
{
    [Option('w', "wait", Required = false, HelpText = "Time in milliseconds to wait after file activity stops before executing the command.")]
    public int DelayInMilliseconds { get; set; }
    
    [Option('c', "command", Required = true, HelpText = "Command to execute after file changes.")]
    public string? Command { get; set; }
   
    [Option('d', "dir", Required = true, HelpText = "Directory to watch for file changes.")]
    public string? TargetDirectory { get; set; }
}