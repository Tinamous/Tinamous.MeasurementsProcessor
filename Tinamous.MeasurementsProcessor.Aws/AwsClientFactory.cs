using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Kinesis;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.Extensions.Options;
using Tinamous.MeasurementsProcessor.Domain.Settings;

//using Amazon.S3;
//using Amazon.SimpleNotificationService;
//using Amazon.SQS;

namespace Tinamous.MeasurementsProcessor.Aws
{
    public class AwsClientFactory : IAwsClientFactory
    {
        private readonly AwsSettings _settings;

        public AwsClientFactory(IOptions<AwsSettings> awsOptions)
        {
            _settings = awsOptions.Value;
        }

        public IAmazonKinesis CreateKinesisClient()
        {
            RegionEndpoint region = GetRegionEndpoint();

            AWSCredentials awsCredentials = GetAwsCredentials();
            if (awsCredentials != null)
            {
                return new AmazonKinesisClient(awsCredentials, region);
            }

            // Production. Uses roles.
            return new AmazonKinesisClient(region);
        }

        //public IAmazonSimpleNotificationService CreateSnsClient()
        //{
        //    RegionEndpoint region = _settings.GetRegionEndpoint();

        //    AWSCredentials awsCredentials = GetAwsCredentials();
        //    if (awsCredentials != null)
        //    {
        //        return new AmazonSimpleNotificationServiceClient(awsCredentials, region);
        //    }

        //    // Production. Uses roles.
        //    return new AmazonSimpleNotificationServiceClient(region);
        //}

        //public IAmazonSQS CreateSqsClient()
        //{
        //    RegionEndpoint region = _settings.GetRegionEndpoint();

        //    AWSCredentials awsCredentials = GetAwsCredentials();
        //    if (awsCredentials != null)
        //    {
        //        return new AmazonSQSClient(awsCredentials, region);
        //    }

        //    // Production. Uses roles.
        //    return new AmazonSQSClient(region);
        //}

        public IAmazonDynamoDB CreateDynamoDBClient()
        {
            RegionEndpoint region = GetRegionEndpoint();

            AWSCredentials awsCredentials = GetAwsCredentials();
            if (awsCredentials != null)
            {
                return new AmazonDynamoDBClient(awsCredentials, region);
            }

            // Production. Uses roles.
            return new AmazonDynamoDBClient(region);
        }

        private AWSCredentials GetAwsCredentials()
        {
            CredentialProfileStoreChain credentialProfileStoreChain = new CredentialProfileStoreChain();

            CredentialProfile profile;
            if (credentialProfileStoreChain.TryGetProfile(_settings.ProfileName , out profile))
            {
                AWSCredentials awsCredentials;
                if (AWSCredentialsFactory.TryGetAWSCredentials(profile, null, out awsCredentials))
                {
                    return awsCredentials;
                }
            }

            // Production. Uses roles.
            return null;
        }


        public RegionEndpoint GetRegionEndpoint()
        {
            return RegionEndpoint.GetBySystemName(_settings.Region);
        }
    }
}