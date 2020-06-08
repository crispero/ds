﻿using System;
using StackExchange.Redis;
using NATS.Client;
using NATS.Client.Rx;
using NATS.Client.Rx.Ops;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace TextRankCalc
{
    class Program
    {
        private static bool isListening = true;
        static void Main(string[] args)
        {
            string natsUrl = "nats://" + Environment.GetEnvironmentVariable("NATS_HOST") + ":" + Environment.GetEnvironmentVariable("NATS_PORT");
            IConnection nats = new ConnectionFactory().CreateConnection(natsUrl);
            
            string redisUrl = Environment.GetEnvironmentVariable("REDIS_HOST") + ":" + Environment.GetEnvironmentVariable("REDIS_PORT");
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisUrl);
            
            string eventBusName = Environment.GetEnvironmentVariable("NATS_EVENT_BUS");

            Program.SubscribeOnEvents(redis, nats);
            Console.WriteLine("Starting listen events:");

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                Program.isListening = false;
            };

            while (isListening) {}
        }

        static void SubscribeOnEvents(ConnectionMultiplexer redis, IConnection nats)
        {
            IDatabase db = redis.GetDatabase();
            string eventBusName = Environment.GetEnvironmentVariable("NATS_EVENT_BUS");

            var events = nats.Observe(eventBusName)
                .Where(m => m.Data?.Any() == true)
                .Select(m => Encoding.Default.GetString(m.Data));

            events.Subscribe(id => 
            {
                string text = db.StringGet("data-" + id);

                int consonantsCount = Regex.Matches(text, @"[bcdfghjklmnpqrstvwxyz]").Count;
                int vowelCount = Regex.Matches(text, @"[aeiou]").Count;
                float value = consonantsCount == 0 ? 0 : (float)vowelCount / consonantsCount;
                db.StringSet("value-" + id, value.ToString());
            });
        }
    }
}
