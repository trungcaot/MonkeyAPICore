using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Formatters;
using MonkeyAPICore.Infrastructure;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using MonkeyAPICore.Filters;
using MonkeyAPICore.Models;
using Microsoft.EntityFrameworkCore;
using MonkeyAPICore.Services;
using AutoMapper;
using Newtonsoft.Json;

namespace MonkeyAPICore
{
    public class Startup
    {
        private readonly int? _httpsPort;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _httpsPort = configuration.GetValue<int>("iisSettings:iisExpress:sslPort");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Use an in-memory database for quick dev and testing.
            //TODO: Swap out with a real database in production
            services.AddDbContext<MonkeyAPIContext>(opt => opt.UseInMemoryDatabase());

            services.AddAutoMapper();

            services.AddMvc(opt => 
            {
                opt.Filters.Add(typeof(JsonExceptionFilter));
                opt.Filters.Add(typeof(LinkRewritingFilter));

                // Require HTTPS for all controller
                opt.SslPort = _httpsPort;
                opt.Filters.Add(typeof(RequireHttpsAttribute));

                var jsonFormatter = opt.OutputFormatters.OfType<JsonOutputFormatter>().Single();
                opt.OutputFormatters.Remove(jsonFormatter);

                opt.OutputFormatters.Add(new IonOutputFormatter(jsonFormatter));
            })
            .AddJsonOptions(opt =>
            {
                // These should be the defaults, but we can be explicit:
                opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                opt.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                opt.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
            });

            // config all api follow camel standard
            services.AddRouting(opt => opt.LowercaseUrls = true);

            services.AddApiVersioning(opt =>
            {
                opt.ApiVersionReader = new MediaTypeApiVersionReader();
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.ReportApiVersions = true;
                opt.DefaultApiVersion = new ApiVersion(1,0);
                opt.ApiVersionSelector = new CurrentImplementationApiVersionSelector(opt);
            });

            // Load resource hotelinfo from appsettings.json
            services.Configure<HotelOptions>(Configuration);
            services.Configure<HotelInfo>(Configuration.GetSection("Info"));
            services.Configure<PagingOptions>(Configuration.GetSection("DefaultPagingOptions"));

            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IOpeningService, OpeningService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IDateLogicService, DateLogicService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Add some test data in development
                var context = serviceProvider.GetService<MonkeyAPIContext>();
                var dateLogicService = serviceProvider.GetService<IDateLogicService>();
                AddTestData(context, dateLogicService);

            }

            app.UseHsts(opt =>
            {
                opt.MaxAge(days: 180);
                opt.IncludeSubdomains();
                opt.Preload();
            });
            app.UseMvc();
        }

        private static void AddTestData(MonkeyAPIContext context,
            IDateLogicService dateLogicService)
        {
            var oxford = context.Rooms.Add(new RoomEntity
            {
                Id = Guid.Parse("301df04d-8679-4b1b-ab92-0a586ae53d08"),
                Name = "Oxford Suite",
                Rate = 10119,
            }).Entity;

            context.Rooms.Add(new RoomEntity
            {
                Id = Guid.Parse("ee2b83be-91db-4de5-8122-35a9e9195976"),
                Name = "Driscoll Suite",
                Rate = 23959
            });

            var today = DateTimeOffset.Now;
            var start = dateLogicService.AlignStartTime(today);
            var end = start.Add(dateLogicService.GetMinimumStay());

            context.Bookings.Add(new BookingEntity
            {
                Id = Guid.Parse("2eac8dea-2749-42b3-9d21-8eb2fc0fd6bd"),
                Room = oxford,
                CreatedAt = DateTimeOffset.UtcNow,
                StartAt = start,
                EndAt = end,
                Total = oxford.Rate,
            });

            context.SaveChanges();
        }
    }
}
