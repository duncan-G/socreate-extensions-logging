﻿using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoCreate.Extensions.Logging;
using SoCreate.Extensions.Logging.ActivityLogger;

namespace ActivityLogger
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            using (var host = CreateHost())
            {
                var activityLogger = host.Services.GetService<IActivityLogger<ExampleActionType>>();

                var randomId = new Random((int)DateTime.Now.ToOADate()).Next();
                // use the activity logger directly
                activityLogger.LogActivity(ExampleActionType.Important,
                    randomId,
                    ExampleKeyTypeEnum.OrderId,
                    1,
                    new AdditionalData(("Extra", "Data"), ("MoreExtra", "Data2")), "Logging Activity with Message: {Structure}", "This is more information");

                activityLogger.LogActivity(ExampleActionType.Important,
                    randomId,
                    ExampleKeyTypeEnum.OrderId,
                    "This is without account {Key} or additional data");

                // use the activity logger extensions
                activityLogger.LogSomeData(51, "This is the extension method");
                host.Run();
            }
        }

        private static IHost CreateHost() =>
            Host.CreateDefaultBuilder()
                .UseEnvironment(Environment.GetEnvironmentVariable("App_Environment") ?? "Production")
                .ConfigureWebHostDefaults(config =>
                {
                    config.ConfigureLogging((hostingContext, builder) =>
                        builder.AddServiceLogging(hostingContext, new LoggerOptions
                            {
                                SendLogDataToApplicationInsights = true,
                                SendLogActivityDataToSql = false
                            },
                            new ActivityLoggerFunctionOptions
                            {
                                GetTenantId = () => 100,
                                GetAccountId = ( key, keyType, accountId) => 
                                    accountId ?? (Enum.Parse<ExampleKeyTypeEnum>(keyType) == ExampleKeyTypeEnum.NoteId ? 3 : 4)
                            }));
                    config.UseStartup<Startup>();
                })
                .Build();
    }
}