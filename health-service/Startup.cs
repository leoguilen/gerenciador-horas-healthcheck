using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HealthChecks.UI;
using HealthChecks.Uris;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using health_service.Models;
using health_service.Extensions;

namespace health_service
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
            // var dadosDependencias = new List<Dependency>();

            // new ConfigureFromConfigurationOptions<List<Dependency>>(
            //     Configuration.GetSection("Dependencies"))
            //     .Configure(dadosDependencias);
            // dadosDependencias = dadosDependencias.OrderBy(d => d.Name).ToList();

            // services.AddHealthChecks()
            //     .AddDependencies(dadosDependencias);
            // services.AddHealthChecksUI();

            services.AddHealthChecks()
                .AddUrlGroup(new Uri("http://httpbin.org/status/200"),"url-ok");
            services.AddHealthChecksUI();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Retornar status em Json
            app.UseHealthChecks("/status",
               new HealthCheckOptions()
               {
                   ResponseWriter = async (context, report) =>
                   {
                       var result = JsonConvert.SerializeObject(
                           new
                           {
                               statusApplication = report.Status.ToString(),
                               healthChecks = report.Entries.Select(e => new
                               {
                                   check = e.Key,
                                   ErrorMessage = e.Value.Exception?.Message,
                                   status = Enum.GetName(typeof(HealthStatus), e.Value.Status)
                               })
                           });
                       context.Response.ContentType = MediaTypeNames.Application.Json;
                       await context.Response.WriteAsync(result);
                   }
                });

            // Gera o endpoint para acessar o dashboard do health check
            app.UseHealthChecks("/health-status", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            // Ativa o dashboard para a visualização da situação de cada Health Check
            app.UseHealthChecksUI(setup => {
                setup.ApiPath = "/health-status";
            });
        }
    }
}
