using System;
using System.Threading.Tasks;
using BackendApi;
using Grpc.Net.Client;

namespace BackendClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var client = new Job.JobClient(channel);
            var reply = await client.RegisterAsync(
                              new RegisterRequest { Description = "This is job" });
            Console.WriteLine("Job Id: " + reply.Id);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }   
    }
}
