using AnalysisUK.Tinamous.Measurements.Messaging.Model.Dtos;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;
using AutoMapper;
using Tinamous.MeasurementsProcessor.Domain.Documents;
using Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails;

namespace Tinamous.MeasurementsProcessor.Services.Mapping
{
    public class AutoMapperConfiguration
    {
        public static MapperConfiguration Configure()
        {
            return new MapperConfiguration(cfg =>
            {
                MapCommonMessaging(cfg);
                MapDynamoDbMeasurement(cfg);
                MapDynamoDbMonthlyMeasurement(cfg);
                MapDynamoMonthlyToDto(cfg);
                MapMembershipDto(cfg);
                MapSummaryDto(cfg);
                MapFieldStatus(cfg);
            });
        }


        private static void MapSummaryDto(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<DynamoTimeValuePoint, AnalysisUK.Tinamous.Measurements.Messaging.Model.Dtos.Summary.TimeValuePointDto>();
            cfg.CreateMap<DynamoSummaryStatistics, AnalysisUK.Tinamous.Measurements.Messaging.Model.Dtos.Summary.SummaryStatisticsDto>();
            cfg.CreateMap<DynamoDailyFieldSummaryModel, AnalysisUK.Tinamous.Measurements.Messaging.Model.Dtos.Summary.SummaryDto>()
                .ForMember(x => x.Field, options => options.MapFrom(y => new ChannelFieldDto { Channel = y.Channel, FieldName = y.Field }))
                .ForMember(x => x.BaseTime, options => options.MapFrom(y => y.Day))
                .ForMember(x => x.CountsPerIntervalUnit, options => options.MapFrom(y => y.CountsPerHour))
                .ForMember(x => x.CreatedAt, options => options.MapFrom(y => y.Created));
            cfg.CreateMap<DynamoHourlyFieldSummaryModel, AnalysisUK.Tinamous.Measurements.Messaging.Model.Dtos.Summary.SummaryDto>()
                .ForMember(x => x.Field, options => options.MapFrom(y => new ChannelFieldDto { Channel = y.Channel, FieldName = y.Field }))
                .ForMember(x => x.BaseTime, options => options.MapFrom(y => y.BaseTime))
                .ForMember(x => x.CountsPerIntervalUnit, options => options.MapFrom(y => y.CountsPerMinute))
                .ForMember(x => x.CreatedAt, options => options.MapFrom(y => y.Created));
        }

        private static void MapDynamoDbMeasurement(IMapperConfigurationExpression cfg)
        {
            // Should not be needed any more
            cfg.CreateMap<DynamoMeasurementField, MeasurementField>()
                .ForMember(x => x.Time, options => options.MapFrom(y => y.Time.HasValue ? y.Time.Value.ToUniversalTime() : y.Time))
                .ForMember(x => x.BoolValue, options => options.MapFrom(y => y.BooleanValue));

            cfg.CreateMap<DynamoLocation, LocationDetails>()
                .ForMember(x => x.LastUpdated, options => options.Ignore())
                .ForMember(x => x.Id, options => options.MapFrom(y => y.LocationId))
                .ForMember(x => x.Name, options => options.MapFrom(y => y.LocationName));

            // Should not be needed any more
            //Mapper.CreateMap<DynamoMeasurementModel, Measurement>()
            //    .ForMember(x => x.MongoDbId, options => options.MapFrom(y => y.Id))
            //    .ForMember(x => x.UserMongoId, options => options.MapFrom(y => y.DeviceId))
            //    .ForMember(x => x.AccountMongoId, options => options.MapFrom(y => y.AccountId))
            //    // TODO: Remove AccountId
            //    .ForMember(x => x.RfStrength, options => options.MapFrom(y => y.RfStrengthPercentage))
            //    .ForMember(x => x.MeasurementDate, options => options.MapFrom(y => y.MeasurementDate.ToUniversalTime()))
            //    .ForMember(x => x.DateAdded, options => options.MapFrom(y => y.MeasurementDate.ToUniversalTime()))
            //    .ForMember(x => x.Private, options => options.Ignore());

            // New Dynamo => Messaging no middle domain model mapping.
            cfg.CreateMap<DynamoLocation, AnalysisUK.Tinamous.Messaging.Common.Dtos.LocationDto>()
                .ForMember(x => x.LastUpdated, options => options.Ignore())
                .ForMember(x => x.Id, options => options.MapFrom(y => y.LocationId))
                .ForMember(x => x.WellKnownLocationId, options => options.MapFrom(y => y.LocationId))
                .ForMember(x => x.Name, options => options.MapFrom(y => y.LocationName));

            cfg.CreateMap<DynamoMeasurementField, AnalysisUK.Tinamous.Measurements.Messaging.Model.Dtos.MeasurementFieldDto>()
                .ForMember(x => x.BoolValue, options => options.MapFrom(y => y.BooleanValue));

            cfg.CreateMap<DynamoMeasurementModel, AnalysisUK.Tinamous.Measurements.Messaging.Model.Dtos.MeasurementDto>()
                .ForMember(x => x.Source, options => options.Ignore())
               .ForMember(x => x.Private, options => options.Ignore())
               .ForMember(x => x.RfStrength, options => options.MapFrom(y => y.RfStrengthPercentage))
               .ForMember(x => x.User, options => options.MapFrom(y => new UserSummaryDto { AccountId = y.AccountId, UserId = y.DeviceId }));
        }

        private static void MapDynamoMonthlyToDto(IMapperConfigurationExpression cfg)
        {
            // New Dynamo => Messaging no middle domain model mapping.
            cfg.CreateMap<DynamoCompactLocation, AnalysisUK.Tinamous.Messaging.Common.Dtos.LocationDto>()
                .ForMember(x => x.LastUpdated, options => options.MapFrom(y => y.Date))
                .ForMember(x => x.Id, options => options.MapFrom(y => y.Id))
                .ForMember(x => x.WellKnownLocationId, options => options.Ignore())
                .ForMember(x => x.Name, options => options.MapFrom(y => y.Name))
                .ForMember(x => x.Longitude, options => options.MapFrom(y => y.Lng))
                .ForMember(x => x.Latitude, options => options.MapFrom(y => y.Lat))
                .ForMember(x => x.Elevation, options => options.MapFrom(y => y.E));

            cfg.CreateMap<DynamoCompactMeasurementField, AnalysisUK.Tinamous.Measurements.Messaging.Model.Dtos.MeasurementFieldDto>()
                .ForMember(x => x.Name, options => options.MapFrom(y => y.N))
                .ForMember(x => x.Value, options => options.MapFrom(y => y.V))
                .ForMember(x => x.StringValue, options => options.MapFrom(y => y.SV))
                .ForMember(x => x.BoolValue, options => options.MapFrom(y => y.BV))
                .ForMember(x => x.RawValue, options => options.MapFrom(y => y.RV))
                .ForMember(x => x.Unit, options => options.MapFrom(y => y.U))
                .ForMember(x => x.Sum, options => options.MapFrom(y => y.Sum))
                .ForMember(x => x.IsComputed, options => options.Ignore())
                .ForMember(x => x.Time, options => options.Ignore())
                .ForMember(x => x.IsCalibrated, options => options.Ignore());

            // NOTE: DOES NOT MAP USER ACCOUNT WHICH IS USED FOR CHECKING IF THE USER CAN ACCESS!
            cfg.CreateMap<DynamoMonthlyMeasurementModel, AnalysisUK.Tinamous.Measurements.Messaging.Model.Dtos.MeasurementDto>()
               .ForMember(x => x.Source, options => options.Ignore())
               .ForMember(x => x.Private, options => options.Ignore())
               .ForMember(x => x.RfStrength, options => options.MapFrom(y => y.RfPc))
               .ForMember(x => x.User, options => options.MapFrom(y => new UserSummaryDto { UserId = y.DeviceId }))
               .ForMember(x => x.MeasurementDate, options => options.MapFrom(y => y.GetDate()))
               .ForMember(x => x.SampleId, options => options.MapFrom(y => y.SId))
               .ForMember(x => x.Deleted, options => options.Ignore())
               .ForMember(x => x.OperatorId, options => options.Ignore())
               .ForMember(x => x.Channel, options => options.MapFrom(y => y.Chan))
                .ForMember(x => x.BatteryLevelPercentage, options => options.MapFrom(y => y.BattPc))
                .ForMember(x => x.PostedOn, options => options.MapFrom(y => y.GetDate()))
                .ForMember(x => x.DateAdded, options => options.MapFrom(y => y.GetDate()))
                .ForMember(x => x.Expires, options => options.MapFrom(y => y.Expires))
                .ForMember(x => x.MeasurementFields, options => options.MapFrom(y => y.Fields));
        }

        private static void MapDynamoDbMonthlyMeasurement(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<DynamoMonthlyMeasurementModel, DynamoMeasurementModel>()
                .ForMember(x => x.AccountId, options => options.Ignore())
                .ForMember(x => x.MeasurementDate, options => options.MapFrom(y => y.GetDate()))
                .ForMember(x => x.SampleId, options => options.MapFrom(y => y.SId))
                .ForMember(x => x.Deleted, options => options.Ignore())
                .ForMember(x => x.OperatorId, options => options.Ignore())
                .ForMember(x => x.Channel, options => options.MapFrom(y => y.Chan))
                .ForMember(x => x.BatteryLevel, options => options.MapFrom(y => y.Batt))
                .ForMember(x => x.BatteryLevelPercentage, options => options.MapFrom(y => y.BattPc))
                .ForMember(x => x.RfStrengthPercentage, options => options.MapFrom(y => y.RfPc))
                .ForMember(x => x.PostedOn, options => options.MapFrom(y => y.GetDate()))
                .ForMember(x => x.DateAdded, options => options.MapFrom(y => y.GetDate()))
                .ForMember(x => x.TTL, options => options.Ignore())
                .ForMember(x => x.MeasurementFields, options => options.MapFrom(y => y.Fields));


            cfg.CreateMap<DynamoCompactMeasurementField, DynamoMeasurementField>()
                .ForMember(x => x.Name, options => options.MapFrom(y => y.N))
                .ForMember(x => x.Value, options => options.MapFrom(y => y.V))
                .ForMember(x => x.StringValue, options => options.MapFrom(y => y.SV))
                .ForMember(x => x.BooleanValue, options => options.MapFrom(y => y.BV))
                .ForMember(x => x.RawValue, options => options.MapFrom(y => y.RV))
                .ForMember(x => x.Unit, options => options.MapFrom(y => y.U))
                .ForMember(x => x.Sum, options => options.MapFrom(y => y.Sum))
                .ForMember(x => x.IsComputed, options => options.Ignore())
                .ForMember(x => x.Time, options => options.Ignore())
                .ForMember(x => x.IsCalibrated, options => options.Ignore());

            cfg.CreateMap<DynamoCompactLocation, DynamoLocation>()
                .ForMember(x => x.Elevation, options => options.MapFrom(y => y.E))
                .ForMember(x => x.Latitude, options => options.MapFrom(y => y.Lat))
                .ForMember(x => x.Longitude, options => options.MapFrom(y => y.Lng))
                .ForMember(x => x.LocationId, options => options.MapFrom(y => y.Id))
                .ForMember(x => x.LocationName, options => options.MapFrom(y => y.Name))
                .ForMember(x => x.LastUpdated, options => options.MapFrom(y => y.Date));
        }

        private static void MapCommonMessaging(IMapperConfigurationExpression cfg)
        {
            //Mapper.CreateMap<LocationDto, LocationDetails>();
            //Mapper.CreateMap<LocationDetails, LocationDto>();

            cfg.CreateMap<LocationDto, LocationDetails>();
            cfg.CreateMap<LocationDetails, LocationDto>()
                .ForMember(x => x.WellKnownLocationId, options => options.MapFrom(y => y.Id));
        }

        //private static void MapMeasurementMessagingModels()
        //{
        //    Mapper.CreateMap<AnalysisUK.Tinamous.Measurements.Domain.Documents.MeasurementField, Messaging.Model.Dtos.MeasurementFieldDto>();
        //    Mapper.CreateMap<Messaging.Model.Dtos.MeasurementFieldDto, AnalysisUK.Tinamous.Measurements.Domain.Documents.MeasurementField>();

        //    Mapper.CreateMap<AnalysisUK.Tinamous.Measurements.Domain.Documents.Measurement, Messaging.Model.Dtos.MeasurementDto>()
        //        .ForMember(x => x.Id, options => options.MapFrom(y => y.MongoDbId))
        //        .ForMember(x => x.Source, options => options.Ignore())
        //        .ForMember(x => x.User, options => options.MapFrom(y => new UserSummaryDto { AccountId = y.AccountMongoId, UserId = y.UserMongoId }));
        //}

        /// <summary>
        /// DTO's from the membership service (come from device updated events etc)
        /// </summary>
        /// <param name="cfg"></param>
        private static void MapMembershipDto(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<AnalysisUK.Tinamous.Membership.Messaging.Dtos.User.FieldRangeDto, FieldRange>()
                .ForMember(x => x.Color, options => options.MapFrom(y => y.BandColor));

            cfg.CreateMap<AnalysisUK.Tinamous.Membership.Messaging.Dtos.User.ComputedFieldVariablesDto, ComputedFieldVariables>();
            cfg.CreateMap<AnalysisUK.Tinamous.Membership.Messaging.Dtos.User.FieldCalibrationDto, FieldCalibration>();

            cfg.CreateMap<AnalysisUK.Tinamous.Membership.Messaging.Dtos.User.FieldDescriptorDto, FieldDescriptor>()
                // TODO: Need to figure out if secondary option is selected.
                .ForMember(x => x.ChartAxisOption, options => options.MapFrom(y => y.YAxis));

            cfg.CreateMap<AnalysisUK.Tinamous.Membership.Messaging.Dtos.User.PublishOptionsDto, PublishOptions>()
                .ForMember(x => x.Mqtt, options => options.MapFrom(y => y.PublishToMqtt));

            cfg
                .CreateMap<AnalysisUK.Tinamous.Membership.Messaging.Dtos.User.UserDto, User>()
                .ForMember(x => x.UserId, options => options.MapFrom(y => y.Id))
                .ForMember(x => x.MembershipUserVersion, options => options.MapFrom(y => y.Version))
                .ForMember(x => x.DynamoDBVersion, options => options.Ignore())
                .ForMember(x => x.DisplayName, options => options.MapFrom(y => y.FullName))
                .ForMember(x => x.PublishOptions, options => options.MapFrom(y => y.PublishOptions.MeasurementOptions))
                .ForMember(x => x.PurgeDate, options => options.MapFrom(y => y.MeasurementsPurgeDate))
                // TODO: This should be settable on the userDto object.
                .ForMember(x => x.MeasurementsRetentionTimeDays, options => options.Ignore());

        }

        private static void MapFieldStatus(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<FieldStatus, DynamoFieldStatus>();
            cfg.CreateMap<DynamoFieldStatus, FieldStatus>()
                .ForMember(x => x.IsDirty, options => options.Ignore());
        }
    }
}