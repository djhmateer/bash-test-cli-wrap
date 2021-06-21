using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BashTest
{
    //class Program
    //{
    //    static async Task Main(string[] args)
    //    {
    //        Console.WriteLine("Hello World!");
    //        Console.WriteLine($"AppDomain.CurrentDomain.BaseDirectory is {AppDomain.CurrentDomain.BaseDirectory}");
    //        Console.WriteLine($"Environment.CurrentDirectory is {Environment.CurrentDirectory}");
    //        var arg = "";
    //        //var cmd = $"scripts/00magic.sh --param {arg}";
    //        //var cmd = $"test.sh";
    //        var cmd = $"infra.azcli";
    //        var directory = Environment.CurrentDirectory;
    //        var result = await ShellHelper.Bash(directory + "/" + cmd);
    //        Console.WriteLine($"Result: {result}");
    //    }
    //}

    // This works
    public static class ShellHelper
    {
        public static Task<int> Bash(string cmd)
        {
            var source = new TaskCompletionSource<int>();
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var directory = Environment.CurrentDirectory;
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                //logger.LogWarning(process.StandardError.ReadToEnd());
                Console.WriteLine(" warning: " + process.StandardError.ReadToEnd());
                //logger.LogInformation(process.StandardOutput.ReadToEnd());
                Console.WriteLine(process.StandardOutput.ReadToEnd());
                if (process.ExitCode == 0)
                {
                    source.SetResult(0);
                }
                else
                {
                    source.SetException(new Exception($"Command `{cmd}` failed with exit code `{process.ExitCode}`"));
                }

                process.Dispose();
            };

            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                //logger.LogError(e, "Command {} failed", cmd);
                Console.WriteLine($"Command {cmd} failed {e.ToString()}");
                source.SetException(e);
            }

            return source.Task;
        }
    }

}
