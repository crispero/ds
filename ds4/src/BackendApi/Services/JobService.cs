using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
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
        private readonly IDatabase _db;

        public JobService(ILogger<JobService> logger)
        {
            _logger = logger;

            string redisUrl = Environment.GetEnvironmentVariable("REDIS_HOST") + ":" + Environment.GetEnvironmentVariable("REDIS_PORT");
            _redis = ConnectionMultiplexer.Connect(redisUrl);

            string natsUrl = "nats://" + Environment.GetEnvironmentVariable("NATS_HOST") + ":" + Environment.GetEnvironmentVariable("NATS_PORT");
            _nats = new ConnectionFactory().CreateConnection(natsUrl);

            _db = _redis.GetDatabase();
        }

        public override Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            string id = Guid.NewGuid().ToString();
            var resp = new RegisterResponse
            {
                Id = id
            };
            _jobs[id] = request.Description;
            SaveMessage(id, request);
            PublishMessage(id);
            return Task.FromResult(resp);
        }

        public override Task<ProcessingResultResponse> GetProcessingResult(RegisterResponse response, ServerCallContext context)
        {
            var processingResult = new ProcessingResultResponse {
                Status = ProcessingResultStatus.InProgress,
                Value = "",
            };

            for (int i = 0; i < 3; i++)
            {
                string value = _db.StringGet("value-" + response.Id);
                if (value != null)
                {
                    processingResult.Status = ProcessingResultStatus.Done;
                    processingResult.Value = value;
                    break;
                }
                Thread.Sleep(1000);
            }

            return Task.FromResult(processingResult);
        }

        private void SaveMessage(string id, RegisterRequest request)
        {
            _db.StringSet("description-" + id, request.Description);
            _db.StringSet("data-" + id, request.Data);
        }

        public void PublishMessage(string id)
        {
            string eventBusName = Environment.GetEnvironmentVariable("NATS_EVENT_BUS");
            byte[] data = Encoding.Default.GetBytes(id);
            _nats.Publish(eventBusName, data);
        }
    }
}