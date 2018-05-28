aws sqs send-message --queue-url https://sqs.eu-west-1.amazonaws.com/525470265062/serverless-mapreduce-rawdata.fifo --message-body raw/MakeModel2016.csv --message-group-id 1 --message-deduplication-id 1
aws sqs send-message --queue-url https://sqs.eu-west-1.amazonaws.com/525470265062/serverless-mapreduce-rawdata.fifo --message-body raw/cars-registered-2016.csv --message-group-id 1 --message-deduplication-id 2

