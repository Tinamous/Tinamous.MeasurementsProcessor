using System;
using System.Collections.Generic;
using System.Linq;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Dtos;
using AnalysisUK.Tinamous.Measurements.Messaging.Model.Events;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;
using Tinamous.MeasurementsProcessor.Domain.Documents;

namespace Tinamous.MeasurementsProcessor.Aws.Kinesis.Mappers
{
    public static class DynamoMonthlyMeasurementMapper
    {
        public static List<DynamoMonthlyMeasurementModel> Map(List<ProcessedMeasurementEvent> processedMeasurementEvents)
        {
            return processedMeasurementEvents.Select(Map).ToList();
        }

        public static DynamoMonthlyMeasurementModel Map(ProcessedMeasurementEvent newMeasurementEvent)
        {
            var measurementFields = MapFields(newMeasurementEvent.MeasurementFields);

            return new DynamoMonthlyMeasurementModel(newMeasurementEvent.Id, newMeasurementEvent.MeasurementDate)
            {
                Chan = newMeasurementEvent.Channel,
                DeviceId = newMeasurementEvent.User.UserId,

                SId = newMeasurementEvent.SampleId,

                BattPc = newMeasurementEvent.BatteryLevelPercentage,
                RfPc = newMeasurementEvent.RfStrength,

                Fields = measurementFields,
                Tags = GetTags(newMeasurementEvent),
                Location = MapLocation(newMeasurementEvent.Location),

                DeleteAfter = newMeasurementEvent.DeleteAfter,
                Expires = newMeasurementEvent.Expires,
                Source = (int)newMeasurementEvent.Source
            };
        }

        private static List<DynamoCompactMeasurementField> MapFields(IEnumerable<MeasurementFieldDto> measurementFields)
        {
            var fields = new List<DynamoCompactMeasurementField>();

            if (measurementFields != null)
            {
                foreach (var measurementFieldDto in measurementFields)
                {
                    fields.Add(MapField(measurementFieldDto));
                }
            }
            else
            {
                //_logger.LogWarning("ProcessedMeasurementEvent received with null MeasurementFields");
            }
            return fields;
        }

        /// <summary>
        /// Map the measurement field dto to a Dynamo persisted field.
        /// Note: Time is ignored. It is expected that all fields have the same
        /// time which is given by the measurement date.
        /// </summary>
        /// <param name="measurementFieldDto"></param>
        /// <returns></returns>
        private static DynamoCompactMeasurementField MapField(MeasurementFieldDto measurementFieldDto)
        {
            return new DynamoCompactMeasurementField
            {
                N = measurementFieldDto.Name,
                U = measurementFieldDto.Unit,
                V = measurementFieldDto.Value,
                RV = measurementFieldDto.RawValue,
                SV = measurementFieldDto.StringValue,
                BV = measurementFieldDto.BoolValue,
                Sum = measurementFieldDto.Sum,
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

        private static DynamoCompactLocation MapLocation(LocationDto location)
        {
            if (location == null)
            {
                return null;
            }

            return new DynamoCompactLocation
            {
                E = location.Elevation,
                Lat = location.Latitude,
                Lng = location.Longitude,
                Id = location.Id,
                Name = location.Name,
            };
        }
    }
}