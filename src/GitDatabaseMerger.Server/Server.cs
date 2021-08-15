using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AustinHarris.JsonRpc;

namespace GitDatabaseMerger
{
    public class JsonRpcServer
    {
        public bool HasExited { get; set; }
        private TcpListener Server { get; set; }
        private MergeAPI Service { get; set; } = new MergeAPI();
        private const int Port = 8090;

        public async Task StartAsync()
        {
            Server = new TcpListener(IPAddress.Parse("127.0.0.1"), Port);
            Server.Start();
            while (!HasExited)
            {
                try
                {
                    using (var client = await Server.AcceptTcpClientAsync())
                    using (var stream = client.GetStream())
                    {
                        Console.WriteLine("Client connected.");

                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                        {

                            var line = await reader.ReadLineAsync();
                            Console.WriteLine("Received request from client: " + line);

                            var response = await JsonRpcProcessor.Process(line);
                            Console.WriteLine("Sent response to client: " + response);

                            await writer.WriteLineAsync(response);
                            await writer.FlushAsync();

                            client.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Caught exception: " + e.Message);
                }
            }

            Console.WriteLine("Stopping JsonRpcServer");
            Server.Stop();
        }
    }
}
