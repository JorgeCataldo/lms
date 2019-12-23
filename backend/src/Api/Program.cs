using System;
using System.Net;
using Domain.Aggregates.Report.Queries;
using Domain.ECommerceIntegration.PagarMe;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Sentry;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (SentrySdk.Init("https://e283586327cf496c99305947c29013b5@sentry.io/1372101"))
            {
                CreateWebHostBuilder(args).Build().Run();
            }

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options =>
                {
                    var port = 5055;
                    if (args.Length > 0)
                        int.TryParse(args[0], out port);
                    options.Listen(IPAddress.Loopback, port); //HTTP port
                })
                .UseStartup<Startup>();
    }
}
