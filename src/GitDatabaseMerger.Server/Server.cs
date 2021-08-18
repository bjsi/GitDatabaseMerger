using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AustinHarris.JsonRpc;
using GitDatabaseMerger.Interop;

namespace GitDatabaseMerger
{
    public class MergeRequestListener : IDisposable
    {
        private TcpListener Server { get; set; }
        private MergeAPI MergeAPI { get; }
        private string Hostname { get; }
        private int Port { get; }

        public MergeRequestListener(string hostname, int port, IMerger merger)
        {
            this.Hostname = hostname;
            this.Port = port;
            this.MergeAPI = new MergeAPI(merger);
        }

        ~MergeRequestListener()
        {
            Dispose();
        }

        public async Task StartAsync()
        {
            Server = new TcpListener(IPAddress.Parse(Hostname), Port);
            Server.Start();
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

        public void Dispose()
        {
            Console.WriteLine("Stopping JsonRpcServer");
            Server?.Stop();
        }
    }
}
