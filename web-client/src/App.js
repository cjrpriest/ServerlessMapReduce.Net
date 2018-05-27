import React, { Component } from 'react';
import logo from './logo.svg';
import './App.css';

import Button from 'react-md/lib/Buttons/Button';

import AWS from 'aws-sdk';
// import { Credentials } from 'aws-sdk/lib/credentials';

class App extends Component {

    constructor() {
        super();
        this.state = {
            queueContents: "",
            queueSize: 0,
            intervalHandler: null
        };
    }

      render() {
        return (
          <div className="App">
            <header className="App-header">
              <img src={logo} className="App-logo" alt="logo" />
              <h1 className="App-title">Welcome to React</h1>
            </header>
            <p className="App-intro">
              To get started, edit <code>src/App.js</code> and save to reload.
            </p>
              <span>No of messages in queue: {this.state.queueSize}</span>
              <Button raised onClick={() => {
                  clearInterval();
                  this.setState({intervalHandler: setInterval(() => this.getQueueInfo(this), 50)});
                }
              }>Go!</Button>
              <Button raised onClick={() => clearInterval(this.state.intervalHandler)}>Stop!</Button>
          </div>
        );
      }

    getQueueInfo(self) {
        let sqs = new AWS.SQS({apiVersion: '2012-11-05', endpoint: 'http://localhost:9324'});

        let creds = new AWS.Credentials('foo', 'bar');

        sqs.config.update(creds);
        sqs.config.update({region: 'eu-central-1'});

        //let self = this;
        sqs.getQueueAttributes({
            QueueUrl: 'http://localhost:9324/queue/serverless-mapreduce-remoteCommandQueue',
            AttributeNames: ['ApproximateNumberOfMessages']
        }, function (err, data) {
            if (err) {
                console.log("Get queue size error", err);
            } else if (data.Attributes) {
                self.setState({queueSize: data.Attributes['ApproximateNumberOfMessages']});
            }
        });

        let params = {
            AttributeNames: [
                "SentTimestamp"
            ],
            MaxNumberOfMessages: 1,
            MessageAttributeNames: [
                "All"
            ],
            QueueUrl: 'http://localhost:9324/queue/serverless-mapreduce-remoteCommandQueue',
            VisibilityTimeout: 5,
            WaitTimeSeconds: 0
        };

        sqs.receiveMessage(params, function(err, data) {
            if (err) {
                console.log("Receive Error", err);
            } else if (data.Messages) {
                //console.log("Message", data.Messages[0]);
                if (!data.Messages[0]) return;

                console.log(data.Messages[0].Body);
                let obj = JSON.parse(data.Messages[0].Body);
                console.log(obj);

                if (obj.Command.$type !== "ServerlessMapReduceDotNet.MapReduce.Commands.Map.BatchMapDataCommand, ServerlessMapReduceDotNet") {
                    console.log("not a BatchMapDataCommand");
                    return;
                }

                let mostAccidentProneKvps = [];
                for (let i = 0, len = obj.Command.Lines.$values.length; i < len; i++) {
                    let line = obj.Command.Lines.$values[i];
                    let data = line.split(',');
                    let manufacturer = data[22].toUpperCase();
                    let mostAccidentProneKvp = {
                        $type: "ServerlessMapReduceDotNet.Model.MostAccidentProneKvp, ServerlessMapReduceDotNet",
                        Key: manufacturer,
                        Value: {
                            NoOfAccidents : 1,
                            NoOfCarsRegistered: 0,
                            RegistrationsPerAccident: 0.0
                        }
                    };
                    mostAccidentProneKvps.push(mostAccidentProneKvp);
                }
                let writeMappedDataCommand = {
                    "$type":"AzureFromTheTrenches.Commanding.Abstractions.Model.NoResultCommandWrapper, AzureFromTheTrenches.Commanding.Abstractions",
                    Command: {
                        "$type":"ServerlessMapReduceDotNet.MapReduce.Commands.Map.WriteMappedDataCommand, ServerlessMapReduceDotNet",
                        ResultOfMap: mostAccidentProneKvps,
                        ContextQueueMessage: obj.Command.ContextQueueMessage
                    }
                };

                console.log(writeMappedDataCommand);

                sqs.sendMessage({
                    QueueUrl: 'http://localhost:9324/queue/serverless-mapreduce-commandQueue',
                    MessageBody: JSON.stringify(writeMappedDataCommand)
                }, function(err, data) {
                    if (err) {
                        console.log("Receive Error", err);
                    } else {
                        console.log(data);
                    }
                });

                sqs.deleteMessage({
                    QueueUrl: 'http://localhost:9324/queue/serverless-mapreduce-remoteCommandQueue',
                    ReceiptHandle: data.Messages[0].ReceiptHandle
                }, function(err) {
                    if (err) {
                        console.log("Delete Error", err);
                    }
                })
            }
        });

        // let params = {};
        //
        // sqs.listQueues(params, function(err, data) {
        //     if (err) {
        //         console.log("Error", err);
        //     } else {
        //         console.log("Success", data.QueueUrls);
        //     }
        // });
    }

}

export default App;
