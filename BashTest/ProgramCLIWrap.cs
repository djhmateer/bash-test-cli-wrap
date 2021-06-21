using System;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.EventStream;

namespace BashTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World from C#!");
            Console.WriteLine($"AppDomain.CurrentDomain.BaseDirectory is {AppDomain.CurrentDomain.BaseDirectory}");
            Console.WriteLine($"Environment.CurrentDirectory is {Environment.CurrentDirectory}");

            // 1.Task based 
            //var result = await Cli.Wrap("infra.azcli")
            //                .WithWorkingDirectory(workingDir)
            //                .ExecuteAsync();

            // 2.Pull based event stream
            // https://github.com/Tyrrrz/CliWrap#pull-based-event-stream
            // don't need to call .ExecuteAsync as we are calling ListenAsync below
            var cmd = Cli.Wrap("infra.azcli");

            using var cts = new CancellationTokenSource();

            // Cancel automatically after a timeout of 10 seconds
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            await foreach (var cmdEvent in cmd.ListenAsync(cts.Token))
            {
                switch (cmdEvent)
                {
                    case StartedCommandEvent started:
                        //_output.WriteLine($"Process started; ID: {started.ProcessId}");
                        Console.WriteLine($"Process started; ID: {started.ProcessId}");
                        break;
                    case StandardOutputCommandEvent stdOut:
                        //_output.WriteLine($"Out> {stdOut.Text}");
                        Console.WriteLine($"Out> {stdOut.Text}");
                        break;
                    case StandardErrorCommandEvent stdErr:
                        //_output.WriteLine($"Err> {stdErr.Text}");
                        Console.WriteLine($"Err> {stdErr.Text}");
                        break;
                    case ExitedCommandEvent exited:
                        //_output.WriteLine($"Process exited; Code: {exited.ExitCode}");
                        Console.WriteLine($"Process exited; Code: {exited.ExitCode}");
                        break;
                }
            }

            //Console.WriteLine($"Result: {result}");
            Console.WriteLine($"Done");
        }
    }
}
