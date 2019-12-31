using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;
using Serilog;
using Serilog.Context;
using Tg4.Infrastructure.Web.Message;

namespace GoalLeague.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            try
            {
                using (LogContext.PushProperty("EventId", Guid.NewGuid()))
                {
                    await _next.Invoke(context);
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exc)
        {
            if (exc is AggregateException)
                exc = exc.InnerException;

            try
            {
                Log.ForContext("Data", new
                {
                    Data = DateTime.Now,
                    EnvironmentMachineName = Environment.MachineName,
                    EnvironmentUserName = Environment.UserName,
                    EnvironmentUserDomainName = Environment.UserDomainName,
                    EnvironmentVersion = Environment.Version.ToString(),
                    EnvironmentProcessorCount = Environment.ProcessorCount,
                    EnvironmentWorkingSet = Environment.WorkingSet,
                    RequestUrl = context.Request.GetDisplayUrl(),
                    Exception = exc
                }, true)
                    .Error("Ocorreu um erro processando o request: {@exception}", exc);

            }
            catch (Exception err)
            {
                Log.Error("Erro ao registar o log @erro", err);
            }

            SentrySdk.ConfigureScope(scope => {
                if (context.User.Claims != null && context.User.Claims.Count() > 0)
                {
                    scope.User = new User {
                        Id = context.User.Claims.First(x =>
                            x.Type == ClaimTypes.NameIdentifier
                        ).Value
                    };
                }
            });
            SentrySdk.CaptureException(exc);

            var response = RestResult.Fail($"Ocorreu um erro interno = {exc.Message}");
            var resultJson = JsonConvert.SerializeObject(response);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync(resultJson);
        }
    }
}
