using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AustinHarris.JsonRpc;
using GitDatabaseMerger.Interop;

namespace GitDatabaseMerger.Server
{
    public class MergeRequestListener : JsonRpcService, IDisposable
    {
        private TcpListener Server { get; set; }
        private string Hostname { get; }
        private int Port { get; }

        public EventHandler<MergeRequestEventArgs> OnMergeRequest;

        public MergeRequestListener(string hostname, int port)
        {
            this.Hostname = hostname;
            this.Port = port;
        }

        [JsonRpcMethod]
        public void Merge(string local, string remote, string ancestor)
        {
            OnMergeRequest?.Invoke(this, new MergeRequestEventArgs(local, remote, ancestor));
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
                            Console.WriteLine("Received Request: " + line);

                            var response = await JsonRpcProcessor.Process(line);
                            Console.WriteLine("Sent Response: " + response);

                            await writer.WriteLineAsync(response);
                            await writer.FlushAsync();

                            client.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"JSON RPC Server caught exception: {e}");
                }
        }

        public void Dispose()
        {
            Console.WriteLine("Stopping the JSON RPC Server");
            Server?.Stop();
        }
    }
}
