using GitDatabaseMerger.Client.Helpers;
using GitDatabaseMerger.Client.Models;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Client.Messenger
{
    public class JsonRpcMessenger
    {
        private string Hostname { get; }
        private int Port { get; }

        public JsonRpcMessenger(string hostname, int port)
        {
            this.Hostname = hostname;
            this.Port = port;
        }

        public async Task<T> SendRequestAsync<T>(string method, params object[] args) where T : JsonResponse
        {
            var jsonRequest = new JsonRequest(method, args);
            try
            {
                using (TcpClient client = new TcpClient(Hostname, Port))
                using (NetworkStream stream = client.GetStream())
                {
                    // Write request
                    var requestString = jsonRequest.ToString();
                    byte[] requestBinary = Encoding.UTF8.GetBytes(requestString + "\n");
                    await stream.WriteAsync(requestBinary, 0, requestBinary.Length);
                    Console.WriteLine("Sent Request: " + requestString);

                    // Read response
                    var responseBinary = new byte[256];
                    await stream.ReadAsync(responseBinary, 0, responseBinary.Length);
                    var resStr = Encoding.UTF8.GetString(responseBinary);
                    Console.WriteLine("Received Response: " + resStr);
                    return HandleResponse<T>(resStr);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to send a JSON RPC request with exception {e}");
            }

            return null;
        }

        private T HandleResponse<T>(string res) where T : JsonResponse
        {
            return !string.IsNullOrEmpty(res)
              ? res.Deserialize<T>()
              : null;
        }
    }
}