using System;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Kinesis.Model;

namespace Tinamous.MeasurementsProcessor.Domain.Documents
{
    [DynamoDBTable("Checkpoint")]
    public class Checkpoint
    {
        public Checkpoint()
        { }

        public Checkpoint(Shard shard, string streamName, string name)
        {
            StreamName = streamName;
            ShardId = shard.ShardId;
            Key = CreateKey(shard.ShardId, streamName, name);

            // In case this is the first usage of the shard checkpoint
            // initialize it at the start of the sequence for the shard.
            LastSequenceNumber = shard.SequenceNumberRange.StartingSequenceNumber;
        }

        [DynamoDBHashKey]
        public string Key { get;set; }

        public string StreamName { get; set; }

        public string ShardId { get; set; }

        public string LastSequenceNumber { get; set; }

        public DateTime LastUpdated { get; set; }

        [DynamoDBVersion]
        public int? Version { get; set; }

        public void SetCheckpoint(Record record)
        {
            LastSequenceNumber = record.SequenceNumber;
            LastUpdated = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return string.Format("StreamName: {0}, ShardId: {1}, LastSequenceNumber: {2}", StreamName, ShardId, LastSequenceNumber);
        }

        public static string CreateKey(string shardId, string streamName, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Format("{0}-{1}", streamName, shardId);
            } 
            return string.Format("{0}-{1}-{2}", streamName, shardId, name);
        }
    }
}