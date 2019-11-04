using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tinamous.MeasurementsProcessor.Aws;
using Tinamous.MeasurementsProcessor.DAL.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Documents;
using Tinamous.MeasurementsProcessor.Domain.Settings;

namespace Tinamous.MeasurementsProcessor.DAL.DynamoDB.Repository
{
    public class CheckpointRepository : DynamoRepositoryBase, ICheckpointRepository
    {
        private readonly ILogger<IAmazonDynamoDB> _logger;

        protected override string TableName
        {
            get { return "Checkpoint"; }
        }

        public CheckpointRepository(IAwsClientFactory clientFactory , IOptions<AwsSettings> awsOptions, ILogger<IAmazonDynamoDB> logger)
            : base(clientFactory, awsOptions)
        {
            _logger = logger;
        }

        #region Table Setup

        public async Task CreateTableAsync()
        {
            if (await TableExists())
            {
                _logger.LogInformation("Table {0} already exists, skipping", TableName);
                return;
            }

            var attributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition {AttributeName = "Key", AttributeType = ScalarAttributeType.S},
            };

            var keySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement("Key", KeyType.HASH)
            };

            var request = new CreateTableRequest
            {
                TableName = TablePrefix + TableName,
                AttributeDefinitions = attributeDefinitions,
                KeySchema = keySchema,
                ProvisionedThroughput = new ProvisionedThroughput(1, 1),
            };

            var response = await Client.CreateTableAsync(request);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to create table: {0}", TableName);
            }
            
            await WaitForTableToCompleteAsync("");
        }

        #endregion

        public Checkpoint Load(string shardShardId, string rawMeasurementsStreamName, string name)
        {
            try
            {
                var config = new DynamoDBContextConfig
                {
                    TableNamePrefix = TablePrefix,
                };
                var context = new DynamoDBContext(Client, config);

                return context.LoadAsync<Checkpoint>(Checkpoint.CreateKey(shardShardId, rawMeasurementsStreamName, name)).Result;
            }
            catch (ProvisionedThroughputExceededException ex)
            {
                _logger.LogError(ex, "ProvisionedThroughputExceededException LoadAsync from DynamoDB: " + ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception loading checkpoint by Id: " + ex);
                throw;
            }
        }

        public void Save(Checkpoint checkpoint)
        {
            try
            {
                var config = new DynamoDBContextConfig { TableNamePrefix = TablePrefix, SkipVersionCheck = true };
                var context = new DynamoDBContext(Client, config);

                _logger.LogTrace("Saving checkpoint: {0}", checkpoint);
                context.SaveAsync(checkpoint);
            }
            catch (ProvisionedThroughputExceededException ex)
            {
                _logger.LogError(ex,
                    "ProvisionedThroughputExceededException writing measurement to DynamoDB: " + ex.Message);
                throw;
            }
            catch (ConditionalCheckFailedException ex)
            {
                _logger.LogError(ex, "Conditional check failed for save measurement - ignoring");
                // Safe to ignore as measurement is not likely to be being updated.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception writing measurement to DynamoDB: " + ex.Message);
                throw;
            }
        }
    }
}