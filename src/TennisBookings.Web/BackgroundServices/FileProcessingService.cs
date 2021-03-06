﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TennisBookings.ResultsProcessing;

namespace TennisBookings.Web.BackgroundServices
{
    public class FileProcessingService : BackgroundService
    {
        private readonly ILogger<FileProcessingService> _logger;
        private readonly FileProcessingChannel _fileProcessingChannel;
        private readonly IServiceProvider _serviceProvider;

        public FileProcessingService(
            ILogger<FileProcessingService> logger, 
            FileProcessingChannel fileProcessingChannel, 
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _fileProcessingChannel = fileProcessingChannel;
            _serviceProvider = serviceProvider;
        }
        internal static class EventIds
        {
            public static readonly EventId StartedProcessing = new EventId(100, "StartedProcessing");
            public static readonly EventId ProcessorStopping = new EventId(101, "ProcessorStopping");
            public static readonly EventId StoppedProcessing = new EventId(102, "StoppedProcessing");
            public static readonly EventId ProcessedMessage = new EventId(110, "ProcessedMessage");
        }

        private static readonly Action<ILogger, string, Exception> _processedMessage = LoggerMessage.Define<string>(
            LogLevel.Debug,
            EventIds.ProcessedMessage,
            "Read and processed message with ID '{MessageId}' from the channel.");

        public static void StartedProcessing(ILogger logger) => logger.Log(LogLevel.Trace, EventIds.StartedProcessing, "Started message processing service.");

        public static void ProcessorStopping(ILogger logger) => logger.Log(LogLevel.Information, EventIds.ProcessorStopping, "Message processing stopping due to app termination!");

        public static void StoppedProcessing(ILogger logger) => logger.Log(LogLevel.Trace, EventIds.StoppedProcessing, "Stopped message processing service.");

        public static void ProcessedMessage(ILogger logger, string messageId) => _processedMessage(logger, messageId, null);
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            /*
             * using await Task.Yield() will force your method to be asynchronous,
             * and return control at that point.
             *
             * The rest of the code will execute at a later time
             * (at which point, it still may run synchronously) on the current context.
             */
            await foreach (var fileName in _fileProcessingChannel.ReadAllAsync(stoppingToken))
            {
                using var scope = _serviceProvider.CreateScope();

                var processor = scope.ServiceProvider.GetRequiredService<IResultProcessor>();

                try
                {
                    await using var stream = File.OpenRead(fileName);
                    await processor.ProcessAsync(stream, stoppingToken);
                }
                finally
                {
                    File.Delete(fileName); // Delete the temp file
                }
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            await base.StopAsync(cancellationToken);

            _logger.LogInformation("completed shutdown in {Milliseconds} ms", sw.ElapsedMilliseconds);
        }
    }
}
