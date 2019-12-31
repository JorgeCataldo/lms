using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Api;
using Domain.Data;
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Xunit.Sdk;

namespace Test.Domain
{
    public class ContainerFixture
    {
        private static readonly Checkpoint _checkpoint;
        private static readonly IServiceProvider _rootContainer;
        private static readonly IServiceScopeFactory _scopeFactory;
        private static readonly IConfiguration _configuration;

        public static IConfiguration Configuration { get; }

        static ContainerFixture()
        {
            var host = A.Fake<IHostingEnvironment>();

            A.CallTo(() => host.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var startup = new Startup(Configuration);
            _configuration = startup.Configuration;
            var services = new ServiceCollection();
            startup.ConfigureServices(services);
            _rootContainer = services.BuildServiceProvider();
            _scopeFactory = _rootContainer.GetService<IServiceScopeFactory>();
            _checkpoint = new Checkpoint();
        }

        public static void ResetCheckpoint()
        {
            _checkpoint.Reset(_configuration["Data:DefaultConnection:ConnectionString"]);
        }

        public static async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<IDbContext>();

                try
                {
                    await action(scope.ServiceProvider);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public static async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<IDbContext>();

                try
                {
                    return await action(scope.ServiceProvider);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public static Task ExecuteDbContextAsync(Func<IDbContext, Task> action)
        {
            return ExecuteScopeAsync(sp => action(sp.GetService<IDbContext>()));
        }

        public static Task<T> ExecuteDbContextAsync<T>(Func<IDbContext, Task<T>> action)
        {
            return ExecuteScopeAsync(sp => action(sp.GetService<IDbContext>()));
        }
    }
}
