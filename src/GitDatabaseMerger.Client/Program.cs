using GitDatabaseMerger.Client.Helpers;
using GitDatabaseMerger.Client.Messenger;
using GitDatabaseMerger.Interop;
using System;

namespace GitDatabaseMerger.Client
{
    class Program
    {
        private static GitMessenger Messenger { get; } = new GitMessenger();

        static int Main(string[] args)
        {
            var res = Messenger.Merge("local filepath", "remote filepath", "ancestor filepath");
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
