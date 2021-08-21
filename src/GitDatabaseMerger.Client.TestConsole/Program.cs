using GitDatabaseMerger.Client.Messenger;
using GitDatabaseMerger.Client.TestConsole.Helpers;
using GitDatabaseMerger.Interop;
using System;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Client.TestConsole
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length != 3)
                Console.WriteLine("args: local, remote, ancestor");

            var hostname = "127.0.0.1";
            var port = 8090;
            var messenger = new JsonRpcMergeMessenger(hostname, port);

            Console.WriteLine($"Sending merge request - local: {args[0]} remote: {args[1]} ancestor: {args[2]}");
            var result = await messenger.MergeAsync(args[0], args[1], args[2]);
            if (result == MergeResult.Success)
                Console.WriteLine("Merge succeeded!");
            else
                Console.WriteLine($"Merge failed: {result.Name()}");

            return (int)result;
        }
    }
}
