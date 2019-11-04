using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;
using Tinamous.MeasurementsProcessor.Aws;
using Tinamous.MeasurementsProcessor.Domain.Settings;

namespace Tinamous.MeasurementsProcessor.DAL.DynamoDB.Repository
{
    public abstract class DynamoRepositoryBase
    {
        private static List<string> _tables;

        protected DynamoRepositoryBase(IAwsClientFactory clientFactory, IOptions<AwsSettings> awsOptions)
        {
            if (clientFactory == null) throw new ArgumentNullException("clientFactory");

            TablePrefix = awsOptions.Value.DynamoDbTablePrefix;
            Client = clientFactory.CreateDynamoDBClient();
        }

        protected abstract string TableName { get; }
        protected string TablePrefix { get; private set; }
        protected IAmazonDynamoDB Client { get; private set; }

        protected async Task<bool> TableExists(string datePrefix = "")
        {
            _tables = await ListTables();
            return _tables.Contains(TablePrefix + datePrefix + TableName);
        }

        protected void ClearTableCache()
        {
            _tables.Clear();
        }

        public async Task<List<string>> ListTables()
        {
            var request = new ListTablesRequest();
            var response = await Client.ListTablesAsync(request);

            return response.TableNames;
        }

        protected async Task WaitForTableToCompleteAsync(string datePrefix = "")
        {
            bool busy = true;
            var tableNameWithPrefixs = TablePrefix + datePrefix + TableName;
            int counter = 0;

            do
            {
                var request = new DescribeTableRequest
                {
                    TableName = tableNameWithPrefixs
                };

                var response = await Client.DescribeTableAsync(request);

                busy = (response.Table.TableStatus != TableStatus.ACTIVE);

                if (busy)
                {
                    Thread.Sleep(1000);

                    counter++;
                    if (counter > 600)
                    {
                        throw new TimeoutException("DynamoDB table busy for longer than expected");
                    }
                }
            } while (busy);
        }
    }
}