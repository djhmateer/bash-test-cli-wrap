using System;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.EventStream;

namespace BashTest
{
    class Program
    {
        // Run this from WSL2 side as I'm expecting to use bash
        // dotnet run
        static async Task Main(string[] args)
        {
            // 1.Task based 
            //var result = await Cli.Wrap("infra.azcli")
            //                .WithWorkingDirectory(workingDir)
            //                .ExecuteAsync();

            // 2.Pull based event stream
            // https://github.com/Tyrrrz/CliWrap#pull-based-event-stream
            // don't need to call .ExecuteAsync as we are calling ListenAsync below
            var cmd = Cli.Wrap("infra.azcli");

            // Can pass a cancellation token
            using var cts = new CancellationTokenSource();

            // Cancel automatically after a timeout of 10 seconds
            //cts.CancelAfter(TimeSpan.FromSeconds(10));

            // Cli will throw if a non 0 return status
            try
            {
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
                            // it could be that we want to exit here if any of the script writes to stderr
                            // make sure script debugging turned off ie no set -x
                            break;
                        case ExitedCommandEvent exited:
                            //_output.WriteLine($"Process exited; Code: {exited.ExitCode}");
                            Console.WriteLine($"Process exited; Code: {exited.ExitCode}");
                            break;
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine("Operation cancelled - can I recover / clean up?");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception - this can be caused when the bash command returns a non 0 status");
            }

            //Console.WriteLine($"Result: {result}");
            Console.WriteLine($"Done");
        }
    }
}
