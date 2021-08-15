using GitDatabaseMerger.Client.Helpers;
using GitDatabaseMerger.Client.Models;
using System;
using System.Net.Sockets;
using System.Text;

namespace GitDatabaseMerger.Client.Messenger
{
    public class JsonMessenger
    {
        private string Hostname { get; } = "127.0.0.1";
        private int Port { get; } = 8090;

        public T SendRequest<T>(string method, params object[] args) where T : JsonResponse
        {
            var request = new JsonRequest(method, args);
            try
            {
                using (TcpClient client = new TcpClient(Hostname, Port))
                using (NetworkStream stream = client.GetStream())
                {
                    var reqStr = request.ToString();
                    byte[] requestBinary = Encoding.UTF8.GetBytes(reqStr + "\n");
                    stream.Write(requestBinary, 0, requestBinary.Length);
                    Console.WriteLine("Sent Message: " + reqStr);
                    var responseBinary = new byte[256];
                    stream.Read(responseBinary, 0, responseBinary.Length);
                    var resStr = Encoding.UTF8.GetString(responseBinary);
                    Console.WriteLine("Received Message: " + resStr);
                    return HandleResponse<T>(resStr);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to send a TCP request with exception {e}");
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