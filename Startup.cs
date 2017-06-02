using System;
using System.Collections.Generic;
using System.IO;
using core.configuration;
using core.plugin.engine;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoleApplication{

public class Startup{

 public static JsonConfiguration jsonConf;

 public static ILoggerFactory loggerFactory;
               // This method gets called by the runtime. Use this method to add services to the container.

 public void Configure(IApplicationBuilder app)
  {
            // ici nous allons ouvrire le fichier de configuration

            try{
               jsonConf = new JsonConfiguration();

            }catch(Exception exc){
                Console.WriteLine(exc.Message);
            }

            app.UseMvcWithDefaultRoute();
             // ********************
            // USE CORS - might not be required.
            // ********************
            app.UseCors("SiteCorsPolicy");
}

public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
             // ********************
            // Setup CORS
            // ********************
            var corsBuilder = new CorsPolicyBuilder();
            corsBuilder.AllowAnyHeader();
            corsBuilder.AllowAnyMethod();
            corsBuilder.AllowAnyOrigin();
            corsBuilder.AllowCredentials();

            services.AddCors(options =>
            {
                options.AddPolicy("SiteCorsPolicy", corsBuilder.Build());
            });

        }
}


}
