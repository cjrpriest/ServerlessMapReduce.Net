aws --endpoint-url http://localhost:9324 sqs purge-queue --queue-url http://localhost:9324/queue/serverless-mapreduce-finalreduced
aws --endpoint-url http://localhost:9324 sqs purge-queue --queue-url http://localhost:9324/queue/serverless-mapreduce-ingested
aws --endpoint-url http://localhost:9324 sqs purge-queue --queue-url http://localhost:9324/queue/serverless-mapreduce-mapped
aws --endpoint-url http://localhost:9324 sqs purge-queue --queue-url http://localhost:9324/queue/serverless-mapreduce-rawdata
aws --endpoint-url http://localhost:9324 sqs purge-queue --queue-url http://localhost:9324/queue/serverless-mapreduce-reduced
aws --endpoint-url http://localhost:9324 sqs purge-queue --queue-url http://localhost:9324/queue/serverless-mapreduce-commandQueue
