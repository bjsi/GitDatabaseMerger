using System.Threading.Tasks;

namespace GitDatabaseMerger
{
    class Program
    {
        private static JsonRpcServer Server = new JsonRpcServer();

        static async Task Main(string[] args)
        {
            await Server.StartAsync();
        }
    }
}
