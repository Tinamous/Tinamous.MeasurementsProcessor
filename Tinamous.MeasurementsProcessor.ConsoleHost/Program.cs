using System;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Tinamous.MeasurementsProcessor.Aws;
using Tinamous.MeasurementsProcessor.Aws.Kinesis;
using Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces;
using Tinamous.MeasurementsProcessor.DAL.DynamoDB.Repository;
using Tinamous.MeasurementsProcessor.DAL.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Settings;
using Tinamous.MeasurementsProcessor.Services;
using Tinamous.MeasurementsProcessor.Services.Interfaces;
using Tinamous.MeasurementsProcessor.Services.Mapping;
using Tinamous.MeasurementsProcessor.Services.RecordProcessors;

namespace Tinamous.MeasurementsProcessor.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var environment = "Development";

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment}.json", true, true)
                .Build();

            var awsConfig = config.GetSection("Aws");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("L:/LogFiles/Tinamous.MeasurementsProcessor/{Date}.txt")
                .WriteTo.Console()
                .CreateLogger();

            var mapperConfiguration = AutoMapperConfiguration.Configure();
            IMapper mapper = mapperConfiguration.CreateMapper();

            IServiceCollection serviceCollection = new ServiceCollection()
                .AddLogging()
                .AddTransient<IBusFactory, BusFactory>()
                .AddTransient<IMembershipService, MembershipService>()
                .AddTransient<IHeartbeatService, HeartbeatService>()
                .AddTransient<IAwsClientFactory, AwsClientFactory>()
                .AddTransient<IAwsKinesisFactory, AwsKinesisFactory>()
                .AddTransient<IRecordProcessorFactory, RecordProcessorFactory>()
                .AddTransient<ICheckpointRepository, CheckpointRepository>()
                .AddTransient<IMeasurementUserPropertiesRepository, MeasurementUserPropertiesRepository>()
                .AddTransient<IMembershipService, MembershipService>()
                //.AddTransient<>()
                .AddSingleton<IMapper>(mapper)

                .AddTransient<IStreamProcessor, StreamProcessor>()
                .Configure<MessagingSettings>(config.GetSection("Messaging"))
                .Configure<AwsSettings>(config.GetSection("Aws"))
                .Configure<ServerSettings>(config.GetSection("Service"));


            serviceCollection.AddLogging(configure => configure.AddSerilog());
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<Program>>();
            serviceCollection.AddSingleton<Microsoft.Extensions.Logging.ILogger>(logger);

            serviceProvider = serviceCollection.BuildServiceProvider();

            //serviceProvider
            //    .GetService<ILoggingBuilder>()
            //    .AddConsole();

            using (IStreamProcessor streamProcessor = serviceProvider.GetService<IStreamProcessor>())
            {
                streamProcessor.CreateTables().Wait();
                streamProcessor.CreateKinesisStreamsAsync().Wait();
                streamProcessor.SetupProcessors();
                streamProcessor.SetupEventWatchers();

                Console.WriteLine("Hello World!");
                Console.ReadLine();
            }
        }
    }
}
