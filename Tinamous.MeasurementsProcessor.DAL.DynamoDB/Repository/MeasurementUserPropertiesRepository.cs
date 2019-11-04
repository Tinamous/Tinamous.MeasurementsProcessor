using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tinamous.MeasurementsProcessor.Aws;
using Tinamous.MeasurementsProcessor.DAL.Interfaces;
using Tinamous.MeasurementsProcessor.Domain.Documents;
using Tinamous.MeasurementsProcessor.Domain.Documents.UserDetails;
using Tinamous.MeasurementsProcessor.Domain.Settings;

namespace Tinamous.MeasurementsProcessor.DAL.DynamoDB.Repository
{
    public class MeasurementUserPropertiesRepository : DynamoRepositoryBase, IMeasurementUserPropertiesRepository
    {
        private readonly ILogger<MeasurementUserPropertiesRepository> _logger;

        protected override string TableName
        {
            get { return "Measurement.UserProperties"; }
        }

        public MeasurementUserPropertiesRepository(IAwsClientFactory clientFactory, IOptions<AwsSettings> awsOptions, ILogger<MeasurementUserPropertiesRepository> logger)
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
                new AttributeDefinition {AttributeName = "AccountId", AttributeType = ScalarAttributeType.S},
                new AttributeDefinition {AttributeName = "UserId", AttributeType = ScalarAttributeType.S}
            };

            var keySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement("AccountId", KeyType.HASH),
                new KeySchemaElement("UserId", KeyType.RANGE),
            };

            var request = new CreateTableRequest
            {
                TableName = TablePrefix + TableName,
                AttributeDefinitions = attributeDefinitions,
                KeySchema = keySchema,
                ProvisionedThroughput = new ProvisionedThroughput(2, 2),
                SSESpecification = new SSESpecification
                {
                    Enabled = true,
                },
                StreamSpecification = new StreamSpecification
                {
                    StreamEnabled = true,
                    StreamViewType = StreamViewType.NEW_AND_OLD_IMAGES
                }
            };

            _logger.LogWarning("Creating DynamoDB table: {0}", request.TableName);
            var response = await Client.CreateTableAsync(request);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to create table: {0}", TablePrefix + TableName);
            }

            // Clear the table cache now that we have added a table.
            ClearTableCache();

            await WaitForTableToCompleteAsync();
        }

        #endregion

        public async Task<List<User>> LoadAsync(Guid accountId)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var config = new DynamoDBContextConfig
                {
                    TableNamePrefix = TablePrefix,
                };
                var context = new DynamoDBContext(Client, config);

                var operationConfig = new DynamoDBOperationConfig
                {
                    TableNamePrefix = TablePrefix,
                    ConditionalOperator = ConditionalOperatorValues.And,
                    // Exclude deleted users.
                    QueryFilter = new List<ScanCondition>
                    {
                        new ScanCondition ("Deleted", ScanOperator.Equal, false),
                    }
                };

                // Hash value (Account Id).
                // Don't query on the sort key as this is the device id
                // so will get us ALL the devices.
                var hashKeyValue = accountId;

                var asyncSearch = context.QueryAsync<User>(hashKeyValue, operationConfig);
                return await asyncSearch.GetRemainingAsync();
            }
            catch (ProvisionedThroughputExceededException ex)
            {
                _logger.LogError(ex, "ProvisionedThroughputExceededException LoadAsync for user properties from DynamoDB: " + ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception loading user properties by account Id: " + ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation("Load users properties by Id took: {0}ms for {1}", stopwatch.ElapsedMilliseconds, accountId);
            }
        }

        public async Task<User> LoadAsync(Guid accountId, Guid userId)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var config = new DynamoDBContextConfig
                {
                    TableNamePrefix = TablePrefix,
                };
                var context = new DynamoDBContext(Client, config);

                return await context.LoadAsync<User>(accountId, userId);
            }
            catch (ProvisionedThroughputExceededException ex)
            {
                _logger.LogError(ex, "ProvisionedThroughputExceededException LoadAsync for user properties from DynamoDB: " + ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception loading user properties by Id: " + ex);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation("Load user properties by Id took: {0}ms for {1}", stopwatch.ElapsedMilliseconds, userId);
            }
        }

        public async Task SaveAsync(User userProperties)
        {
            userProperties.Tags = CleanTags(userProperties.Tags);
            CleanFieldDescriptors(userProperties.FieldDescriptors);

            try
            {
                var config = new DynamoDBContextConfig { TableNamePrefix = TablePrefix };
                var context = new DynamoDBContext(Client, config);

                await context.SaveAsync(userProperties);
            }
            catch (ProvisionedThroughputExceededException ex)
            {
                _logger.LogError("ProvisionedThroughputExceededException writing user properties to DynamoDB: " + ex.Message);
                throw;
            }
            catch (ConditionalCheckFailedException ex)
            {
                _logger.LogError("Conditional check failed for save user properties: " + userProperties.UserId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception writing user properties to DynamoDB: " + ex.Message);
                throw;
            }
        }

        private void CleanFieldDescriptors(List<FieldDescriptor> userPropertiesFieldDescriptors)
        {
            foreach (var userPropertiesFieldDescriptor in userPropertiesFieldDescriptors)
            {
                if (!userPropertiesFieldDescriptor.StartDate.HasValue)
                {
                    userPropertiesFieldDescriptor.StartDate = DateTime.MinValue;
                }
                userPropertiesFieldDescriptor.Tags = CleanTags(userPropertiesFieldDescriptor.Tags);
            }
        }

        private List<string> CleanTags(List<string> userPropertiesTags)
        {
            if (userPropertiesTags == null)
            {
                return new List<string>();
            }

            var tags = userPropertiesTags.Distinct().ToList();
            tags.RemoveAll(x => x == null);
            return tags;
        }

        public async Task DeleteAsync(User userProperties)
        {
            try
            {
                var config = new DynamoDBContextConfig
                {
                    TableNamePrefix = TablePrefix,
                    SkipVersionCheck = true,
                };
                var context = new DynamoDBContext(Client, config);

                await context.DeleteAsync(userProperties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception deleting user properties." + ex.Message);
                throw;
            }
        }

    }
}