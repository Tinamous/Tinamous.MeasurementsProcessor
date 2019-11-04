using Amazon.DynamoDBv2;
using Amazon.Kinesis;

//using Amazon.S3;
//using Amazon.SimpleNotificationService;
//using Amazon.SQS;

namespace Tinamous.MeasurementsProcessor.Aws
{
    public interface IAwsClientFactory
    {
        //IAmazonS3 CreateS3Client();
        IAmazonKinesis CreateKinesisClient();
        //IAmazonSimpleNotificationService CreateSnsClient();
        //IAmazonSQS CreateSqsClient();
        IAmazonDynamoDB CreateDynamoDBClient();
    }
}