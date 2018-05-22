using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using ServerlessMapReduceDotNet.Model;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Helpers;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Queue.AmazonSqs
{
    class AmazonSqsQueueClient : IQueueClient
    {
        private readonly IConfig _config;
        private readonly AmazonSQSClient _sqsClient;

        public AmazonSqsQueueClient(IConfig config)
        {
            _config = config;

            var amazonSqsClientConfig = new AmazonSQSConfig
            {
                ServiceURL = _config.AmazonSqsServiceUrl
            };
            _sqsClient = new AmazonSQSClient(amazonSqsClientConfig);
        }
        
        public async Task Enqueue(string queueName, string message)
        {
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = QueueUrlFactory(queueName),
                MessageGroupId = UniqueIdHelper.GenerateUniqueId(),
                MessageDeduplicationId = UniqueIdHelper.GenerateUniqueId(),
                MessageBody = message
            };
            await _sqsClient.SendMessageAsync(sendMessageRequest);
        }

        public async Task<IList<QueueMessage>> Dequeue(string queueName, int maxMessagesToDequeue)
        {
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = QueueUrlFactory(queueName),
                MaxNumberOfMessages = maxMessagesToDequeue
            };
            var receiveMessageResponse = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest);
            return receiveMessageResponse.Messages.Select(x => new QueueMessage
            {
                Message = x.Body,
                MessageId = x.ReceiptHandle
            }).ToList();
        }

        public async Task MessageProcessed(string queueName, string messageId)
        {
            var deleteMessageRequest = new DeleteMessageRequest
            {
                QueueUrl = QueueUrlFactory(queueName),
                ReceiptHandle = messageId
            };
            await _sqsClient.DeleteMessageAsync(deleteMessageRequest);
        }

        public async Task ReturnMessageToQueue(string queueName, string messageId)
        {
            var changeMessageVisibilityRequest = new ChangeMessageVisibilityRequest()
            {
                QueueUrl = QueueUrlFactory(queueName),
                ReceiptHandle = messageId,
                VisibilityTimeout = 0
            };
            await _sqsClient.ChangeMessageVisibilityAsync(changeMessageVisibilityRequest);
        }

        public async Task<int> MessageCount(string queueName)
        {
            var getQueueAttributesRequest = new GetQueueAttributesRequest
            {
                QueueUrl = QueueUrlFactory(queueName),
                AttributeNames = new List<string> {"ApproximateNumberOfMessages"}
            };
            var getQueueAttributesResponse = await _sqsClient.GetQueueAttributesAsync(getQueueAttributesRequest);
            
            // for FIFO queues this number is, in fact, exact
            return getQueueAttributesResponse.ApproximateNumberOfMessages;
        }

        private string QueueUrlFactory(string queueName)
        {
            return $"{_config.AmazonSqsBaseUrl}{queueName}";
        }
    }
}