using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Dtos;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Events;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Tinamous.MeasurementsProcessor.Domain.Documents;
using Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails;
using Tinamous.MeasurementsProcessor.Services.Interfaces;
using Tinamous.MeasurementsProcessor.Services.Processors;

namespace Tinamous.MeasurementsProcessor.Services.Tests.Processors
{
    [TestFixture]
    public class NewMeasurementRecordProcessorTest
    {
        // Below are for measurement processing and should be move to the 
        // measurement processor test...

        public class ForFieldWithCalibration
        {
            [Test]
            public async Task OnMessage_ForFieldWithCalibration_AppliesCalibration()
            {
                // Arrange
                Guid accountId = new Guid("D1CB14F5-1376-4F82-B4B7-1F2A36639DE4");
                Guid deviceId = new Guid("8D37AD07-AB31-43E0-BCFD-B8E9C339F7EA");

                var membershipService = new Mock<IMembershipService>();
                var logger = new Mock<ILogger>();

                User device = new User
                {
                    AccountId = accountId,
                    UserId = deviceId,
                    FieldDescriptors = new List<FieldDescriptor>
                    {
                        new FieldDescriptor
                        {
                            Channel = 0,
                            Name = "ToBeCalibrated",
                            Calibration = new FieldCalibration
                            {
                                Enabled = true,
                                Offset = 10,
                                Slope = 100
                            }
                        }
                    }
                };

                membershipService
                    .Setup(x => x.LoadAsync(accountId, deviceId, false))
                    .Returns(Task.FromResult(device));

                var measurementProcessor = new NewMeasurementRecordProcessor(membershipService.Object, logger.Object);

                NewMeasurementEvent measurementEvent = new NewMeasurementEvent
                {
                    Id = new Guid("0D696E5F-F525-43EE-9536-B11F94D8547B"),
                    AccountId = accountId,
                    UserId = deviceId,
                    Channel = 0,
                    MeasurementFields = new List<MeasurementFieldDto>
                    {
                        new MeasurementFieldDto
                        {
                            Value = 2.34M,
                            Name = "ToBeCalibrated"
                        }
                    }
                };

                // Act
                var processedMeasurement = await measurementProcessor.ProcessAsync(measurementEvent);

                // Assert
                Assert.NotNull(processedMeasurement);
                Assert.NotNull(processedMeasurement.MeasurementFields);
                Assert.AreEqual(1, processedMeasurement.MeasurementFields.Count);

                var field = processedMeasurement.MeasurementFields[0];
                Assert.IsTrue(field.IsCalibrated);

                decimal originalValue = 2.34M;
                decimal slope = 100;
                decimal offset = 10;
                decimal expected = slope * originalValue + offset;
                // 100 * 2.34 = 234
                // + 10 = 244

                // Check the calibration is applied correctly
                Assert.AreEqual(244M, field.Value);

                // Check original value stored in raw value
                Assert.AreEqual(2.34M, field.RawValue);
            }
        }

        public class ForFieldWithoutCalibration
        {
            [Test]
            public async Task OnMessage_ForFieldWitoutCalibration_DoesNotApplyCalibration()
            {
                // Arrange
                Guid accountId = new Guid("D1CB14F5-1376-4F82-B4B7-1F2A36639DE4");
                Guid deviceId = new Guid("8D37AD07-AB31-43E0-BCFD-B8E9C339F7EA");

                var membershipService = new Mock<IMembershipService>();
                var logger = new Mock<ILogger>();

                User device = new User
                {
                    AccountId = accountId,
                    UserId = deviceId,
                    FieldDescriptors = new List<FieldDescriptor>
                    {
                        new FieldDescriptor
                        {
                            Channel = 0,
                            Name = "Uncalibrated",
                            Calibration = null
                        }
                    }
                };

                membershipService
                    .Setup(x => x.LoadAsync(accountId, deviceId, false))
                    .Returns(Task.FromResult(device));

                var measurementProcessor = new NewMeasurementRecordProcessor(membershipService.Object, logger.Object);

                NewMeasurementEvent measurementEvent = new NewMeasurementEvent
                {
                    Id = new Guid("0D696E5F-F525-43EE-9536-B11F94D8547B"),
                    UserId = deviceId,
                    AccountId = accountId,
                    Channel = 0,
                    MeasurementFields = new List<MeasurementFieldDto>
                    {
                        new MeasurementFieldDto
                        {
                            Value = 2.34M,
                            Name = "ToBeCalibrated"
                        }
                    }
                };

                // Act
                var processedMeasurement = await measurementProcessor.ProcessAsync(measurementEvent);

                // Assert
                Assert.NotNull(processedMeasurement);
                Assert.NotNull(processedMeasurement.MeasurementFields);
                Assert.AreEqual(1, processedMeasurement.MeasurementFields.Count);

                var field = processedMeasurement.MeasurementFields[0];
                Assert.IsFalse(field.IsCalibrated);

                // Value and Raw value should match (no calibration applied)
                Assert.AreEqual(2.34M, field.Value);
                Assert.AreEqual(2.34M, field.RawValue);
            }
        }

        public class WithLocationDetails
        {
            [Test]
            [TestCase("lat", "long")]
            [TestCase("Lat", "Long")]
            [TestCase("latitude", "longitude")]
            [TestCase("Latitude", "Longitude")]
            public async Task WithLatAndLongSeperateFields_SetsMeasurementLocation(string latFieldName,
                string longFieldName)
            {
                // Arrange
                Guid deviceId = new Guid("FC6B39AF-352A-42F3-A848-45AA1FA08183");
                Guid accountId = new Guid("D1CB14F5-1376-4F82-B4B7-1F2A36639DE4");

                var membershipService = new Mock<IMembershipService>();
                var logger = new Mock<ILogger>();

                var device = new User { AccountId = accountId, UserId = deviceId };
                device.FieldDescriptors.Add(new FieldDescriptor { Name = "temp", Visible = true });
                device.FieldDescriptors.Add(new FieldDescriptor { Name = latFieldName, Visible = false });
                device.FieldDescriptors.Add(new FieldDescriptor { Name = longFieldName, Visible = false });

                membershipService
                    .Setup(x => x.LoadAsync(accountId, deviceId, false))
                    .Returns(Task.FromResult(device));

                var measurementProcessor = new NewMeasurementRecordProcessor(membershipService.Object, logger.Object);

                var measurementEvent = new NewMeasurementEvent
                {
                    UserId = deviceId,
                    AccountId = accountId,
                    MeasurementFields = new List<MeasurementFieldDto>
                    {
                        new MeasurementFieldDto {Name = "temp", Value = 23.5M},
                        new MeasurementFieldDto {Name = latFieldName, Value = 1.2345M},
                        new MeasurementFieldDto {Name = longFieldName, Value = 2.5876M},
                    }
                };

                // Act
                var processedMeasurement = await measurementProcessor.ProcessAsync(measurementEvent);

                // Assert
                Assert.NotNull(processedMeasurement);

                Assert.NotNull(processedMeasurement.MeasurementFields);
                Assert.AreEqual(3, processedMeasurement.MeasurementFields.Count);

                Assert.NotNull(processedMeasurement.Location);
                Assert.AreEqual(1.2346D, processedMeasurement.Location.Latitude, 0.0001D, "Latitude");
                Assert.AreEqual(2.5876D, processedMeasurement.Location.Longitude, 0.0001D, "Longitude");
                Assert.AreEqual(0.0D, processedMeasurement.Location.Elevation, 0.0001D, "Elevation");
                Assert.AreEqual(Guid.Empty, processedMeasurement.Location.Id, "Id");

                Assert.IsNull(processedMeasurement.Location.Name);
            }

            [Test]
            [TestCase("latlong")]
            [TestCase("LatLong")]
            [TestCase("g")]
            [TestCase("gps")]
            public async Task WithLatAndLongSingleField_SetsMeasurementLocation(string fieldName)
            {
                // Arrange
                Guid deviceId = new Guid("FC6B39AF-352A-42F3-A848-45AA1FA08183");
                Guid accountId = new Guid("D1CB14F5-1376-4F82-B4B7-1F2A36639DE4");

                var membershipService = new Mock<IMembershipService>();
                var logger = new Mock<ILogger>();

                var device = new User { AccountId = accountId, UserId = deviceId };
                device.FieldDescriptors.Add(new FieldDescriptor { Name = "temp", Visible = true });
                device.FieldDescriptors.Add(new FieldDescriptor { Name = fieldName, Visible = false });

                membershipService
                    .Setup(x => x.LoadAsync(accountId, deviceId, false))
                    .Returns(Task.FromResult(device));

                var measurementProcessor = new NewMeasurementRecordProcessor(membershipService.Object, logger.Object);

                var measurementEvent = new NewMeasurementEvent
                {
                    UserId = deviceId,
                    AccountId = accountId,
                    MeasurementFields = new List<MeasurementFieldDto>
                    {
                        new MeasurementFieldDto {Name = "temp", Value = 23.5M},
                        new MeasurementFieldDto {Name = fieldName, StringValue = "1.2345,2.5876"},
                    }
                };

                // Act
                var processedMeasurement = await measurementProcessor.ProcessAsync(measurementEvent);

                // Assert
                Assert.NotNull(processedMeasurement);

                Assert.NotNull(processedMeasurement.MeasurementFields);
                Assert.AreEqual(2, processedMeasurement.MeasurementFields.Count);

                Assert.NotNull(processedMeasurement.Location);
                Assert.AreEqual(1.2346D, processedMeasurement.Location.Latitude, 0.0001D, "Latitude");
                Assert.AreEqual(2.5876D, processedMeasurement.Location.Longitude, 0.0001D, "Longitude");
                Assert.AreEqual(0.0D, processedMeasurement.Location.Elevation, 0.0001D, "Elevation");
                Assert.AreEqual(Guid.Empty, processedMeasurement.Location.Id, "Id");

                Assert.IsNull(processedMeasurement.Location.Name);
            }

            [Test]
            [TestCase("latlong", "1.23")]
            [TestCase("latlong", "1.23,")] // is this 0?
            [TestCase("latlong", ",12")] // Is this 0?
            [TestCase("latlong", "Not a location")]
            [TestCase("latlong", "1' 15\",2'22\"")] // degrees and minutes format.
            public async Task WithInvalidLatAndLong_DoesNotSetLocation(string fieldName, string locationValue)
            {
                // Arrange
                Guid deviceId = new Guid("FC6B39AF-352A-42F3-A848-45AA1FA08183");
                Guid accountId = new Guid("D1CB14F5-1376-4F82-B4B7-1F2A36639DE4");

                var membershipService = new Mock<IMembershipService>();
                var logger = new Mock<ILogger>();

                var device = new User { AccountId = accountId, UserId = deviceId };
                device.FieldDescriptors.Add(new FieldDescriptor { Name = "temp", Visible = true });
                device.FieldDescriptors.Add(new FieldDescriptor { Name = fieldName, Visible = false });

                membershipService
                    .Setup(x => x.LoadAsync(accountId, deviceId, false))
                    .Returns(Task.FromResult(device));

                var measurementProcessor = new NewMeasurementRecordProcessor(membershipService.Object, logger.Object);

                var measurementEvent = new NewMeasurementEvent
                {
                    UserId = deviceId,
                    AccountId = accountId,
                    MeasurementFields = new List<MeasurementFieldDto>
                    {
                        new MeasurementFieldDto {Name = "temp", Value = 23.5M},
                        new MeasurementFieldDto {Name = fieldName, StringValue = locationValue},
                    }
                };

                // Act
                var processedMeasurement = await measurementProcessor.ProcessAsync(measurementEvent);

                // Assert
                Assert.NotNull(processedMeasurement);

                Assert.NotNull(processedMeasurement.MeasurementFields);
                Assert.AreEqual(2, processedMeasurement.MeasurementFields.Count);

                Assert.IsNull(processedMeasurement.Location);
            }
        }

        public class WithTags
        {
            [Test]
            [TestCase("tags", "tag1, tag2, tag3", 3)] // Comma separated
            [TestCase("tags", "tag1 tag2 tag3", 3)] // Space separated
            [TestCase("tags", "tag1;tag2;tag3", 3)] // semi-colon separated
            [TestCase("tags", "tag1", 1)]
            [TestCase("tags", "", 0)]
            [TestCase("Tags", "UpperCase", 1)]
            public async Task ExtractsTagsFromTagsField(string fieldName, string value, int expectedTagCount)
            {
                // Arrange
                Guid deviceId = new Guid("FC6B39AF-352A-42F3-A848-45AA1FA08183");
                Guid accountId = new Guid("D1CB14F5-1376-4F82-B4B7-1F2A36639DE4");

                var membershipService = new Mock<IMembershipService>();
                var logger = new Mock<ILogger>();

                var device = new User { AccountId = accountId, UserId = deviceId };
                device.FieldDescriptors.Add(new FieldDescriptor { Name = "temp", Visible = true });
                device.FieldDescriptors.Add(new FieldDescriptor { Name = "tags", Visible = false });

                membershipService
                    .Setup(x => x.LoadAsync(accountId, deviceId, false))
                    .Returns(Task.FromResult(device));

                var measurementProcessor = new NewMeasurementRecordProcessor(membershipService.Object, logger.Object);

                var measurementEvent = new NewMeasurementEvent
                {
                    UserId = deviceId,
                    AccountId = accountId,
                    MeasurementFields = new List<MeasurementFieldDto>
                    {
                        new MeasurementFieldDto {Name = "temp", Value = 23.5M},
                        new MeasurementFieldDto {Name = fieldName, StringValue = value},
                    }
                };

                // Act
                var processedMeasurement = await measurementProcessor.ProcessAsync(measurementEvent);

                // Assert
                Assert.NotNull(processedMeasurement);

                Assert.NotNull(processedMeasurement.MeasurementFields);
                Assert.AreEqual(2, processedMeasurement.MeasurementFields.Count);

                Assert.IsNotNull(processedMeasurement.Tags);
                Assert.AreEqual(expectedTagCount, processedMeasurement.Tags.Count);
            }

            [Test]
            public async Task WhereMeasurementHasTags_CombinesTags()
            {
                // Arrange
                Guid deviceId = new Guid("FC6B39AF-352A-42F3-A848-45AA1FA08183");
                Guid accountId = new Guid("D1CB14F5-1376-4F82-B4B7-1F2A36639DE4");

                var membershipService = new Mock<IMembershipService>();
                var logger = new Mock<ILogger>();

                var device = new User { AccountId = accountId, UserId = deviceId };
                device.FieldDescriptors.Add(new FieldDescriptor { Name = "temp", Visible = true });
                device.FieldDescriptors.Add(new FieldDescriptor { Name = "tags", Visible = false });

                membershipService
                    .Setup(x => x.LoadAsync(accountId, deviceId, false))
                    .Returns(Task.FromResult(device));

                var measurementProcessor = new NewMeasurementRecordProcessor(membershipService.Object, logger.Object);

                var measurementEvent = new NewMeasurementEvent
                {
                    Tags = new List<string> { "Foo", "Bar" },
                    UserId = deviceId,
                    AccountId = accountId,
                    MeasurementFields = new List<MeasurementFieldDto>
                    {
                        new MeasurementFieldDto {Name = "temp", Value = 23.5M},
                        new MeasurementFieldDto {Name = "tags", StringValue = "Baz"},
                    }
                };

                // Act
                var processedMeasurement = await measurementProcessor.ProcessAsync(measurementEvent);

                // Assert
                Assert.NotNull(processedMeasurement);

                Assert.NotNull(processedMeasurement.MeasurementFields);
                Assert.AreEqual(2, processedMeasurement.MeasurementFields.Count);

                Assert.IsNotNull(processedMeasurement.Tags);
                Assert.AreEqual(3, processedMeasurement.Tags.Count);
            }

            [Test]
            public async Task WhereNoTagsExist_HasNonNullArray()
            {
                // Arrange
                Guid deviceId = new Guid("FC6B39AF-352A-42F3-A848-45AA1FA08183");
                Guid accountId = new Guid("D1CB14F5-1376-4F82-B4B7-1F2A36639DE4");

                var membershipService = new Mock<IMembershipService>();
                var logger = new Mock<ILogger>();

                var device = new User { AccountId = accountId, UserId = deviceId };
                device.FieldDescriptors.Add(new FieldDescriptor { Name = "temp", Visible = true });

                membershipService
                    .Setup(x => x.LoadAsync(accountId, deviceId, false))
                    .Returns(Task.FromResult(device));

                var measurementProcessor = new NewMeasurementRecordProcessor(membershipService.Object, logger.Object);

                var measurementEvent = new NewMeasurementEvent
                {
                    UserId = deviceId,
                    AccountId = accountId,
                    MeasurementFields = new List<MeasurementFieldDto>
                    {
                        // no tags, but same number of fields (to help test :-) )
                        new MeasurementFieldDto {Name = "temp", Value = 23.5M},
                        new MeasurementFieldDto {Name = "humidity", Value = 23.5M},
                    }
                };

                // Act
                var processedMeasurement = await measurementProcessor.ProcessAsync(measurementEvent);

                // Assert
                Assert.NotNull(processedMeasurement);

                Assert.NotNull(processedMeasurement.MeasurementFields);
                Assert.AreEqual(2, processedMeasurement.MeasurementFields.Count);

                Assert.IsNotNull(processedMeasurement.Tags);
                Assert.AreEqual(0, processedMeasurement.Tags.Count);
            }
        }

        public class WithSampleId
        {
            [Test]
            [TestCase("sampleid", "Sample-12")]
            [TestCase("sampleId", "Sample-12")]
            [TestCase("SampleId", "Sample-12")]
            [TestCase("SampleId", "12")]
            [TestCase("NoSampleId", null)] // diffierent field.
            public async Task ExtractsSampleIdFromField(string fieldName, string sampleId)
            {
                // Arrange
                Guid deviceId = new Guid("FC6B39AF-352A-42F3-A848-45AA1FA08183");
                Guid accountId = new Guid("D1CB14F5-1376-4F82-B4B7-1F2A36639DE4");

                var membershipService = new Mock<IMembershipService>();
                var logger = new Mock<ILogger>();

                var device = new User { AccountId = accountId, UserId = deviceId };
                device.FieldDescriptors.Add(new FieldDescriptor { Name = "temp", Visible = true });
                device.FieldDescriptors.Add(new FieldDescriptor { Name = "sampleid", Visible = false });

                membershipService
                    .Setup(x => x.LoadAsync(accountId, deviceId, false))
                    .Returns(Task.FromResult(device));

                var measurementProcessor = new NewMeasurementRecordProcessor(membershipService.Object, logger.Object);

                var measurementEvent = new NewMeasurementEvent
                {
                    UserId = deviceId,
                    AccountId = accountId,
                    MeasurementFields = new List<MeasurementFieldDto>
                    {
                        new MeasurementFieldDto {Name = "temp", Value = 23.5M},
                        new MeasurementFieldDto {Name = fieldName, StringValue = sampleId},
                    }
                };

                // Act
                var processedMeasurement = await measurementProcessor.ProcessAsync(measurementEvent);

                // Assert
                Assert.NotNull(processedMeasurement);

                Assert.NotNull(processedMeasurement.MeasurementFields);
                Assert.AreEqual(2, processedMeasurement.MeasurementFields.Count);

                // Ensure the sample id is as expected.
                Assert.AreEqual(sampleId, processedMeasurement.SampleId);
            }
        }

        public class WithOperatorId
        {
            [Test]
            [TestCase("operatorid", "Operator-12")]
            [TestCase("operatorId", "Operator-12")]
            [TestCase("OperatorId", "Operator-12")]
            [TestCase("OperatorId", "12")]
            [TestCase("NoOperatorId", null)] // diffierent field.
            public async Task ExtractsSampleIdFromField(string fieldName, string operatorId)
            {
                // Arrange
                Guid deviceId = new Guid("FC6B39AF-352A-42F3-A848-45AA1FA08183");
                Guid accountId = new Guid("D1CB14F5-1376-4F82-B4B7-1F2A36639DE4");

                var membershipService = new Mock<IMembershipService>();
                var logger = new Mock<ILogger>();

                var device = new User { AccountId = accountId, UserId = deviceId };
                device.FieldDescriptors.Add(new FieldDescriptor { Name = "temp", Visible = true });
                device.FieldDescriptors.Add(new FieldDescriptor { Name = "operatorid", Visible = false });

                membershipService
                    .Setup(x => x.LoadAsync(accountId, deviceId, false))
                    .Returns(Task.FromResult(device));

                var measurementProcessor = new NewMeasurementRecordProcessor(membershipService.Object, logger.Object);

                var measurementEvent = new NewMeasurementEvent
                {
                    UserId = deviceId,
                    AccountId = accountId,
                    MeasurementFields = new List<MeasurementFieldDto>
                    {
                        new MeasurementFieldDto {Name = "temp", Value = 23.5M},
                        new MeasurementFieldDto {Name = fieldName, StringValue = operatorId},
                    }
                };

                // Act
                var processedMeasurement = await measurementProcessor.ProcessAsync(measurementEvent);

                // Assert
                Assert.NotNull(processedMeasurement);

                Assert.NotNull(processedMeasurement.MeasurementFields);
                Assert.AreEqual(2, processedMeasurement.MeasurementFields.Count);

                // Ensure the sample id is as expected.
                Assert.AreEqual(operatorId, processedMeasurement.OperatorId);
            }
        }

        public class WithBatteryDetails
        {
            [Test]
            [TestCase("battery", 1.2D, 1)]
            [TestCase("Battery", 2.6D, 3)]
            [TestCase("batterylevel", 20.4D, 20)]
            [TestCase("BatteryLevel", 30.6D, 31)]
            // battery and battery level may need some computation to get them to be ints. May have only
            // a small range (see ps-soc on Photon power shield)
            [TestCase("BatteryLevelPercentage", 44.1D, 44)]
            [TestCase("batterylevelpercentage", 54.0D, 54)]
            public async Task ExtractsSampleIdFromField(string fieldName, double value, int expectedLevel)
            {
                // Arrange
                Guid deviceId = new Guid("FC6B39AF-352A-42F3-A848-45AA1FA08183");
                Guid accountId = new Guid("D1CB14F5-1376-4F82-B4B7-1F2A36639DE4");

                var membershipService = new Mock<IMembershipService>();
                var logger = new Mock<ILogger>();

                var device = new User { AccountId = accountId, UserId = deviceId };
                device.FieldDescriptors.Add(new FieldDescriptor { Name = "temp", Visible = true });
                device.FieldDescriptors.Add(new FieldDescriptor { Name = fieldName, Visible = false });

                membershipService
                    .Setup(x => x.LoadAsync(accountId, deviceId, false))
                    .Returns(Task.FromResult(device));

                var measurementProcessor = new NewMeasurementRecordProcessor(membershipService.Object, logger.Object);

                var measurementEvent = new NewMeasurementEvent
                {
                    UserId = deviceId,
                    AccountId = accountId,
                    MeasurementFields = new List<MeasurementFieldDto>
                    {
                        new MeasurementFieldDto {Name = "temp", Value = 23.5M},
                        new MeasurementFieldDto {Name = fieldName, Value = Convert.ToDecimal(value)},
                    }
                };

                // Act
                var processedMeasurement = await measurementProcessor.ProcessAsync(measurementEvent);

                // Assert
                Assert.NotNull(processedMeasurement);

                Assert.NotNull(processedMeasurement.MeasurementFields);
                Assert.AreEqual(2, processedMeasurement.MeasurementFields.Count);

                var doubleLevel = Convert.ToDouble(processedMeasurement.BatteryLevel);
                Assert.AreEqual(value, doubleLevel, 0.001D, "BatteryLevel");

                Assert.AreEqual(expectedLevel, processedMeasurement.BatteryLevelPercentage, "BatteryLevelPercentage");

                // Battery level message?
            }
        }

        public class WithRfStrengthDetails
        {
            // TODO...
        }

        public class WithOldApiFields
        {
            [Test]
            public async Task OnMessage_WithOldApiFields_ConvertsToNewFieldNames()
            {
                // Arrange
                Guid deviceId = new Guid("FC6B39AF-352A-42F3-A848-45AA1FA08183");
                Guid accountId = new Guid("D1CB14F5-1376-4F82-B4B7-1F2A36639DE4");

                var membershipService = new Mock<IMembershipService>();
                var logger = new Mock<ILogger>();

                var device = new User
                {
                    UserId = deviceId,
                    AccountId = accountId,
                    FieldDescriptors = new List<FieldDescriptor>
                    {
                        new FieldDescriptor
                        {
                            Index = 0,
                            Name = "Temperature",
                            Visible = true
                        }
                    }
                };

                membershipService
                    .Setup(x => x.LoadAsync(accountId, deviceId, false))
                    .Returns(Task.FromResult(device));

                var measurementProcessor = new NewMeasurementRecordProcessor(membershipService.Object, logger.Object);

                var measurementEvent = new NewMeasurementEvent
                {
                    UserId = deviceId,
                    AccountId = accountId,
                    MeasurementFields = new List<MeasurementFieldDto>
                    {
                        new MeasurementFieldDto
                        {
                            Name = "Field1",
                            StringValue = "23.6",
                            Value = 23.6M,
                        }
                    },
                    RemapOldFields = true
                };

                // Act
                var processedMeasurement = await measurementProcessor.ProcessAsync(measurementEvent);

                // Assert
                Assert.NotNull(processedMeasurement);

                Assert.NotNull(processedMeasurement.MeasurementFields);
                Assert.AreEqual(1, processedMeasurement.MeasurementFields.Count);

                var measurementField = processedMeasurement.MeasurementFields[0];
                Assert.AreEqual("Temperature", measurementField.Name);
                Assert.AreEqual(23.6M, measurementField.Value);
            }

            /// <summary>
            /// If user uses "Field1"... in SenML or intentionally
            /// do not remap those fields.
            /// </summary>
            [Test]
            public async Task OnMessage_WithFieldnames_MatchingOldApi_ButNotUsingOldApi_DoesNotRemap()
            {
                // Arrange
                Guid deviceId = new Guid("FC6B39AF-352A-42F3-A848-45AA1FA08183");
                Guid accountId = new Guid("D1CB14F5-1376-4F82-B4B7-1F2A36639DE4");

                var membershipService = new Mock<IMembershipService>();
                var logger = new Mock<ILogger>();

                var device = new User
                {
                    UserId = deviceId,
                    AccountId = accountId,
                    FieldDescriptors = new List<FieldDescriptor>
                    {
                        new FieldDescriptor
                        {
                            Index = 0,
                            Name = "Temperature",
                            Visible = true
                        }
                    }
                };

                membershipService
                    .Setup(x => x.LoadAsync(accountId, deviceId, false))
                    .Returns(Task.FromResult(device));

                var measurementProcessor = new NewMeasurementRecordProcessor(membershipService.Object, logger.Object);

                var measurementEvent = new NewMeasurementEvent
                {
                    UserId = deviceId,
                    AccountId = accountId,
                    MeasurementFields = new List<MeasurementFieldDto>
                    {
                        new MeasurementFieldDto
                        {
                            Name = "Field1",
                            StringValue = "23.6",
                            Value = 23.6M,
                        }
                    },
                    RemapOldFields = false
                };

                // Act
                var processedMeasurement = await measurementProcessor.ProcessAsync(measurementEvent);

                // Assert
                Assert.NotNull(processedMeasurement);

                Assert.NotNull(processedMeasurement.MeasurementFields);
                Assert.AreEqual(1, processedMeasurement.MeasurementFields.Count);

                var measurementField = processedMeasurement.MeasurementFields[0];
                // Not remapped as RemapOldFields is set to false.
                Assert.AreEqual("Field1", measurementField.Name);
                Assert.AreEqual(23.6M, measurementField.Value);
            }
        }
    }
}