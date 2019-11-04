using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Dtos;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Events;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Documents;
using Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails;
using Tinamous.MeasurementsProcessor.Services.Interfaces;
using Tinamous.MeasurementsProcessor.Services.Messaging.EventHandlers;

namespace Tinamous.MeasurementsProcessor.Services.Tests.Messaging.EventHandlers
{
    [TestFixture]
    public class NewDecimalMeasurementProcessorTest
    {
        [Test]
        public async Task OnMessage_ProcessesMessage_PublishesOnKinesisStream()
        {
            // Arrange
            Guid deviceId = new Guid("FC6B39AF-352A-42F3-A848-45AA1FA08183");

            var bus = new Mock<IBus>();
            var streamWriter= new Mock<IRawMeasurementStreamWriter>();
            var logger = new Mock<ILogger>();

            var device = new User { UserId = deviceId };
            device.FieldDescriptors.Add(new FieldDescriptor { Name = "temp", Visible = true });

            var message = new NewMeasurementEvent
            {
                UserId = deviceId,
                MeasurementFields = new List<MeasurementFieldDto>
                {
                    new MeasurementFieldDto {Name = "temp", Value = 23.5M}
                }
            };

            streamWriter
                .Setup(x => x.PushStreamAsync(message))
                .Returns(Task.CompletedTask);

            NewDecimalMeasurementProcessor measurementProcessor = new NewDecimalMeasurementProcessor(bus.Object, streamWriter.Object, logger.Object);

            // Act
            await measurementProcessor.OnNewMeasurementAsync(message);

            // Assert
            streamWriter.Verify(x=> x.PushStreamAsync(message));
        }
    }
}