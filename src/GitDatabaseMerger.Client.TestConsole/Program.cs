using GitDatabaseMerger.Client.Messenger;
using GitDatabaseMerger.Client.TestConsole.Helpers;
using GitDatabaseMerger.Interop;
using System;

namespace GitDatabaseMerger.Client.TestConsole
{
    class Program
    {
        static int Main(string[] args)
        {
            var hostname = "127.0.0.1";
            var port = 8090;
            var msgr = new JsonRpcMergeMessenger(hostname, port);
            var res = msgr.Merge("local filepath", "remote filepath", "ancestor filepath");
            if (res == MergeResult.Success)
            {
                Console.WriteLine("Merge succeeded!");
            }
            else
            {
                Console.WriteLine($"Merge failed: {res.Name()}");
            }

            return (int)res;
        }
    }
}
