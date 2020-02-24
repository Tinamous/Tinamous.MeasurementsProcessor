using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Dtos;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Events;
using AnalysisUK.Tinamous.Membership.Messaging.Dtos;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;
using AnalysisUK.Tinamous.Messaging.Common.Enums;
using Microsoft.Extensions.Logging;
using Tinamous.MeasurementsProcessor.Aws.Kinesis.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Documents;
using Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails;
using Tinamous.MeasurementsProcessor.Domain.Extensions;
using Tinamous.MeasurementsProcessor.Services.Interfaces;

namespace Tinamous.MeasurementsProcessor.Services.Processors
{
    public class NewMeasurementRecordProcessor : IRecordProcessor
    {
        private readonly IMembershipService _membershipService;
        private readonly ILogger _logger;

        public NewMeasurementRecordProcessor(IMembershipService membershipService, ILogger logger)
        {
            _membershipService = membershipService;
            _logger = logger;
        }

        public async Task<ProcessedMeasurementEvent> ProcessAsync(NewMeasurementEvent newMeasurementEvent)
        {
            if (newMeasurementEvent.UserId == Guid.Empty)
            {
                
                _logger.LogInformation("Not persisting measurement {0} as invalid user id. {1}",
                    newMeasurementEvent.Id,
                    newMeasurementEvent.UserId);
                return null;
            }

            try
            {
                Measurement measurement = MapFromNewMeasurementEvent(newMeasurementEvent);

                User device = await _membershipService.LoadAsync(newMeasurementEvent.AccountId, newMeasurementEvent.UserId);

                if (device == null)
                {
                    return null;
                }

                Measurement processedMeasurement = await ProcessMeasurementAsync(measurement, device, newMeasurementEvent.RemapOldFields);

                ProcessedMeasurementEvent processedMeasurementEvent = CreateProcessedMeasurementEvent(processedMeasurement, device);

                await _membershipService.UpdateFieldsAsync(device, processedMeasurementEvent);

                return processedMeasurementEvent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process new measurement.");
                // re-throw so that messaging keeps the message.
                throw;
            }
        }

        #region Mapping

        private Measurement MapFromNewMeasurementEvent(NewMeasurementEvent newMeasurementEvent)
        {
            var measurement = new Measurement
            {
                MongoDbId = newMeasurementEvent.Id,
                Channel = newMeasurementEvent.Channel,
                AccountMongoId = newMeasurementEvent.AccountId,
                UserMongoId = newMeasurementEvent.UserId,
                MeasurementDate = newMeasurementEvent.MeasurementDate,
                PostedOn = newMeasurementEvent.PostedOn,
                DateAdded = DateTime.UtcNow,
                SampleId = newMeasurementEvent.SampleId,
                Tags = newMeasurementEvent.Tags ?? new List<string>(),
                Expires = newMeasurementEvent.Expires,
                TTL = newMeasurementEvent.TTL,
                Source = newMeasurementEvent.Source.ToString(),
            };

            if (measurement.TTL.HasValue && !measurement.Expires.HasValue)
            {
                // Expires not set, but measurement has TTL.
                measurement.Expires = measurement.MeasurementDate.Add(measurement.TTL.Value);
            }

            if (newMeasurementEvent.Location != null)
            {
                measurement.Location = new LocationDetails
                {
                    Elevation = newMeasurementEvent.Location.Elevation,
                    Latitude = newMeasurementEvent.Location.Latitude,
                    Longitude = newMeasurementEvent.Location.Longitude,
                    Id = newMeasurementEvent.Location.Id,
                    Name = newMeasurementEvent.Location.Name,
                };
            }

            // MeasurementFields is to replace Fields and Field1..12 as 
            // this is a better match with SenML and 
            if (newMeasurementEvent.MeasurementFields != null)
            {
                measurement.MeasurementFields = Map(newMeasurementEvent.MeasurementFields, newMeasurementEvent.MeasurementDate);
            }
            else
            {
                _logger.LogWarning("NewMeasurementEvent received with null MeasurementFields");
            }

            return measurement;
        }

        private List<MeasurementField> Map(List<MeasurementFieldDto> newMeasurementEvent, DateTime measurementDate)
        {
            var fields = new List<MeasurementField>();
            foreach (var measurementFieldDto in newMeasurementEvent)
            {
                fields.Add(Map(measurementFieldDto, measurementDate));
            }
            return fields;
        }

        private MeasurementField Map(MeasurementFieldDto measurementFieldDto, DateTime measurementDate)
        {
            return new MeasurementField
            {
                BoolValue = measurementFieldDto.BoolValue,
                Name = measurementFieldDto.Name,
                StringValue = measurementFieldDto.StringValue,
                Sum = GetSum(measurementFieldDto.Sum),
                Time = measurementFieldDto.Time ?? measurementDate,
                Unit = measurementFieldDto.Unit,
                Value = measurementFieldDto.Value,
                RawValue = measurementFieldDto.Value,
                IsCalibrated = measurementFieldDto.IsCalibrated,
                IsComputed = measurementFieldDto.IsComputed,
            };
        }

        private static decimal GetSum(decimal? sum)
        {
            if (!sum.HasValue)
            {
                return 0;
            }
            return sum.Value;
        }

        private List<UserMeasurementField> MapToMembershipFields(List<MeasurementField> measurementFields, int channel)
        {
            var membershipFields = new List<UserMeasurementField>();
            foreach (var measurementField in measurementFields)
            {
                membershipFields.Add(new UserMeasurementField
                {
                    Channel = channel,
                    Name = measurementField.Name,
                    Time = measurementField.Time,
                    BoolValue = measurementField.BoolValue,
                    StringValue = measurementField.StringValue,
                    Sum = measurementField.Sum,
                    Unit = measurementField.Unit,
                    Value = measurementField.Value,
                });
            }
            return membershipFields;
        }

        private List<MeasurementFieldDto> Map(List<MeasurementField> measurementFields)
        {
            var fields = new List<MeasurementFieldDto>();

            if (measurementFields != null)
            {
                foreach (var measurementField in measurementFields)
                {
                    fields.Add(new MeasurementFieldDto
                    {
                        BoolValue = measurementField.BoolValue,
                        IsComputed = measurementField.IsComputed,
                        Name = measurementField.Name,
                        StringValue = measurementField.StringValue,
                        Sum = measurementField.Sum,
                        Time = measurementField.Time,
                        Unit = measurementField.Unit,
                        Value = measurementField.Value,
                        RawValue = measurementField.RawValue,
                        IsCalibrated = measurementField.IsCalibrated,
                    });
                }
            }

            return fields;
        }

        private LocationDto Map(LocationDetails location)
        {
            if (location == null)
            {
                return null;
            }

            return new LocationDto
            {
                Id = location.Id,
                Name = location.Name,
                Elevation = location.Elevation,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                LastUpdated = location.LastUpdated,
                WellKnownLocationId = location.Id
            };
        }

        #endregion

        #region Publishing

         /// <summary>
        /// Publish the ProcessedMeasurementEvent from the Measurements service
        /// (i.e. the new event)
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="device"></param>
        private ProcessedMeasurementEvent CreateProcessedMeasurementEvent(Measurement measurement, User device)
        {
            if (measurement.MeasurementFields == null)
            {
                _logger.LogError("Old Field1...Field12 values are no longer supported in Measurements Service");
            }

            if (measurement.PostedOn < new DateTime(2016, 1, 1))
            {
                _logger.LogError("Measurement posted on {0} before 2016. UserId: {1}", measurement.PostedOn, measurement.UserMongoId);
                // "Fix" the date by using UTC now.
                measurement.PostedOn = DateTime.UtcNow;
            }

            var processedMeasurementEvent = new ProcessedMeasurementEvent
            {
                Id = measurement.MongoDbId,
                User = new UserSummaryDto
                {
                    AccountId = measurement.AccountMongoId,
                    UserId = measurement.UserMongoId,
                    UserName = device.UserName,
                    FullUserName = device.FullUserName,
                    FullName = device.DisplayName,
                },
                BatteryLevel = measurement.BatteryLevel,
                BatteryLevelPercentage = measurement.BatteryLevelPercentage,
                Channel = measurement.Channel,
                Location = Map(measurement.Location),
                MeasurementDate = measurement.MeasurementDate,
                MeasurementFields = Map(measurement.MeasurementFields),
                OperatorId = measurement.OperatorId,
                PostedOn = measurement.PostedOn,
                PublishToMqtt = device.PublishOptions.Mqtt,
                RfStrength = measurement.RfStrength,
                SampleId = measurement.SampleId,
                Expires = measurement.Expires,
                DeleteAfter = measurement.DeleteAfter,
                Tags = measurement.Tags,
                Source = (Source)Enum.Parse(typeof(Source), measurement.Source),
            };

            return processedMeasurementEvent;
        }


        #endregion

        // TODO: Drop usage of measurement as parameter and ideally return type.
        private async Task<Measurement> ProcessMeasurementAsync(Measurement measurement, 
            User device, 
            bool remapOldFields)
        {
            if (measurement.MeasurementFields == null)
            {
                _logger.LogError("Old Field1...Field12 values are no longer supported in Measurements Service");
            }

            try
            {
                measurement = MapOldApiFieldNames(measurement, device, remapOldFields);
                measurement = ExtractMetaData(measurement);
                measurement = ApplyCalibration(measurement, device);
                measurement = PopulateComputedFields(measurement, device);
                measurement = PopulateLocation(measurement, device);
                measurement = SetDeleteAfter(measurement, device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process measurement.");
            }

            // TODO: How to handle virtual fields?
            return measurement;
        }

        private Measurement PopulateLocation(Measurement measurement, User device)
        {
            if (measurement.Location != null)
            {
                return measurement;
            }

            if (device.Location == null)
            {
                return measurement;
            }

            if (!device.Location.IsValidLocation())
            {
                
                _logger.LogWarning("Device {0} has an invalid location", device.FullUserName);
            }

            // If we have a location for the device and the measurement doesn't have one assigned
            // then set the measurements location
            measurement.Location = device.Location;

            return measurement;
        }

        private Measurement ExtractMetaData(Measurement measurement)
        {
            foreach (var measurementField in measurement.MeasurementFields)
            {
                TryExtractMetaData(measurement, measurementField);
            }
            return measurement;
        }

        private void TryExtractMetaData(Measurement measurement, MeasurementField measurementField)
        {
            if (string.IsNullOrEmpty(measurementField.Name))
            {
                return;
            }

            try
            {
                switch (measurementField.Name.ToLower())
                {
                    case "sampleid":
                        measurement.SampleId = measurementField.StringValue;
                        break;
                    case "operatorid":
                        measurement.OperatorId = measurementField.StringValue;
                        break;
                    case "soc": // stage of charge (expect %)
                    case "ps-soc": // (stage of charge) to match photon battery shield.
                    case "battery":
                    case "batterylevel":
                    case "batterylevelpercentage":
                        measurement.BatteryLevel = measurementField.Value;
                        measurement.BatteryLevelPercentage = Convert.ToInt32(measurementField.Value);
                        break;
                    case "rf":
                    case "rfstrength":
                        measurement.RfStrength = Convert.ToInt32(measurementField.Value);
                        break;
                    case "lat":
                    case "latitude":
                        measurement.Location = measurement.Location ?? new LocationDetails();
                        measurement.Location.Latitude = Convert.ToDouble(measurementField.Value);
                        break;
                    case "long":
                    case "longitude":
                        measurement.Location = measurement.Location ?? new LocationDetails();
                        measurement.Location.Longitude = Convert.ToDouble(measurementField.Value);
                        break;
                    case "elevation":
                    case "altitude":
                        measurement.Location = measurement.Location ?? new LocationDetails();
                        measurement.Location.Elevation = Convert.ToDouble(measurementField.Value);
                        break;
                    case "g":
                    case "gps":
                    case "latlong":
                    case "location":
                        measurement.Location = GetLocation(measurementField.StringValue);
                        break;
                    case "tag":
                    case "tags":
                        measurement.Tags.AddRange(ExtractTags(measurementField));
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract field metadata.");
                // and sink, metadata is not overly critical
            }
        }

        private IEnumerable<string> ExtractTags(MeasurementField measurementField)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(measurementField.StringValue))
                {
                    return measurementField.StringValue.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting tags.");
                // and sink
            }

            return new List<string>();
        }

        private LocationDetails GetLocation(string stringValue)
        {
            if (!stringValue.Contains(","))
            {
                _logger.LogInformation("Latlong not in DDDD.dddd,DDDD.dddd format. Comma missing");
                return null;
            }

            try
            {
                string[] split = stringValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length < 2)
                {
                    // Invalid location.
                    _logger.LogInformation("Latlong not in DDDD.dddd,DDDD.dddd format. Length < 2");
                    return null;
                }

                var location = new LocationDetails
                {
                    Latitude = Convert.ToDouble(split[0]),
                    Longitude = Convert.ToDouble(split[1]),
                };

                if (split.Length == 3)
                {
                    location.Elevation = Convert.ToDouble(split[2]);
                }

                return location;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Latlong not in DDDD.dddd,DDDD.dddd format. Error: " + ex.Message);
                return null;
            }
        }

        private Measurement MapOldApiFieldNames(Measurement measurement, User device, bool remapOldFields)
        {
            List<MeasurementField> mappedFields = new List<MeasurementField>();
            foreach (var field in measurement.MeasurementFields)
            {
                var fieldName = field.Name.ToLower();
                if (remapOldFields && fieldName.StartsWith("field"))
                {
                    mappedFields.Add(MapOldApiField(field, device));
                }
                else
                {
                    mappedFields.Add(field);
                }
            }

            measurement.MeasurementFields.Clear();
            measurement.MeasurementFields.AddRange(mappedFields);
            return measurement;
        }

        private MeasurementField MapOldApiField(MeasurementField field, User device)
        {
            string fieldId = field.Name.ToLower().Replace("field", "");

            int fieldIdValue;
            if (int.TryParse(fieldId, out fieldIdValue))
            {
                var fieldDescriptor = device.FieldDescriptors.FirstOrDefault(x => x.Index == fieldIdValue - 1);

                if (fieldDescriptor != null)
                {
                    // Change the field name to reflect the mapped field.
                    field.Name = fieldDescriptor.Name;
                }
            }

            // No match then return the existing field.
            return field;
        }

        private Measurement ApplyCalibration(Measurement measurement, User device)
        {
            IterateFields(measurement, device, (fieldDescriptor, field) =>
            {
                FieldCalibration calibration = fieldDescriptor.Calibration ?? new FieldCalibration();

                if (calibration.Enabled)
                {
                    field.Value = calibration.Apply(field.RawValue);
                    field.IsCalibrated = true;
                }
            });

            return measurement;
        }

        private Measurement PopulateComputedFields(Measurement measurement, User device)
        {
            IterateFields(measurement, device, (fieldDescriptor, field) =>
            {
                if (fieldDescriptor.FieldType == FieldType.Computed)
                {
                    // TODO: Compute the field value.
                    _logger.LogError("Computed field handling not implemented");
                }
            });

            return measurement;
        }
      
        /// <summary>
        /// Iterate each of the fields executing the action.
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="device"></param>
        /// <param name="action"></param>
        private void IterateFields(Measurement measurement, User device, Action<FieldDescriptor, MeasurementField> action)
        {
            int channel = measurement.Channel;
            foreach (var field in measurement.MeasurementFields)
            {
                var fieldDescriptor = device.GetFieldDescriptor(channel, field.Name);
                if (fieldDescriptor != null)
                {
                    action(fieldDescriptor, field);
                }
            }
        }

        /// <summary>
        /// Set the measurements delete after to 
        /// enable DynamoDB to delete the measurement from the database
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        private Measurement SetDeleteAfter(Measurement measurement, User device)
        {
            // Default retention if nothing else is specified
            // 5 years.
            int deleteAfterDays = 365 * 5;

            if (device.MeasurementsRetentionTimeDays.HasValue)
            {
                deleteAfterDays = device.MeasurementsRetentionTimeDays.Value;
            }

            if (measurement.TTL.HasValue)
            {
                // If the measurement specifies a shorter
                // TTL then use that for expiry (and add a day to ensure we don't delete before
                if (measurement.TTL.Value.Days < deleteAfterDays)
                {
                    // 
                    deleteAfterDays = measurement.TTL.Value.Days + 1;
                }
            }

            measurement.DeleteAfter = DateTime.UtcNow.AddDays(deleteAfterDays).ToLongUnixSeconds();

            return measurement;
        }
    }
}