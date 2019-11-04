using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tinamous.MeasurementsProcessor.Aws;
using Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces;
using Tinamous.MeasurementsProcessor.DAL.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Settings;
using Tinamous.MeasurementsProcessor.Services.Interfaces;
using Tinamous.MeasurementsProcessor.Services.Messaging.EventHandlers;
using Tinamous.MeasurementsProcessor.Services.RecordProcessors;

namespace Tinamous.MeasurementsProcessor.Services
{
    public class StreamProcessor : IStreamProcessor
    {
        private readonly List<IDisposable> _handlers = new List<IDisposable>();
        private readonly IBus _eventBus;
        private readonly ICheckpointRepository _checkpointRepository;
        private readonly IKinesisStreamReader _rawMeasurementsStreamReader;
        private readonly IKinesisStreamReader _processedMeasurementsStreamReader;
        private readonly IAwsKinesisFactory _kinesisFactory;
        private readonly IMapper _mapper;
        private readonly ILogger<StreamProcessor> _logger;
        private readonly IMembershipService _membershipService;
        private readonly ServerSettings _serverSettings;

        public StreamProcessor(IBusFactory busFactory,
            IAwsClientFactory awsClientFactory,
            IAwsKinesisFactory awsKinesisFactory,
            IOptions<AwsSettings> awsOptions,
            IOptions<ServerSettings> serverOptions,
            IMapper mapper,
            ILogger<StreamProcessor> logger,
            IMembershipService membershipService,
            ICheckpointRepository checkpointRepository)
        {
            if (busFactory == null) throw new ArgumentNullException(nameof(busFactory));
            if (awsClientFactory == null) throw new ArgumentNullException("awsClientFactory");
            if (awsOptions == null) throw new ArgumentNullException(nameof(awsOptions));
            if (serverOptions == null) throw new ArgumentNullException(nameof(serverOptions));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            _eventBus = busFactory.CreateEventsBus();
            _kinesisFactory = awsKinesisFactory;
            _mapper = mapper;
            _logger = logger;
            _membershipService = membershipService;
            _serverSettings = serverOptions.Value;

            _checkpointRepository = checkpointRepository;

            var processedMeasurementStreamWriter = _kinesisFactory.CreateWriter();

            var recordProcessingFactory = new RecordProcessorFactory(membershipService, logger);

            _rawMeasurementsStreamReader = _kinesisFactory.CreateReader(
                recordProcessingFactory, 
                _checkpointRepository, 
                processedMeasurementStreamWriter);

            // Handle processed measurements. 
            // Read from the processed measurements stream and publish out to 
            // EasyNetQ/RabbitQ. This should be removed once
            // all other services (Web/MQTT/Notifier/Membership) are getting measurements
            // from the processed stream rather than the queue
            _processedMeasurementsStreamReader = _kinesisFactory.CreateProcesssedMeasurementsReader(
                _checkpointRepository, 
                _eventBus);
        }

        public async Task CreateTables()
        {
            try
            {
                await _checkpointRepository.CreateTableAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating streaming tables...");
                throw;
            }
        }

        public async Task CreateKinesisStreamsAsync()
        {
            var streamCreator = _kinesisFactory.CreateCreator();
            await streamCreator.CreateStreamsAsync();
        }

        public void SetupProcessors()
        {
            _logger.LogWarning("Starting Kinesis processors.");
            _rawMeasurementsStreamReader.Start();
            _processedMeasurementsStreamReader.Start();
            //_monthlyProcessedMeasurementsStreamReader.Start();
        }

        public void SetupEventWatchers()
        {
            IOptions<ServerSettings> serverOptions = Options.Create(_serverSettings);

            // Membership
            AddHandler(new UserUpdatedEventHandler(_eventBus, _membershipService, _mapper, serverOptions, _logger));
            // Shared between services to update the DB - NOTE: Also shared with Measurements service for now.
            //AddHandler(new UserUpdatedEventSharedHandler(_eventBus, _membershipService, _mapper));

            AddHandler(new UserCurrentLocationChangedEventHandler(_eventBus, _membershipService, _mapper, serverOptions, _logger));
            // Shared between services to update the DB - NOTE: Also shared with Measurements service for now.
            //AddHandler(new UserCurrentLocationChangedEventSharedHandler(_eventBus, _membershipService, _mapper));

            AddHandler(new MemberDeletedEventHandler(_eventBus, _membershipService, _logger));
            AddHandler(new DeviceDeletedEventHandler(_eventBus, _membershipService, _logger));

            // Heartbeats
            AddHandler(new PingEventHandler(_eventBus, serverOptions, _logger));

            // Measurements (old style)
            // Responsible for processing new measurements as they are received
            // and then publishing for persistence.
            var streamWriter = _kinesisFactory.CreateRawMeasurementStreamWriter();
            AddHandler(new NewDecimalMeasurementProcessor(_eventBus, streamWriter, _logger));
        }

        private void AddHandler(IDisposable disposable)
        {
            _handlers.Add(disposable);
        }

        public void Dispose()
        {
            foreach (var disposable in _handlers)
            {
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            _handlers.Clear();

            if (_rawMeasurementsStreamReader != null)
            {
                _rawMeasurementsStreamReader.Stop();
                _rawMeasurementsStreamReader.Dispose();
            }

            if (_processedMeasurementsStreamReader != null)
            {
                _processedMeasurementsStreamReader.Stop();
                _processedMeasurementsStreamReader.Dispose();
            }

            _eventBus?.Dispose();
        }
    }
}