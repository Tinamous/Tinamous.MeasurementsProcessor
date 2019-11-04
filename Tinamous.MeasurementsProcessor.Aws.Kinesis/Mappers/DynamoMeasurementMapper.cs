using System;
using System.Collections.Generic;
using System.Linq;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Dtos;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Events;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;
using Tinamous.MeasurementsProcessor.Domain.Documents;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis.Mappers
{
    public static class DynamoMeasurementMapper
    {
        public static List<DynamoMeasurementModel> Map(List<ProcessedMeasurementEvent> processedMeasurementEvents)
        {
            return processedMeasurementEvents.Select(Map).ToList();
        }

        public static DynamoMeasurementModel Map(ProcessedMeasurementEvent newMeasurementEvent)
        {
            var measurementFields = MapFields(newMeasurementEvent.MeasurementFields);

            return new DynamoMeasurementModel
            {
                Id = newMeasurementEvent.Id,
                Channel = newMeasurementEvent.Channel,
                AccountId = newMeasurementEvent.User.AccountId,
                DeviceId = newMeasurementEvent.User.UserId,

                MeasurementDate = newMeasurementEvent.MeasurementDate,
                PostedOn = newMeasurementEvent.PostedOn,
                DateAdded = DateTime.UtcNow,
                SampleId = newMeasurementEvent.SampleId,
                OperatorId = newMeasurementEvent.OperatorId,

                BatteryLevelPercentage = newMeasurementEvent.BatteryLevelPercentage,
                RfStrengthPercentage = newMeasurementEvent.RfStrength,

                MeasurementFields = measurementFields,
                Tags = GetTags(newMeasurementEvent),
                Location = MapLocation(newMeasurementEvent.Location),

                DeleteAfter = newMeasurementEvent.DeleteAfter,
                Expires = newMeasurementEvent.Expires,
                Source = ((int)newMeasurementEvent.Source).ToString()
            };
        }

        private static List<DynamoMeasurementField> MapFields(IEnumerable<MeasurementFieldDto> measurementFields)
        {
            var fields = new List<DynamoMeasurementField>();

            if (measurementFields != null)
            {
                foreach (var measurementFieldDto in measurementFields)
                {
                    fields.Add(MapField(measurementFieldDto));
                }
            }
            else
            {
               // _logger.LogWarning("ProcessedMeasurementEvent received with null MeasurementFields");
            }
            return fields;
        }

        private static DynamoMeasurementField MapField(MeasurementFieldDto measurementFieldDto)
        {
            return new DynamoMeasurementField
            {
                Name = measurementFieldDto.Name,
                Time = measurementFieldDto.Time,
                Unit = measurementFieldDto.Unit,
                Value = measurementFieldDto.Value,
                RawValue = measurementFieldDto.RawValue,
                StringValue = measurementFieldDto.StringValue,
                BooleanValue = measurementFieldDto.BoolValue,
                Sum = measurementFieldDto.Sum,
                IsComputed = measurementFieldDto.IsComputed,
                IsCalibrated = measurementFieldDto.IsCalibrated,
            };
        }

        private static List<string> GetTags(ProcessedMeasurementEvent newMeasurementEvent)
        {
            if (newMeasurementEvent.Tags == null)
            {
                return new List<string>();
            }
            return newMeasurementEvent
                .Tags
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        private static DynamoLocation MapLocation(LocationDto location)
        {
            if (location == null)
            {
                return null;
            }

            return new DynamoLocation
            {
                Elevation = location.Elevation,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                LocationId = location.Id,
                LocationName = location.Name,
            };
        }
    }
}