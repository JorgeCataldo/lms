using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Api.Configuration;
using Api.Extensions;
using Api.Middlewares;
using Api.Scheduler;
using Domain.Aggregates.Modules.Commands;
using Domain.Data;
using Domain.IdentityStores.Settings;
using Domain.Options;
using FluentValidation.AspNetCore;
using GoalLeague.Api.Middlewares;
using Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Tg4.Infrastructure.Email;
using Tg4.Infrastructure.Email.Data;
using Tg4.Infrastructure.Email.Extensions;
using Tg4.Infrastructure.Email.MongoDb;
using Tg4.Infrastructure.Email.Providers;
using Tg4.Infrastructure.Email.Services;
using Tg4.Infrastructure.Web.Upload;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc(config =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    config.Filters.Add(new AuthorizeFilter(policy));
                })
                .AddFluentValidation()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddScoped<IDbContext, DbContext>();
            services.AddScoped<IEmailRepository, EmailRepository>();
            services.AddScoped<ITokenGenerator, TokenGenerator>();

            services.Configure<SendGridOptions>(Configuration.GetSection("SendGridAuthentication"));
            services.Configure<MongoEmailSettings>(Configuration.GetSection("MongoEmailSettings"));
            services.Configure<MongoSettings>(Configuration.GetSection("MongoConnection"));
            services.Configure<DomainOptions>(Configuration.GetSection("DomainOptions"));
            services.Configure<BaseUrlsOption>(Configuration.GetSection("UploadSettings"));
            services.AddTransient<IEmailProvider, SendGridProvider>();
            services.AddEmailServices(Configuration);


            services.AddMediatR(typeof(AddModuleCommand).GetTypeInfo().Assembly);

            // MongoConfigurator.InitializeDb(Configuration.GetSection("MongoConnection:ConnectionString").Value,
            //    Configuration.GetSection("MongoConnection:Database").Value);

            // BsonSerializer.RegisterSerializer(new EnumSerializer<CategoryEnum>(BsonType.String));

            services.AddAuthenticationServices(Configuration);

            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, ScheduleTask>();

            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = ctx => {
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
                },
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot")),
                RequestPath = new PathString("")
            });

            app.UseAuthentication();

            app.UseCors(builder => builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin());

            MongoConfigurator.Initialize();

            app.UseImpersonate();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMvc();
        }
    }
}
