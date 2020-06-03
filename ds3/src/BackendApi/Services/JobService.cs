using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using NATS.Client;
using System.Text;

namespace BackendApi.Services
{
    public class JobService : Job.JobBase
    {
        private readonly static Dictionary<string, string> _jobs = new Dictionary<string, string>();
        private readonly ILogger<JobService> _logger;
        private readonly ConnectionMultiplexer _redis;
        private readonly IConnection _nats;

        public JobService(ILogger<JobService> logger)
        {
            _logger = logger;

            string redisUrl = Environment.GetEnvironmentVariable("REDIS_HOST") + ":" + Environment.GetEnvironmentVariable("REDIS_PORT");
            _redis = ConnectionMultiplexer.Connect(redisUrl);

            string natsUrl = "nats://" + Environment.GetEnvironmentVariable("NATS_HOST") + ":" + Environment.GetEnvironmentVariable("NATS_PORT");
            _nats = new ConnectionFactory().CreateConnection(natsUrl);
        }

        public override Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            string id = Guid.NewGuid().ToString();
            var resp = new RegisterResponse
            {
                Id = id
            };
            _jobs[id] = request.Description;
            SaveMessage(id, request.Description);
            PublishMessage(id);
            return Task.FromResult(resp);
        }

        private void SaveMessage(string id, string message)
        {
            IDatabase db = _redis.GetDatabase();
            db.StringSet(id, message);
        }

        public void PublishMessage(string id)
        {
            string eventBusName = Environment.GetEnvironmentVariable("NATS_EVENT_BUS");
            byte[] data = Encoding.Default.GetBytes(id);
            _nats.Publish(eventBusName, data);
        }
    }
}