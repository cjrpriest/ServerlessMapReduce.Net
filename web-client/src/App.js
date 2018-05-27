import React, { Component } from 'react';
import logo from './logo.svg';
import './App.css';

import Button from 'react-md/lib/Buttons/Button';

import AWS from 'aws-sdk';
// import { Credentials } from 'aws-sdk/lib/credentials';

class App extends Component {

    queueUrlPrefix = "https://sqs.eu-west-1.amazonaws.com/525470265062/";
    queueUrlPostfix = ".fifo";
    region = "eu-west-1";
    endoint = "https://sqs.eu-west-1.amazonaws.com";

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
                  this.setState({intervalHandler: setInterval(() => this.getQueueInfo(this), 1000)});
                }
              }>Go!</Button>
              <Button raised onClick={() => clearInterval(this.state.intervalHandler)}>Stop!</Button>
          </div>
        );
      }

      getQueueUrl(queueName) {
        return this.queueUrlPrefix + queueName + this.queueUrlPostfix;
      }

    uuidv4() {
        return ([1e7]+-1e3+-4e3+-8e3+-1e11).replace(/[018]/g, c =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        )
    }

    getQueueInfo(self) {
        let sqs = new AWS.SQS({apiVersion: '2012-11-05'});//, endpoint: this.endoint});

        sqs.config.update({
            region: this.region,
            accessKeyId: 'AKIAIPV5N5QMERUWT2OQ',
            secretAccessKey: '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'});

        let remoteQueueUrl = self.getQueueUrl('serverless-mapreduce-remoteCommandQueue');

        //let self = this;
        sqs.getQueueAttributes({
            QueueUrl: remoteQueueUrl,
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
            QueueUrl: self.getQueueUrl('serverless-mapreduce-remoteCommandQueue'),
            VisibilityTimeout: 5,
            WaitTimeSeconds: 0
        };

        sqs.receiveMessage(params, function(err, data) {
            if (err) {
                console.log("Receive Error", err);
            } else if (data.Messages) {
                //console.log("Message", data.Messages[0]);
                if (!data.Messages[0]) return;

                // console.log(data.Messages[0].Body);
                let obj = JSON.parse(data.Messages[0].Body);
                console.log(obj);

                let mostAccidentProneKvps = [];
                for (let i = 0, len = obj.Command.Lines.$values.length; i < len; i++) {
                    let line = obj.Command.Lines.$values[i];
                    let lineValues = line.split(',');
                    if (lineValues.length > 20) {
                        // accident stat
                        let ageOfVehicleStr = lineValues[19];
                        let ageOfVehicle = parseInt(ageOfVehicleStr);
                        if (ageOfVehicle === 1) {
                            let manufacturer = lineValues[22].toUpperCase();
                            let mostAccidentProneKvp = {
                                $type: "ServerlessMapReduceDotNet.Model.MostAccidentProneKvp, ServerlessMapReduceDotNet",
                                Key: manufacturer,
                                Value: {
                                    NoOfAccidents: 1,
                                    NoOfCarsRegistered: 0,
                                    RegistrationsPerAccident: 0.0
                                }
                            };
                            mostAccidentProneKvps.push(mostAccidentProneKvp);
                        }
                    } else {
                        // registrations stat
                        let manufacturer = lineValues[0].toUpperCase();
                        let dirtyInt = lineValues[6];
                        let cleanInt = dirtyInt.replace(',','').replace('"','');
                        let noOfRegistrations = parseInt(cleanInt);
                        let mostAccidentProneKvp = {
                            $type: "ServerlessMapReduceDotNet.Model.MostAccidentProneKvp, ServerlessMapReduceDotNet",
                            Key: manufacturer,
                            Value: {
                                NoOfAccidents : 0,
                                NoOfCarsRegistered: noOfRegistrations,
                                RegistrationsPerAccident: 0.0
                            }
                        };
                        mostAccidentProneKvps.push(mostAccidentProneKvp);
                    }
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
                    QueueUrl: self.getQueueUrl('serverless-mapreduce-commandQueue'),
                    MessageBody: JSON.stringify(writeMappedDataCommand),
                    MessageGroupId: self.uuidv4(),
                    MessageDeduplicationId: self.uuidv4()
                }, function(err, data) {
                    if (err) {
                        console.log("Receive Error", err);
                    } else {
                        console.log(data);
                    }
                });

                let remoteQueue = self.getQueueUrl('serverless-mapreduce-remoteCommandQueue');

                sqs.deleteMessage({
                    QueueUrl: remoteQueue,
                    ReceiptHandle: data.Messages[0].ReceiptHandle
                }, function(err) {
                    if (err) {
                        console.log("Delete Error", err);
                    }
                })
            }
        });
        
    }

}

export default App;
