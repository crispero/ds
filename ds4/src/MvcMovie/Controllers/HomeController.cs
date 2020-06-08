using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MvcMovie.Models;
using BackendApi;
using Grpc.Net.Client;

namespace MvcMovie.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost] 
        public async Task<IActionResult> GetTask(RegisterRequest request)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var host = Environment.GetEnvironmentVariable("BACKEND_HOST");
            using var channel = GrpcChannel.ForAddress("http://" + host + ":5000");
            var client = new Job.JobClient(channel);
            var response = await client.RegisterAsync(request);
            var processingResult = client.GetProcessingResult(response);
            return View("TextDetailsView", new TextDetailsViewModel { Status = processingResult.Status, Value = processingResult.Value });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
